using System;
using helengine;
using city.rendering;

namespace city.tests {
    /// <summary>
    /// Verifies the scalar software tracing intersection contract.
    /// </summary>
    public sealed class SoftwareIntersectionTests {
        /// <summary>
        /// Ensures a front-facing ray intersects the triangle without back-face culling.
        /// </summary>
        [Fact]
        public void Front_and_back_facing_rays_hit() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay frontRay = new SoftwareRay(new float3(0.25f, 0.25f, 1f), new float3(0f, 0f, -1f));
            SoftwareRay backRay = new SoftwareRay(new float3(0.25f, 0.25f, -1f), new float3(0f, 0f, 1f));

            Assert.True(city.rendering.SoftwareBvh.IntersectTriangle(ref frontRay, ref triangle, 0f, 10f, out SoftwareHit frontHit));
            Assert.True(city.rendering.SoftwareBvh.IntersectTriangle(ref backRay, ref triangle, 0f, 10f, out SoftwareHit backHit));
            Assert.Equal(1f, frontHit.Distance, precision: 5);
            Assert.Equal(1f, backHit.Distance, precision: 5);
            Assert.Equal(0.25f, frontHit.U, precision: 5);
            Assert.Equal(0.25f, frontHit.V, precision: 5);
        }

        /// <summary>
        /// Ensures a ray that lands outside the triangle is rejected.
        /// </summary>
        [Fact]
        public void Ray_outside_triangle_misses() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay ray = new SoftwareRay(new float3(1.1f, 0.1f, 1f), new float3(0f, 0f, -1f));

            Assert.False(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 10f, out SoftwareHit hit));
            Assert.Equal(default, hit);
        }

        /// <summary>
        /// Ensures the shared diagonal edge is accepted within the finite barycentric tolerance.
        /// </summary>
        [Fact]
        public void Barycentric_edge_hit_is_inclusive() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay ray = new SoftwareRay(new float3(0.5f, 0.5f, 1f), new float3(0f, 0f, -1f));

            Assert.True(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 10f, out SoftwareHit hit));
            Assert.Equal(0.5f, hit.U, precision: 5);
            Assert.Equal(0.5f, hit.V, precision: 5);
        }

        /// <summary>
        /// Ensures only the positive portion of the caller-supplied distance range is accepted.
        /// </summary>
        [Fact]
        public void Triangle_hit_respects_positive_nearest_and_range_distances() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 2f), new float3(0f, 0f, -1f));
            SoftwareRay awayRay = new SoftwareRay(new float3(0.25f, 0.25f, -1f), new float3(0f, 0f, -1f));

            Assert.True(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 10f, out SoftwareHit hit));
            Assert.Equal(2f, hit.Distance, precision: 5);
            Assert.True(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 2f, 2f, out _));
            Assert.False(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 2.001f, 10f, out _));
            Assert.False(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 1.999f, out _));
            Assert.False(city.rendering.SoftwareBvh.IntersectTriangle(ref awayRay, ref triangle, 0f, 10f, out _));
        }

        /// <summary>
        /// Ensures a ray parallel to the triangle plane is rejected.
        /// </summary>
        [Fact]
        public void Parallel_ray_misses_triangle() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 1f), new float3(1f, 0f, 0f));

            Assert.False(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 10f, out _));
        }

        /// <summary>
        /// Ensures an AABB contains a ray origin inside it and reports forward entry/exit range correctly.
        /// </summary>
        [Fact]
        public void Bounds_hit_accepts_origin_inside_box() {
            SoftwareBounds bounds = new SoftwareBounds(new float3(0f, 0f, 0f), new float3(1f, 1f, 1f));
            SoftwareRay ray = new SoftwareRay(new float3(0.5f, 0.5f, 0.5f), new float3(1f, 0f, 0f));
            SoftwareRay outsideRay = new SoftwareRay(new float3(-1f, 0.5f, 0.5f), new float3(1f, 0f, 0f));

            Assert.True(city.rendering.SoftwareBvh.IntersectBounds(ref ray, ref bounds, 0.5f));
            Assert.False(city.rendering.SoftwareBvh.IntersectBounds(ref outsideRay, ref bounds, 0.99f));
        }

        /// <summary>
        /// Ensures zero direction components use slab containment rather than dividing by zero.
        /// </summary>
        [Fact]
        public void Bounds_hit_handles_zero_direction_components() {
            SoftwareBounds bounds = new SoftwareBounds(new float3(0f, 0f, 0f), new float3(1f, 1f, 1f));
            SoftwareRay ray = new SoftwareRay(new float3(0.5f, -1f, 0.5f), new float3(0f, 1f, 0f));

            Assert.True(city.rendering.SoftwareBvh.IntersectBounds(ref ray, ref bounds, 2f));
            Assert.False(city.rendering.SoftwareBvh.IntersectBounds(ref ray, ref bounds, 0.99f));
        }

        /// <summary>
        /// Ensures a ray tangent to one slab boundary is considered an intersection.
        /// </summary>
        [Fact]
        public void Bounds_hit_accepts_grazing_slab() {
            SoftwareBounds bounds = new SoftwareBounds(new float3(0f, 0f, 0f), new float3(1f, 1f, 1f));
            SoftwareRay ray = new SoftwareRay(new float3(-1f, 0f, 0.5f), new float3(1f, 0f, 0f));

            Assert.True(city.rendering.SoftwareBvh.IntersectBounds(ref ray, ref bounds, 2f));
        }

        /// <summary>
        /// Ensures a finite near-parallel determinant is rejected by the intersection epsilon.
        /// </summary>
        [Fact]
        public void Triangle_intersection_uses_finite_parallel_epsilon() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 1f), new float3(1f, 0f, 0.00000001f));

            Assert.False(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 1000000f, out _));
        }

        /// <summary>
        /// Ensures successful intersections return finite distance and barycentric outputs.
        /// </summary>
        [Fact]
        public void Triangle_intersection_outputs_finite_hit_values() {
            SoftwareTriangle triangle = CreateTriangle();
            SoftwareRay ray = new SoftwareRay(new float3(0.2f, 0.3f, 1f), new float3(0f, 0f, -1f));

            Assert.True(city.rendering.SoftwareBvh.IntersectTriangle(ref ray, ref triangle, 0f, 10f, out SoftwareHit hit));
            Assert.True(float.IsFinite(hit.Distance));
            Assert.True(float.IsFinite(hit.U));
            Assert.True(float.IsFinite(hit.V));
            Assert.True(float.IsFinite(hit.Position.X));
            Assert.True(float.IsFinite(hit.Position.Y));
            Assert.True(float.IsFinite(hit.Position.Z));
        }

        static SoftwareTriangle CreateTriangle() {
            return new SoftwareTriangle(
                new float3(0f, 0f, 0f),
                new float3(1f, 0f, 0f),
                new float3(0f, 1f, 0f),
                new float3(0f, 0f, 1f),
                0,
                new float3(1f / 3f, 1f / 3f, 0f),
                new float3(0f, 0f, 0f),
                new float3(1f, 1f, 0f));
        }
    }
}
