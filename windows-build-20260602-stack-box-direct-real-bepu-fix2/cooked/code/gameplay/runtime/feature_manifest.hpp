#pragma once

#include <cstddef>

enum class HEFeature {
};

enum class HEFeatureDecisionOrigin {
    ForcedEnabled,
    ForcedDisabled,
    AutoDetected,
    NotIncluded
};

struct HEFeatureEntry {
    HEFeature Feature;
    bool Enabled;
    HEFeatureDecisionOrigin Origin;
    const char* Name;
};

bool he_feature_enabled(HEFeature feature);
const HEFeatureEntry* he_get_feature_entries(std::size_t* count);