using System;
using System.Reflection;
using helengine;
using city.rendering;

namespace city.tests {
    /// <summary>
    /// Verifies the deterministic bounded software BVH contract.
    /// </summary>
    public sealed class SoftwareBvhTests {
        /// <summary>
        /// Ensures null triangle input is rejected before a root is created.
        /// </summary>
        [Fact]
        public void Build_rejects_null_triangles() {
            Assert.Throws<ArgumentNullException>(() => SoftwareBvh.Build(null));
        }

        /// <summary>
        /// Ensures an empty triangle input is rejected instead of creating an empty root.
        /// </summary>
        [Fact]
        public void Build_rejects_empty_triangles() {
            Assert.Throws<ArgumentException>(() => SoftwareBvh.Build(Array.Empty<SoftwareTriangle>()));
        }

        /// <summary>
        /// Ensures one through four triangles remain in one compact leaf.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void One_through_four_triangles_produce_one_leaf(int triangleCount) {
            SoftwareTriangle[] triangles = CreateLineTriangles(triangleCount);
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Single(bvh.Nodes);
            Assert.True(bvh.Nodes[0].IsLeaf);
            Assert.Equal(0, bvh.Nodes[0].FirstIndex);
            Assert.Equal(triangleCount, bvh.Nodes[0].Count);
            Assert.Equal(0, bvh.MaximumDepth);
        }

