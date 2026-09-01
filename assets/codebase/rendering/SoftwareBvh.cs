using System;
using helengine;

namespace city.rendering {
    /// <summary>
    /// Stores a world-space ray for scalar software tracing.
    /// </summary>
    public readonly struct SoftwareRay {
        /// <summary>
        /// Gets the world-space ray origin.
        /// </summary>
        public float3 Origin { get; }

        /// <summary>
        /// Gets the world-space ray direction, which is not normalized by this type.
        /// </summary>
        public float3 Direction { get; }

        /// <summary>
        /// Initializes one scalar software ray.
        /// </summary>
        /// <param name="origin">World-space ray origin.</param>
        /// <param name="direction">World-space ray direction.</param>
        public SoftwareRay(float3 origin, float3 direction) {
            Origin = origin;
            Direction = direction;
        }
    }

    /// <summary>
    /// Stores the scalar result of a software triangle intersection.
    /// </summary>
    public readonly struct SoftwareHit {
        /// <summary>
        /// Gets the ray distance at the intersection.
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// Gets the first barycentric coordinate associated with the triangle edge from P0 to P1.
        /// </summary>
        public float U { get; }

        /// <summary>
        /// Gets the second barycentric coordinate associated with the triangle edge from P0 to P2.
        /// </summary>
        public float V { get; }

        /// <summary>
        /// Gets the world-space hit position.
        /// </summary>
        public float3 Position { get; }

        /// <summary>
        /// Gets the triangle's geometric normal without changing its winding orientation.
        /// </summary>
        public float3 GeometricNormal { get; }

        /// <summary>
        /// Gets the first barycentric coordinate using the descriptive property name.
        /// </summary>
        public float BarycentricU => U;

        /// <summary>
        /// Gets the second barycentric coordinate using the descriptive property name.
        /// </summary>
        public float BarycentricV => V;

        /// <summary>
        /// Gets the geometric normal using the short property name used by shading callers.
        /// </summary>
        public float3 Normal => GeometricNormal;

        /// <summary>
        /// Gets all three barycentric coordinates in P0, P1, P2 order.
        /// </summary>
        public float3 Barycentric => new float3(1f - U - V, U, V);

        /// <summary>
        /// Initializes one scalar software hit.
        /// </summary>
        /// <param name="distance">Ray distance at the hit.</param>
        /// <param name="u">Barycentric coordinate for Edge1.</param>
        /// <param name="v">Barycentric coordinate for Edge2.</param>
        /// <param name="position">World-space position at the hit.</param>
        /// <param name="geometricNormal">Triangle geometric normal.</param>
        public SoftwareHit(float distance, float u, float v, float3 position, float3 geometricNormal) {
            Distance = distance;
            U = u;
            V = v;
            Position = position;
            GeometricNormal = geometricNormal;
        }
    }

