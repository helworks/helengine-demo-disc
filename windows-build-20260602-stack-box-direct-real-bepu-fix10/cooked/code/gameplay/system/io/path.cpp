#include "path.hpp"

#include "helcpp_config.hpp"

#include <algorithm>
#include <filesystem>

#if HE_CPP_PLATFORM_PS2
namespace {
    bool IsPs2DevicePath(const std::string& path) {
        return path.rfind("cdrom0:", 0) == 0
            || path.rfind("host:", 0) == 0
            || path.rfind("mc0:", 0) == 0
            || path.rfind("mc1:", 0) == 0
            || path.rfind("mass:", 0) == 0;
    }

    std::string NormalizePs2Path(const std::string& path) {
        if (path.empty()) {
            return path;
        }

        std::string normalized = path;
        std::replace(normalized.begin(), normalized.end(), '/', '\\');
        const std::size_t deviceSeparatorIndex = normalized.find(':');
        if (deviceSeparatorIndex == std::string::npos) {
            return normalized;
        }

        std::string prefix = normalized.substr(0, deviceSeparatorIndex + 1);
        std::string suffix = normalized.substr(deviceSeparatorIndex + 1);
        while (!suffix.empty() && suffix.front() == '\\') {
            suffix.erase(suffix.begin());
        }

        std::string collapsedSuffix;
        bool previousWasSeparator = false;
        for (char character : suffix) {
            if (character == '\\') {
                if (!previousWasSeparator) {
                    collapsedSuffix.push_back(character);
                }

                previousWasSeparator = true;
                continue;
            }

            collapsedSuffix.push_back(character);
            previousWasSeparator = false;
        }

        if (collapsedSuffix.empty()) {
            return prefix + "\\";
        }

        return prefix + "\\" + collapsedSuffix;
    }

    std::string CombinePs2Path(const std::string& left, const std::string& right) {
        if (left.empty()) {
            return NormalizePs2Path(right);
        }

        if (right.empty()) {
            return NormalizePs2Path(left);
        }

        if (IsPs2DevicePath(right)) {
            return NormalizePs2Path(right);
        }

        std::string normalizedLeft = NormalizePs2Path(left);
        std::string normalizedRight = NormalizePs2Path(right);
        while (!normalizedRight.empty() && normalizedRight.front() == '\\') {
            normalizedRight.erase(normalizedRight.begin());
        }

        if (!normalizedLeft.empty() && normalizedLeft.back() != '\\') {
            normalizedLeft.push_back('\\');
        }

        return normalizedLeft + normalizedRight;
    }

    std::string GetPs2DirectoryName(const std::string& path) {
        std::string normalized = NormalizePs2Path(path);
        std::size_t separatorIndex = normalized.find_last_of("\\/");
        if (separatorIndex == std::string::npos) {
            return std::string();
        }

        if (separatorIndex > 0 && normalized[separatorIndex - 1] == ':') {
            return normalized.substr(0, separatorIndex + 1);
        }

        return normalized.substr(0, separatorIndex);
    }

    std::string GetPs2FileName(const std::string& path) {
        std::string normalized = NormalizePs2Path(path);
        std::size_t separatorIndex = normalized.find_last_of("\\/");
        std::string fileName = separatorIndex == std::string::npos ? normalized : normalized.substr(separatorIndex + 1);
        std::size_t versionSeparatorIndex = fileName.find(';');
        if (versionSeparatorIndex != std::string::npos) {
            fileName = fileName.substr(0, versionSeparatorIndex);
        }

        return fileName;
    }
}
#endif

#if HELENGINE_NINTENDO_DS_HAS_GENERATED_CORE
namespace {
    bool IsNintendoDsDevicePath(const std::string& path) {
        return path.rfind("nitro:", 0) == 0;
    }
}
#endif

std::string Path::Combine(const std::string& left, const std::string& right) {
#if HE_CPP_PLATFORM_PS2
    if (IsPs2DevicePath(left) || IsPs2DevicePath(right)) {
        return CombinePs2Path(left, right);
    }
#endif
#if HELENGINE_NINTENDO_DS_HAS_GENERATED_CORE
    if (IsNintendoDsDevicePath(left)) {
        if (right.empty()) {
            return left;
        }

        if (right[0] == '/') {
            return left + right;
        }

        return left + "/" + right;
    }
#endif
    if (left.empty()) {
        return right;
    }

    if (right.empty()) {
        return left;
    }

    return (std::filesystem::path(left) / right).lexically_normal().string();
}

std::string Path::Combine(const std::string& first, const std::string& second, const std::string& third) {
    return Combine(Combine(first, second), third);
}

std::string Path::GetDirectoryName(const std::string& path) {
    if (path.empty()) {
        return std::string();
    }

#if HE_CPP_PLATFORM_PS2
    if (IsPs2DevicePath(path)) {
        return GetPs2DirectoryName(path);
    }
#endif
    return std::filesystem::path(path).parent_path().string();
}

std::string Path::GetFileName(const std::string& path) {
    if (path.empty()) {
        return std::string();
    }

#if HE_CPP_PLATFORM_PS2
    if (IsPs2DevicePath(path)) {
        return GetPs2FileName(path);
    }
#endif
    return std::filesystem::path(path).filename().string();
}

std::string Path::GetFullPath(const std::string& path) {
#if HELENGINE_NINTENDO_DS_HAS_GENERATED_CORE
    if (IsNintendoDsDevicePath(path)) {
        return path;
    }
#endif
#if !HE_CPP_PLATFORM_IS_WINDOWS_HOST
    if (path.empty()) {
        return std::string(".");
    }

#if HE_CPP_PLATFORM_PS2
    if (IsPs2DevicePath(path)) {
        return NormalizePs2Path(path);
    }
#endif
    return std::filesystem::path(path).lexically_normal().string();
#else
    if (path.empty()) {
        return std::filesystem::current_path().string();
    }

    return std::filesystem::absolute(std::filesystem::path(path)).lexically_normal().string();
#endif
}

std::string Path::ChangeExtension(const std::string& path, const std::string& extension) {
    if (path.empty()) {
        return std::string();
    }

    std::filesystem::path updatedPath(path);
    updatedPath.replace_extension(extension);
    return updatedPath.string();
}

bool Path::IsPathRooted(const std::string& path) {
    if (path.empty()) {
        return false;
    }

#if HE_CPP_PLATFORM_PS2
    if (IsPs2DevicePath(path)) {
        return true;
    }
#endif
#if HELENGINE_NINTENDO_DS_HAS_GENERATED_CORE
    if (IsNintendoDsDevicePath(path)) {
        return true;
    }
#endif
    return std::filesystem::path(path).is_absolute();
}
