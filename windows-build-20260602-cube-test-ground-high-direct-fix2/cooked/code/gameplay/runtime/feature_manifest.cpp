#include "feature_manifest.hpp"

static const HEFeatureEntry kFeatureEntries[] = {
};

bool he_feature_enabled(HEFeature feature) {
    for (const HEFeatureEntry& entry : kFeatureEntries) {
        if (entry.Feature == feature) {
            return entry.Enabled;
        }
    }

    return false;
}

const HEFeatureEntry* he_get_feature_entries(std::size_t* count) {
    if (count != nullptr) {
        *count = sizeof(kFeatureEntries) / sizeof(kFeatureEntries[0]);
    }

    return kFeatureEntries;
}