    /// <summary>
    /// Stores an axis-aligned world-space bounding box for scalar software tracing.
    /// </summary>
    public readonly struct SoftwareBounds {
        /// <summary>
        /// Gets the minimum corner of the bounds.
        /// </summary>
        public float3 Min { get; }

        /// <summary>
        /// Gets the maximum corner of the bounds.
        /// </summary>
        public float3 Max { get; }

        /// <summary>
        /// Gets the minimum corner using the descriptive property name.
        /// </summary>
        public float3 Minimum => Min;

        /// <summary>
        /// Gets the maximum corner using the descriptive property name.
        /// </summary>
        public float3 Maximum => Max;

        /// <summary>
        /// Gets the minimum corner using the trace-scene bounds naming convention.
        /// </summary>
        public float3 BoundsMin => Min;

        /// <summary>
        /// Gets the maximum corner using the trace-scene bounds naming convention.
        /// </summary>
        public float3 BoundsMax => Max;

        /// <summary>
        /// Initializes one axis-aligned software bounds.
        /// </summary>
        /// <param name="min">Minimum corner.</param>
        /// <param name="max">Maximum corner.</param>
        public SoftwareBounds(float3 min, float3 max) {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Provides scalar, allocation-free intersection primitives for the software BVH.
    /// </summary>
    public sealed class SoftwareBvh {
        /// <summary>
        /// Tolerance used when a Moller-Trumbore determinant is close to parallel.
        /// </summary>
        const float DeterminantEpsilon = 0.0000001f;

        /// <summary>
        /// Tolerance used to keep numerically exact triangle edges inclusive.
        /// </summary>
        const float BarycentricEpsilon = 0.000001f;

        /// <summary>
        /// Tolerance used for a ray that grazes a bounds slab.
        /// </summary>
        const float BoundsEpsilon = 0.000001f;

        /// <summary>
        /// Intersects a scalar ray with a world-space triangle using Moller-Trumbore arithmetic.
        /// </summary>
        /// <param name="ray">Ray whose direction is used as supplied; it is not normalized.</param>
        /// <param name="triangle">Triangle to test.</param>
        /// <param name="minimumDistance">Inclusive lower distance bound.</param>
        /// <param name="maximumDistance">Inclusive upper distance bound.</param>
        /// <param name="hit">Intersection details when the method returns true; default otherwise.</param>
        /// <returns>True when the ray intersects the triangle inside the supplied forward distance range.</returns>
        public static bool IntersectTriangle(ref SoftwareRay ray, ref SoftwareTriangle triangle, float minimumDistance, float maximumDistance, out SoftwareHit hit) {
            hit = default;
            if (float.IsNaN(minimumDistance) || float.IsNaN(maximumDistance) || minimumDistance > maximumDistance || maximumDistance < 0f) {
                return false;
            }

            float3 pVector = float3.Cross(ray.Direction, triangle.Edge2);
            float determinant = float3.Dot(triangle.Edge1, pVector);
            if (Math.Abs(determinant) <= DeterminantEpsilon) {
                return false;
            }

            float inverseDeterminant = 1f / determinant;
            float3 originToCorner = ray.Origin - triangle.P0;
            float u = float3.Dot(originToCorner, pVector) * inverseDeterminant;
            if (u < -BarycentricEpsilon || u > 1f + BarycentricEpsilon) {
                return false;
            }

            float3 qVector = float3.Cross(originToCorner, triangle.Edge1);
            float v = float3.Dot(ray.Direction, qVector) * inverseDeterminant;
            if (v < -BarycentricEpsilon || u + v > 1f + BarycentricEpsilon) {
                return false;
            }

            float distance = float3.Dot(triangle.Edge2, qVector) * inverseDeterminant;
            if (distance < 0f || distance < minimumDistance || distance > maximumDistance || !float.IsFinite(distance)) {
                return false;
            }

            float3 position = ray.Origin + (ray.Direction * distance);
            hit = new SoftwareHit(distance, u, v, position, triangle.GeometricNormal);
            return true;
        }

        /// <summary>
        /// Intersects a scalar ray with an axis-aligned bounding box using robust slab intervals.
        /// </summary>
        /// <param name="ray">Ray whose direction is used as supplied; it is not normalized.</param>
        /// <param name="bounds">Axis-aligned bounds to test.</param>
        /// <param name="maximumDistance">Inclusive upper distance bound.</param>
        /// <returns>True when the forward ray enters the bounds before the supplied distance.</returns>
        public static bool IntersectBounds(ref SoftwareRay ray, ref SoftwareBounds bounds, float maximumDistance) {
            if (float.IsNaN(maximumDistance) || maximumDistance < 0f || bounds.Min.X > bounds.Max.X || bounds.Min.Y > bounds.Max.Y || bounds.Min.Z > bounds.Max.Z) {
                return false;
            }

            float nearDistance = 0f;
            float farDistance = maximumDistance;
            if (!IntersectSlab(ray.Origin.X, ray.Direction.X, bounds.Min.X, bounds.Max.X, ref nearDistance, ref farDistance) ||
                !IntersectSlab(ray.Origin.Y, ray.Direction.Y, bounds.Min.Y, bounds.Max.Y, ref nearDistance, ref farDistance) ||
                !IntersectSlab(ray.Origin.Z, ray.Direction.Z, bounds.Min.Z, bounds.Max.Z, ref nearDistance, ref farDistance)) {
                return false;
            }

            return nearDistance <= farDistance + BoundsEpsilon && farDistance >= -BoundsEpsilon;
        }

        /// <summary>
        /// Intersects one scalar ray component with one bounds slab and narrows its interval in place.
        /// </summary>
        /// <param name="origin">Ray origin component.</param>
        /// <param name="direction">Ray direction component.</param>
        /// <param name="minimum">Slab minimum.</param>
        /// <param name="maximum">Slab maximum.</param>
        /// <param name="nearDistance">Current near interval, updated on success.</param>
        /// <param name="farDistance">Current far interval, updated on success.</param>
        /// <returns>True when this slab overlaps the current interval.</returns>
        static bool IntersectSlab(float origin, float direction, float minimum, float maximum, ref float nearDistance, ref float farDistance) {
            if (direction == 0f) {
                return origin >= minimum - BoundsEpsilon && origin <= maximum + BoundsEpsilon;
            }

            float inverseDirection = 1f / direction;
            float firstDistance = (minimum - origin) * inverseDirection;
            float secondDistance = (maximum - origin) * inverseDirection;
            if (firstDistance > secondDistance) {
                float swap = firstDistance;
                firstDistance = secondDistance;
                secondDistance = swap;
            }

            if (firstDistance > nearDistance) {
                nearDistance = firstDistance;
            }
            if (secondDistance < farDistance) {
                farDistance = secondDistance;
            }
            return nearDistance <= farDistance + BoundsEpsilon;
        }

    }
}