        /// <summary>
        /// Ensures five triangles split at the median of their largest centroid extent.
        /// </summary>
        [Fact]
        public void Five_triangles_split_on_largest_centroid_axis() {
            SoftwareTriangle[] triangles = CreateCentroidTriangles(
                new float3(-4f, 0f, 0f),
                new float3(-2f, 0.1f, 0f),
                new float3(0f, -0.1f, 0f),
                new float3(2f, 0.2f, 0f),
                new float3(4f, -0.2f, 0f));

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.False(bvh.Nodes[0].IsLeaf);
            Assert.Equal(1, bvh.Nodes[0].FirstIndex);
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, bvh.TriangleOrder);
            Assert.Equal(2, bvh.Nodes[1].Count);
            Assert.Equal(3, bvh.Nodes[2].Count);
        }

        /// <summary>
        /// Ensures equal largest centroid extents choose X before Y.
        /// </summary>
        [Fact]
        public void Axis_extent_tie_prefers_x_before_y() {
            SoftwareTriangle[] triangles = CreateCentroidTriangles(
                new float3(4f, 0f, 0f),
                new float3(3f, 1f, 0f),
                new float3(2f, 2f, 0f),
                new float3(1f, 3f, 0f),
                new float3(0f, 4f, 0f));

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Equal(new[] { 4, 3, 2, 1, 0 }, bvh.TriangleOrder);
        }

        /// <summary>
        /// Ensures equal largest centroid extents choose Y before Z.
        /// </summary>
        [Fact]
        public void Axis_extent_tie_prefers_y_before_z() {
            SoftwareTriangle[] triangles = CreateCentroidTriangles(
                new float3(0f, 4f, 0f),
                new float3(0f, 3f, 1f),
                new float3(0f, 2f, 2f),
                new float3(0f, 1f, 3f),
                new float3(0f, 0f, 4f));

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Equal(new[] { 4, 3, 2, 1, 0 }, bvh.TriangleOrder);
        }

        /// <summary>
        /// Ensures equal centroid positions resolve by original triangle index.
        /// </summary>
        [Fact]
        public void Equal_centroids_resolve_by_original_index() {
            SoftwareTriangle[] triangles = CreateCentroidTriangles(
                new float3(2f, 2f, 2f),
                new float3(2f, 2f, 2f),
                new float3(2f, 2f, 2f),
                new float3(2f, 2f, 2f),
                new float3(2f, 2f, 2f),
                new float3(2f, 2f, 2f));

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, bvh.TriangleOrder);
        }

        /// <summary>
        /// Ensures repeated builds have identical compact nodes and order arrays.
        /// </summary>
        [Fact]
        public void Repeated_builds_are_bytewise_deterministic_in_fields() {
            SoftwareTriangle[] triangles = CreateCentroidTriangles(
                new float3(3f, 1f, 2f),
                new float3(-2f, 2f, 1f),
                new float3(0f, -4f, 3f),
                new float3(5f, 0f, -1f),
                new float3(-1f, 3f, 0f),
                new float3(2f, -1f, 4f),
                new float3(4f, 4f, 2f),
                new float3(-3f, 0f, 1f),
                new float3(1f, 5f, -2f));

            using SoftwareBvh first = SoftwareBvh.Build(triangles);
            using SoftwareBvh second = SoftwareBvh.Build(triangles);

            Assert.Equal(first.TriangleOrder, second.TriangleOrder);
            Assert.Equal(first.Nodes.Length, second.Nodes.Length);
            Assert.Equal(first.MaximumDepth, second.MaximumDepth);
            for (int i = 0; i < first.Nodes.Length; i++) {
                Assert.Equal(first.Nodes[i].Bounds.Min, second.Nodes[i].Bounds.Min);
                Assert.Equal(first.Nodes[i].Bounds.Max, second.Nodes[i].Bounds.Max);
                Assert.Equal(first.Nodes[i].FirstIndex, second.Nodes[i].FirstIndex);
                Assert.Equal(first.Nodes[i].Count, second.Nodes[i].Count);
                Assert.Equal(first.Nodes[i].IsLeaf, second.Nodes[i].IsLeaf);
            }
        }

        /// <summary>
        /// Ensures every original triangle occurs exactly once and every leaf range is contiguous and bounded.
        /// </summary>
        [Fact]
        public void Every_triangle_occurs_once_and_leaf_ranges_are_bounded() {
            SoftwareTriangle[] triangles = CreateLineTriangles(37);
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            bool[] seenTriangles = new bool[triangles.Length];
            bool[] seenOrderSlots = new bool[bvh.TriangleOrder.Length];

            for (int nodeIndex = 0; nodeIndex < bvh.Nodes.Length; nodeIndex++) {
                SoftwareBvhNode node = bvh.Nodes[nodeIndex];
                if (!node.IsLeaf) {
                    Assert.InRange(node.FirstIndex, 0, bvh.Nodes.Length - 2);
                    continue;
                }

                Assert.InRange(node.Count, 1, SoftwareBvh.LeafTriangleCapacity);
                Assert.InRange(node.FirstIndex, 0, bvh.TriangleOrder.Length - node.Count);
                for (int orderSlot = node.FirstIndex; orderSlot < node.FirstIndex + node.Count; orderSlot++) {
                    Assert.False(seenOrderSlots[orderSlot]);
                    seenOrderSlots[orderSlot] = true;
                    int originalIndex = bvh.TriangleOrder[orderSlot];
                    Assert.InRange(originalIndex, 0, triangles.Length - 1);
                    Assert.False(seenTriangles[originalIndex]);
                    seenTriangles[originalIndex] = true;
                }
            }

            for (int i = 0; i < seenTriangles.Length; i++) {
                Assert.True(seenTriangles[i]);
            }
            for (int i = 0; i < seenOrderSlots.Length; i++) {
                Assert.True(seenOrderSlots[i]);
            }
        }

        /// <summary>
        /// Ensures every interior node bounds both children and every leaf bounds its triangles.
        /// </summary>
        [Fact]
        public void Parent_bounds_contain_children_and_leaf_triangles() {
            SoftwareTriangle[] triangles = CreateCentroidTriangles(
                new float3(-5f, -3f, -1f),
                new float3(-4f, 2f, 0f),
                new float3(-1f, 4f, 1f),
                new float3(0f, -2f, 2f),
                new float3(2f, 0f, -2f),
                new float3(4f, 3f, 3f),
                new float3(6f, -4f, 4f),
                new float3(8f, 1f, 5f),
                new float3(10f, 5f, 6f),
                new float3(12f, -5f, 7f),
                new float3(14f, 2f, 8f));
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            for (int nodeIndex = 0; nodeIndex < bvh.Nodes.Length; nodeIndex++) {
                SoftwareBvhNode node = bvh.Nodes[nodeIndex];
                if (!node.IsLeaf) {
                    SoftwareBvhNode left = bvh.Nodes[node.FirstIndex];
                    SoftwareBvhNode right = bvh.Nodes[node.FirstIndex + 1];
                    AssertBoundsContains(node.Bounds, left.Bounds.Min);
                    AssertBoundsContains(node.Bounds, left.Bounds.Max);
                    AssertBoundsContains(node.Bounds, right.Bounds.Min);
                    AssertBoundsContains(node.Bounds, right.Bounds.Max);
                } else {
                    for (int orderSlot = node.FirstIndex; orderSlot < node.FirstIndex + node.Count; orderSlot++) {
                        SoftwareTriangle triangle = triangles[bvh.TriangleOrder[orderSlot]];
                        AssertBoundsContains(node.Bounds, triangle.BoundsMin);
                        AssertBoundsContains(node.Bounds, triangle.BoundsMax);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the recorded maximum depth equals the actual flat tree depth.
        /// </summary>
        [Fact]
        public void Maximum_depth_matches_flat_tree_depth() {
            SoftwareTriangle[] triangles = CreateLineTriangles(73);
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Equal(ComputeDepth(bvh, 0), bvh.MaximumDepth);
        }

        /// <summary>
        /// Ensures exact node preallocation matches the recursively visited flat tree for representative median splits.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(37)]
        [InlineData(73)]
        public void Node_count_matches_actual_visited_tree(int triangleCount) {
            SoftwareTriangle[] triangles = CreateLineTriangles(triangleCount);
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Equal(CountVisitedNodes(bvh, 0), bvh.Nodes.Length);
        }

        /// <summary>
        /// Ensures traversal agrees with the brute-force scalar oracle for 1,024 deterministic rays.
        /// </summary>
        [Fact]
        public void Traversal_matches_brute_force_for_1024_hash_rays() {
            SoftwareTriangle[] triangles = CreateRayTestTriangles();
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            int[] traversalStack = new int[SoftwareBvh.TraversalStackCapacity];

            for (int rayIndex = 0; rayIndex < 1024; rayIndex++) {
                SoftwareRay ray = CreateHashRay(rayIndex);
                bool expected = BruteForce(triangles, ref ray, 0.0001f, 100f, out SoftwareHit expectedHit, out int expectedIndex);
                bool actual = bvh.Intersect(triangles, ref ray, 0.0001f, 100f, traversalStack, out SoftwareHit actualHit, out int actualIndex);

                Assert.Equal(expected, actual);
                Assert.Equal(expectedIndex, actualIndex);
                if (expected) {
                    Assert.Equal(expectedHit.Distance, actualHit.Distance, precision: 5);
                    Assert.Equal(expectedHit.U, actualHit.U, precision: 5);
                    Assert.Equal(expectedHit.V, actualHit.V, precision: 5);
                } else {
                    Assert.Equal(default, actualHit);
                    Assert.Equal(-1, actualIndex);
                }
            }
        }

        /// <summary>
        /// Ensures the nearest collinear hit wins when the nearest leaf is reached after another leaf.
        /// </summary>
        [Fact]
        public void Traversal_returns_nearest_collinear_hit_independent_of_leaf_order() {
            SoftwareTriangle[] triangles = new SoftwareTriangle[5];
            triangles[0] = CreateTriangleWithBounds(0f, 0f, 2f, 0, new float3(0f, 0f, 0f), new float3(0f, 0f, 0f), new float3(0.8f, 0.8f, 5f));
            triangles[1] = CreateTriangleAt(3f, 3f, 3f, 1, new float3(3f, 3f, 3f));
            triangles[2] = CreateTriangleAt(-3f, -3f, 2f, 2, new float3(-3f, -3f, 2f));
            triangles[3] = CreateTriangleAt(5f, 5f, 5f, 3, new float3(5f, 5f, 5f));
            triangles[4] = CreateTriangleWithBounds(0f, 0f, 5f, 4, new float3(0f, 0f, 0f), new float3(0f, 0f, 0f), new float3(0.8f, 0.8f, 5f));
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 6f), new float3(0f, 0f, -1f));
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            Assert.True(bvh.Intersect(triangles, ref ray, 0f, 100f, stack, out SoftwareHit hit, out int triangleIndex));
            Assert.Equal(1f, hit.Distance, precision: 5);
            Assert.Equal(4, triangleIndex);
        }

        /// <summary>
        /// Ensures equal ray parameters resolve to the lower original triangle index.
        /// </summary>
        [Fact]
        public void Equal_parameter_hits_choose_lower_original_index() {
            SoftwareTriangle[] triangles = new SoftwareTriangle[] {
                CreateTriangleAt(0f, 0f, 0f, 0, new float3(0f, 0f, 0f)),
                CreateTriangleAt(0f, 0f, 0f, 1, new float3(0f, 0f, 0f)),
                CreateTriangleAt(0f, 0f, 0f, 2, new float3(0f, 0f, 0f)),
                CreateTriangleAt(0f, 0f, 0f, 3, new float3(0f, 0f, 0f)),
                CreateTriangleAt(0f, 0f, 0f, 4, new float3(0f, 0f, 0f))
            };
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 1f), new float3(0f, 0f, -1f));
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            Assert.True(bvh.Intersect(triangles, ref ray, 0f, 10f, stack, out SoftwareHit _, out int triangleIndex));
            Assert.Equal(0, triangleIndex);
        }

        /// <summary>
        /// Ensures misses return default hit details and the sentinel original index.
        /// </summary>
        [Fact]
        public void Miss_returns_default_hit_and_negative_one_index() {
            SoftwareTriangle[] triangles = CreateLineTriangles(5);
            SoftwareRay ray = new SoftwareRay(new float3(100f, 100f, 100f), new float3(0f, 0f, 1f));
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            Assert.False(bvh.Intersect(triangles, ref ray, 0f, 10f, stack, out SoftwareHit hit, out int triangleIndex));
            Assert.Equal(default, hit);
            Assert.Equal(-1, triangleIndex);
        }

        /// <summary>
        /// Ensures caller minimum and maximum ray parameters are applied by traversal.
        /// </summary>
        [Fact]
        public void Traversal_respects_minimum_and_maximum_parameters() {
            SoftwareTriangle[] triangles = new[] { CreateTriangleAt(0f, 0f, 0f, 0, new float3(0f, 0f, 0f)) };
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 2f), new float3(0f, 0f, -1f));
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];

            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            Assert.False(bvh.Intersect(triangles, ref ray, 2.1f, 10f, stack, out _, out int tooNearIndex));
            Assert.Equal(-1, tooNearIndex);
            Assert.False(bvh.Intersect(triangles, ref ray, 0f, 1.9f, stack, out _, out int tooFarIndex));
            Assert.Equal(-1, tooFarIndex);
            Assert.True(bvh.Intersect(triangles, ref ray, 2f, 2f, stack, out SoftwareHit hit, out _));
            Assert.Equal(2f, hit.Distance, precision: 5);
        }

        /// <summary>
        /// Ensures null and undersized caller stacks are rejected before any traversal or fallback.
        /// </summary>
        [Fact]
        public void Traversal_rejects_null_or_undersized_stack_without_fallback() {
            SoftwareTriangle[] triangles = new[] { CreateTriangleAt(0f, 0f, 0f, 0, new float3(0f, 0f, 0f)) };
            SoftwareRay ray = new SoftwareRay(new float3(0.25f, 0.25f, 1f), new float3(0f, 0f, -1f));
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);

            Assert.Throws<ArgumentNullException>(() => bvh.Intersect(triangles, ref ray, 0f, 10f, null, out _, out _));
            Assert.Throws<ArgumentException>(() => bvh.Intersect(triangles, ref ray, 0f, 10f, new int[SoftwareBvh.TraversalStackCapacity - 1], out _, out _));
        }

        /// <summary>
        /// Ensures disposal releases only BVH-owned arrays, is idempotent, and blocks later traversal.
        /// </summary>
        [Fact]
        public void Dispose_is_idempotent_and_preserves_caller_arrays() {
            SoftwareTriangle[] triangles = CreateLineTriangles(5);
            int[] stack = new int[SoftwareBvh.TraversalStackCapacity];
            stack[0] = 123456;
            using SoftwareBvh bvh = SoftwareBvh.Build(triangles);
            SoftwareBvhNode[] ownedNodes = bvh.Nodes;
            int[] ownedOrder = bvh.TriangleOrder;

            bvh.Dispose();
            bvh.Dispose();

            Assert.Empty(bvh.Nodes);
            Assert.Empty(bvh.TriangleOrder);
            Assert.Equal(123456, stack[0]);
            Assert.NotSame(ownedNodes, bvh.Nodes);
            Assert.NotSame(ownedOrder, bvh.TriangleOrder);
            SoftwareRay ray = new SoftwareRay(new float3(0f, 0f, 1f), new float3(0f, 0f, -1f));
            Assert.Throws<ObjectDisposedException>(() => bvh.Intersect(triangles, ref ray, 0f, 10f, stack, out _, out _));
        }

        /// <summary>
        /// Ensures the private depth-cap guard accepts one less than capacity and rejects capacity.
        /// </summary>
        [Fact]
        public void Depth_cap_guard_accepts_sixty_three_and_rejects_sixty_four() {
            MethodInfo guard = typeof(SoftwareBvh).GetMethod("ValidateMaximumDepth", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(guard);
            guard.Invoke(null, new object[] { SoftwareBvh.TraversalStackCapacity - 1 });
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => guard.Invoke(null, new object[] { SoftwareBvh.TraversalStackCapacity }));
            Assert.IsType<ArgumentOutOfRangeException>(exception.InnerException);
        }

        /// <summary>
        /// Computes the maximum depth of the compact flat tree.
        /// </summary>
        /// <param name="bvh">BVH whose root is at index zero.</param>
        /// <param name="nodeIndex">Current node index.</param>
        /// <returns>Maximum depth below the current node.</returns>
        static int ComputeDepth(SoftwareBvh bvh, int nodeIndex) {
            SoftwareBvhNode node = bvh.Nodes[nodeIndex];
            if (node.IsLeaf) {
                return 0;
            }
            return 1 + Math.Max(ComputeDepth(bvh, node.FirstIndex), ComputeDepth(bvh, node.FirstIndex + 1));
        }

        /// <summary>
        /// Counts nodes reachable from one flat-tree node.
        /// </summary>
        /// <param name="bvh">BVH whose root is at index zero.</param>
        /// <param name="nodeIndex">Current node index.</param>
        /// <returns>Number of reachable nodes including the current node.</returns>
        static int CountVisitedNodes(SoftwareBvh bvh, int nodeIndex) {
            SoftwareBvhNode node = bvh.Nodes[nodeIndex];
            if (node.IsLeaf) {
                return 1;
            }
            return 1 + CountVisitedNodes(bvh, node.FirstIndex) + CountVisitedNodes(bvh, node.FirstIndex + 1);
        }

        /// <summary>
        /// Checks that one point lies inside or on an axis-aligned box.
        /// </summary>
        /// <param name="bounds">Containing bounds.</param>
        /// <param name="point">Point to check.</param>
        static void AssertBoundsContains(SoftwareBounds bounds, float3 point) {
            const float epsilon = 0.00001f;
            Assert.InRange(point.X, bounds.Min.X - epsilon, bounds.Max.X + epsilon);
            Assert.InRange(point.Y, bounds.Min.Y - epsilon, bounds.Max.Y + epsilon);
            Assert.InRange(point.Z, bounds.Min.Z - epsilon, bounds.Max.Z + epsilon);
        }

        /// <summary>
        /// Creates triangles with centers along a supplied deterministic sequence.
        /// </summary>
        /// <param name="centroids">Triangle center positions.</param>
        /// <returns>Triangles with matching bounds and centroids.</returns>
        static SoftwareTriangle[] CreateCentroidTriangles(params float3[] centroids) {
            SoftwareTriangle[] triangles = new SoftwareTriangle[centroids.Length];
            for (int i = 0; i < centroids.Length; i++) {
                triangles[i] = CreateTriangleAt(centroids[i].X, centroids[i].Y, centroids[i].Z, i, centroids[i]);
            }
            return triangles;
        }

        /// <summary>
        /// Creates triangles along the X axis for tree-shape and ownership tests.
        /// </summary>
        /// <param name="count">Number of triangles.</param>
        /// <returns>Deterministic linearly spaced triangles.</returns>
        static SoftwareTriangle[] CreateLineTriangles(int count) {
            SoftwareTriangle[] triangles = new SoftwareTriangle[count];
            for (int i = 0; i < count; i++) {
                triangles[i] = CreateTriangleAt(i * 2f, 0f, 0f, i, new float3(i * 2f, 0f, 0f));
            }
            return triangles;
        }

        /// <summary>
        /// Creates one triangle with a controllable centroid key.
        /// </summary>
        /// <param name="centerX">Triangle center X coordinate.</param>
        /// <param name="centerY">Triangle center Y coordinate.</param>
        /// <param name="centerZ">Triangle plane Z coordinate.</param>
        /// <param name="materialIndex">Material index stored on the triangle.</param>
        /// <param name="centroid">Centroid key used by the BVH builder.</param>
        /// <returns>A unit right triangle on a horizontal plane.</returns>
        static SoftwareTriangle CreateTriangleAt(float centerX, float centerY, float centerZ, int materialIndex, float3 centroid) {
            float3 p0 = new float3(centerX, centerY, centerZ);
            float3 edge1 = new float3(0.8f, 0f, 0f);
            float3 edge2 = new float3(0f, 0.8f, 0f);
            return CreateTriangleWithBounds(centerX, centerY, centerZ, materialIndex, centroid, p0, p0 + edge1 + edge2);
        }

        /// <summary>
        /// Creates one triangle with explicitly supplied BVH bounds for traversal-order fixtures.
        /// </summary>
        /// <param name="centerX">Triangle corner X coordinate.</param>
        /// <param name="centerY">Triangle corner Y coordinate.</param>
        /// <param name="centerZ">Triangle plane Z coordinate.</param>
        /// <param name="materialIndex">Material index stored on the triangle.</param>
        /// <param name="centroid">Centroid key used by the BVH builder.</param>
        /// <param name="boundsMin">Explicit triangle bounds minimum.</param>
        /// <param name="boundsMax">Explicit triangle bounds maximum.</param>
        /// <returns>A triangle with the requested explicit bounds.</returns>
        static SoftwareTriangle CreateTriangleWithBounds(float centerX, float centerY, float centerZ, int materialIndex, float3 centroid, float3 boundsMin, float3 boundsMax) {
            float3 p0 = new float3(centerX, centerY, centerZ);
            float3 edge1 = new float3(0.8f, 0f, 0f);
            float3 edge2 = new float3(0f, 0.8f, 0f);
            return new SoftwareTriangle(
                p0,
                edge1,
                edge2,
                new float3(0f, 0f, 1f),
                materialIndex,
                centroid,
                boundsMin,
                boundsMax);
        }

        /// <summary>
        /// Creates a spread of triangles used by the deterministic ray oracle.
        /// </summary>
        /// <returns>Thirty-two deterministic triangles.</returns>
        static SoftwareTriangle[] CreateRayTestTriangles() {
            SoftwareTriangle[] triangles = new SoftwareTriangle[32];
            for (int i = 0; i < triangles.Length; i++) {
                float x = (i % 8) * 1.5f - 5.25f;
                float y = (i / 8) * 1.5f - 2.25f;
                float z = (i % 4) * 0.75f;
                triangles[i] = CreateTriangleAt(x, y, z, i, new float3(x + 0.4f, y + 0.4f, z));
            }
            return triangles;
        }

        /// <summary>
        /// Creates one deterministic ray from an integer hash of its index.
        /// </summary>
        /// <param name="index">Ray sequence index.</param>
        /// <returns>A finite deterministic ray.</returns>
        static SoftwareRay CreateHashRay(int index) {
            uint state = (uint)index + 0x9E3779B9u;
            float originX = HashUnit(ref state) * 16f - 8f;
            float originY = HashUnit(ref state) * 10f - 5f;
            float originZ = HashUnit(ref state) * 5f + 2f;
            float directionX = HashUnit(ref state) * 2f - 1f;
            float directionY = HashUnit(ref state) * 2f - 1f;
            float directionZ = -0.25f - HashUnit(ref state) * 2f;
            return new SoftwareRay(new float3(originX, originY, originZ), new float3(directionX, directionY, directionZ));
        }

        /// <summary>
        /// Advances an integer hash and converts its upper bits to a deterministic unit float.
        /// </summary>
        /// <param name="state">Hash state updated in place.</param>
        /// <returns>A value in the half-open interval [0, 1).</returns>
        static float HashUnit(ref uint state) {
            state ^= state >> 16;
            state *= 0x7FEB352Du;
            state ^= state >> 15;
            state *= 0x846CA68Bu;
            state ^= state >> 16;
            return (state >> 8) * (1f / 16777216f);
        }

        /// <summary>
        /// Computes the nearest hit through the approved scalar triangle oracle.
        /// </summary>
        /// <param name="triangles">Triangles to test.</param>
        /// <param name="ray">Ray to test.</param>
        /// <param name="minimumDistance">Inclusive minimum ray parameter.</param>
        /// <param name="maximumDistance">Inclusive maximum ray parameter.</param>
        /// <param name="hit">Nearest hit when one exists.</param>
        /// <param name="triangleIndex">Nearest original triangle index or -1.</param>
        /// <returns>True when one triangle is hit.</returns>
        static bool BruteForce(SoftwareTriangle[] triangles, ref SoftwareRay ray, float minimumDistance, float maximumDistance, out SoftwareHit hit, out int triangleIndex) {
            hit = default;
            triangleIndex = -1;
            float nearestDistance = maximumDistance;
            for (int i = 0; i < triangles.Length; i++) {
                SoftwareTriangle triangle = triangles[i];
                if (!SoftwareBvh.IntersectTriangle(ref ray, ref triangle, minimumDistance, maximumDistance, out SoftwareHit candidate)) {
                    continue;
                }
                if (triangleIndex < 0 || candidate.Distance < nearestDistance || (candidate.Distance == nearestDistance && i < triangleIndex)) {
                    nearestDistance = candidate.Distance;
                    hit = candidate;
                    triangleIndex = i;
                }
            }
            return triangleIndex >= 0;
        }
    }
}
