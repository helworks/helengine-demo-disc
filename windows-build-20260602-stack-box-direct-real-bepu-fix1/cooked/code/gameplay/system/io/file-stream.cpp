#include "file-stream.hpp"
#include <stdexcept>  // For exceptions
#include <cstring>    // For std::memcpy
#include <sys/stat.h> // For file size retrieval
#if defined(_WIN32)
#include <io.h>
#else
#include <unistd.h>
#endif

// Helper function to get file mode as C-style string
const char* GetFileMode(FileMode mode) {
    switch (mode) {
    case FileMode::Append: return "a+b";
    case FileMode::Create: return "w+b";
    case FileMode::CreateNew: return "wbx+";
    case FileMode::Open: return "rb";
    case FileMode::OpenOrCreate: return "r+b";
    case FileMode::Truncate: return "wb";
    default: throw std::runtime_error("Invalid FileMode");
    }
}

// Constructor
FileStream::FileStream(const uint8_t* data, size_t dataLength)
    : file(nullptr), memoryBuffer(), position(0), length(0), ownsMemoryBuffer(true), writable(false) {
    if (data == nullptr && dataLength > 0) {
        throw std::runtime_error("Cannot create a memory-backed file stream from a null buffer.");
    }

    memoryBuffer.assign(data, data + dataLength);
    length = memoryBuffer.size();
}

FileStream::FileStream(const char* path, FileMode mode)
    : file(nullptr), memoryBuffer(), position(0), length(0), ownsMemoryBuffer(false), writable(true) {
    file = std::fopen(path, GetFileMode(mode));
    if (!file) {
        throw std::runtime_error(std::string("Failed to open file: ") + path);
    }

    UpdateLength();
}

FileStream::FileStream(const char* path, FileMode mode, FileAccess, FileShare)
    : FileStream(path, mode) {
}

FileStream::FileStream(const std::string& path, FileMode mode)
    : FileStream(path.c_str(), mode) {
}

FileStream::FileStream(const std::string& path, FileMode mode, FileAccess access, FileShare share)
    : FileStream(path.c_str(), mode, access, share) {
}

// Destructor
FileStream::~FileStream() {
    Close();
}

// Reads data from file
size_t FileStream::Read(uint8_t* buffer, size_t offset, size_t count) {
    if (!CanRead() || !buffer) return 0;

    if (file == nullptr) {
        size_t available = position >= memoryBuffer.size() ? 0 : memoryBuffer.size() - position;
        size_t bytesRead = std::min(count, available);
        if (bytesRead == 0) {
            return 0;
        }

        std::memcpy(buffer + offset, memoryBuffer.data() + position, bytesRead);
        position += bytesRead;
        return bytesRead;
    }

    std::fseek(file, position, SEEK_SET);

    size_t bytesRead = std::fread(buffer + offset, 1, count, file);
    position += bytesRead;
    return bytesRead;
}

// Writes data to file
void FileStream::Write(const uint8_t* buffer, size_t offset, size_t count) {
    if (!CanWrite() || !buffer) return;

    if (file == nullptr) {
        size_t requiredLength = position + count;
        if (requiredLength > memoryBuffer.size()) {
            memoryBuffer.resize(requiredLength);
        }

        std::memcpy(memoryBuffer.data() + position, buffer + offset, count);
        position += count;
        length = memoryBuffer.size();
        return;
    }

    std::fseek(file, position, SEEK_SET);

    size_t bytesWritten = std::fwrite(buffer + offset, 1, count, file);
    position += bytesWritten;
    UpdateLength();
}

// Seeks to a position in file
size_t FileStream::Seek(int64_t offset, SeekOrigin origin) {
    if (!CanSeek()) return position;

    if (file == nullptr) {
        int64_t basePosition = 0;
        switch (origin) {
        case SeekOrigin::Begin: basePosition = 0; break;
        case SeekOrigin::Current: basePosition = static_cast<int64_t>(position); break;
        case SeekOrigin::End: basePosition = static_cast<int64_t>(length); break;
        }

        int64_t nextPosition = basePosition + offset;
        if (nextPosition < 0) {
            nextPosition = 0;
        } else if (static_cast<size_t>(nextPosition) > length) {
            nextPosition = static_cast<int64_t>(length);
        }

        position = static_cast<size_t>(nextPosition);
        return position;
    }

    int seekMode;
    switch (origin) {
    case SeekOrigin::Begin: seekMode = SEEK_SET; break;
    case SeekOrigin::Current: seekMode = SEEK_CUR; break;
    case SeekOrigin::End: seekMode = SEEK_END; break;
    }

    std::fseek(file, offset, seekMode);
    position = std::ftell(file);
    return position;
}

// Truncates or extends the file
void FileStream::SetLength(size_t newLength) {
    if (file == nullptr) {
        if (!writable) {
            return;
        }

        memoryBuffer.resize(newLength);
        length = memoryBuffer.size();
        if (position > length) {
            position = length;
        }
        return;
    }

    std::fflush(file);
#if defined(_WIN32)
    _chsize_s(fileno(file), newLength);
#else
    ftruncate(fileno(file), newLength);
#endif
    UpdateLength();
}

// Updates the stored file length
void FileStream::UpdateLength() {
    if (!file) {
        length = memoryBuffer.size();
        return;
    }

    struct stat fileStat;
    if (fstat(fileno(file), &fileStat) == 0) {
        length = fileStat.st_size;
    }
}

// Properties
bool FileStream::CanRead() const { return file != nullptr || ownsMemoryBuffer; }
bool FileStream::CanWrite() const { return file != nullptr || (ownsMemoryBuffer && writable); }
bool FileStream::CanSeek() const { return file != nullptr || ownsMemoryBuffer; }

size_t FileStream::Length() const { return length; }
size_t FileStream::Position() const { return position; }
void FileStream::SetPosition(size_t value) { position = std::min(value, length); }

// Internal byte-level operations
void FileStream::InternalReserve(size_t count) { /* Not needed for file streams */ }

void FileStream::InternalWriteByte(uint8_t byte) {
    Write(&byte, 0, 1);
}

int FileStream::InternalReadByte() {
    uint8_t byte;
    return (Read(&byte, 0, 1) > 0) ? byte : -1;
}

// Flushes the file buffer
void FileStream::Flush() {
    if (file) std::fflush(file);
}

// Closes the file
void FileStream::Close() {
    if (file) {
        std::fclose(file);
        file = nullptr;
    }

    if (ownsMemoryBuffer) {
        memoryBuffer.clear();
        memoryBuffer.shrink_to_fit();
        ownsMemoryBuffer = false;
    }
}

// Cleanup function
void FileStream::Dispose() {
    Close();
}
