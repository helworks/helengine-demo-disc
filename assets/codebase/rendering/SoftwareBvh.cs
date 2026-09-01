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
        /// Gets the world-space ray direction as supplied; intersection routines return the ray parameter t and do not normalize it.
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
        /// Gets the ray parameter t at the intersection. This equals world-space distance only when the direction is unit length.
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
        /// Initializes one scalar software hit.
        /// </summary>
        /// <param name="distance">Ray parameter t at the hit; this is world-space distance only for a unit direction.</param>
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
    /// Stores one compact flat software BVH node.
    /// </summary>
    public readonly struct SoftwareBvhNode {
        /// <summary>
        /// Gets the axis-aligned bounds enclosing this node.
        /// </summary>
        public SoftwareBounds Bounds { get; }

        /// <summary>
        /// Gets the first triangle-order slot for a leaf or the left child index for an interior.
        /// </summary>
        public int FirstIndex { get; }

        /// <summary>
        /// Gets the leaf triangle count, or zero for an interior node.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets a value indicating whether this node is a leaf; interiors use a zero count.
        /// </summary>
        public bool IsLeaf {
            get { return Count > 0; }
        }

        /// <summary>
        /// Initializes one compact node from its bounds and payload fields.
        /// </summary>
        /// <param name="bounds">Axis-aligned bounds enclosing the node.</param>
        /// <param name="firstIndex">Leaf order slot or left child index.</param>
        /// <param name="count">Leaf triangle count, or zero for an interior.</param>
        internal SoftwareBvhNode(SoftwareBounds bounds, int firstIndex, int count) {
            Bounds = bounds;
            FirstIndex = firstIndex;
            Count = count;
        }
    }

    /// <summary>
    /// Provides scalar, allocation-free intersection primitives and a deterministic bounded BVH.
    /// </summary>
    public sealed class SoftwareBvh : IDisposable {
        /// <summary>
        /// Maximum number of original triangles stored in one leaf.
        /// </summary>
        public const int LeafTriangleCapacity = 4;

        /// <summary>
        /// Maximum number of caller-supplied traversal stack entries used by one ray. In v0, the single worker reuses one caller-owned traversal stack; before any future multithreading, this scratch must become worker-local.
        /// </summary>
        public const int TraversalStackCapacity = 64;

        /// <summary>
        /// Compact flat nodes owned by this BVH.
        /// </summary>
        SoftwareBvhNode[] nodes;

        /// <summary>
        /// Original triangle indices arranged into deterministic leaf ranges.
        /// </summary>
        int[] triangleOrder;

        /// <summary>
        /// Exact source triangle array borrowed from the caller; its contents must remain immutable for the BVH lifetime.
        /// </summary>
        SoftwareTriangle[] sourceTriangles;

        /// <summary>
        /// Maximum root-relative depth recorded during construction.
        /// </summary>
        readonly int maximumDepth;

        /// <summary>
        /// Indicates that owned arrays have been released and traversal is no longer valid.
        /// </summary>
        bool disposed;

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
        /// Gets the compact flat nodes owned by this BVH.
        /// </summary>
        public SoftwareBvhNode[] Nodes {
            get { return nodes; }
        }

        /// <summary>
        /// Gets the triangle-order slots owned by this BVH.
        /// </summary>
        public int[] TriangleOrder {
            get { return triangleOrder; }
        }

        /// <summary>
        /// Gets the maximum root-relative depth recorded during construction.
        /// </summary>
        public int MaximumDepth {
            get { return maximumDepth; }
        }

        /// <summary>
        /// Initializes one deterministic BVH from its exact compact arrays.
        /// </summary>
        /// <param name="nodes">Flat node array owned by the BVH.</param>
        /// <param name="triangleOrder">Triangle-order array owned by the BVH.</param>
        /// <param name="maximumDepth">Maximum root-relative build depth.</param>
        /// <param name="sourceTriangles">Exact source triangle array retained by reference for traversal identity validation.</param>
        SoftwareBvh([NativeTakesOwnership] SoftwareBvhNode[] nodes, [NativeTakesOwnership] int[] triangleOrder, int maximumDepth, SoftwareTriangle[] sourceTriangles) {
            this.nodes = nodes;
            this.triangleOrder = triangleOrder;
            this.maximumDepth = maximumDepth;
            this.sourceTriangles = sourceTriangles;
        }

        /// <summary>
        /// Builds one deterministic median-split BVH over finite compact triangle bounds.
        /// </summary>
        /// <param name="triangles">Validated triangles to index; the caller retains ownership and must keep this exact array instance and its contents immutable for the BVH lifetime.</param>
        /// <returns>A compact deterministic BVH owning only its nodes and order array.</returns>
        public static SoftwareBvh Build(SoftwareTriangle[] triangles) {
            if (triangles == null) {
                throw new ArgumentNullException(nameof(triangles));
            }
            if (triangles.Length == 0) {
                throw new ArgumentException("A software BVH requires at least one triangle.", nameof(triangles));
            }

            int[] order = new int[triangles.Length];
            for (int i = 0; i < order.Length; i++) {
                order[i] = i;
            }

            int nodeCount = CountNodes(triangles.Length);
            SoftwareBvhNode[] nodes = new SoftwareBvhNode[nodeCount];
            int nextNodeIndex = 1;
            int maximumDepth = 0;
            BuildNode(triangles, order, nodes, 0, 0, triangles.Length, 0, ref nextNodeIndex, ref maximumDepth);
            if (nextNodeIndex != nodeCount) {
                throw new InvalidOperationException("The deterministic BVH node prepass did not match its build.");
            }
            ValidateMaximumDepth(maximumDepth);
            return new SoftwareBvh(nodes, order, maximumDepth, triangles);
        }

        /// <summary>
        /// Releases the node and triangle-order arrays; repeated disposal is harmless.
        /// </summary>
        public void Dispose() {
            if (disposed) {
                return;
            }

            disposed = true;
            nodes = Array.Empty<SoftwareBvhNode>();
            triangleOrder = Array.Empty<int>();
            sourceTriangles = null;
        }

        /// <summary>
        /// Counts the exact flat nodes required by the fixed-capacity median recursion.
        /// </summary>
        /// <param name="triangleCount">Number of source triangles.</param>
        /// <returns>Exact number of full-binary-tree nodes.</returns>
        static int CountNodes(int triangleCount) {
            if (triangleCount <= LeafTriangleCapacity) {
                return 1;
            }

            int leftCount = triangleCount / 2;
            int rightCount = triangleCount - leftCount;
            return 1 + CountNodes(leftCount) + CountNodes(rightCount);
        }

        /// <summary>
        /// Builds one node and its reserved adjacent child roots in deterministic order.
        /// </summary>
        /// <param name="triangles">Source triangles.</param>
        /// <param name="order">Mutable original-index order.</param>
        /// <param name="nodes">Final flat node array.</param>
        /// <param name="nodeIndex">Reserved index for this node.</param>
        /// <param name="start">First order slot in this range.</param>
        /// <param name="count">Number of order slots in this range.</param>
        /// <param name="depth">Root-relative depth of this node.</param>
        /// <param name="nextNodeIndex">Next unreserved node slot.</param>
        /// <param name="maximumDepth">Maximum depth updated in place.</param>
        static void BuildNode(SoftwareTriangle[] triangles, [NativeNoEscape] int[] order, [NativeNoEscape] SoftwareBvhNode[] nodes, int nodeIndex, int start, int count, int depth, ref int nextNodeIndex, ref int maximumDepth) {
            if (depth > maximumDepth) {
                maximumDepth = depth;
            }

            int axis = SelectLargestCentroidAxis(triangles, order, start, count);
            SortTriangleOrder(triangles, order, start, count, axis);
            SoftwareBounds bounds = ComputeRangeBounds(triangles, order, start, count);
            if (count <= LeafTriangleCapacity) {
                nodes[nodeIndex] = new SoftwareBvhNode(bounds, start, count);
                return;
            }

            int leftCount = count / 2;
            int rightCount = count - leftCount;
            int leftIndex = nextNodeIndex;
            int rightIndex = leftIndex + 1;
            nextNodeIndex += 2;
            nodes[nodeIndex] = new SoftwareBvhNode(bounds, leftIndex, 0);
            BuildNode(triangles, order, nodes, leftIndex, start, leftCount, depth + 1, ref nextNodeIndex, ref maximumDepth);
            BuildNode(triangles, order, nodes, rightIndex, start + leftCount, rightCount, depth + 1, ref nextNodeIndex, ref maximumDepth);
        }

        /// <summary>
        /// Selects the largest centroid extent, resolving ties in X, then Y, then Z order.
        /// </summary>
        /// <param name="triangles">Source triangles.</param>
        /// <param name="order">Current original-index order.</param>
        /// <param name="start">First order slot.</param>
        /// <param name="count">Number of order slots.</param>
        /// <returns>Zero for X, one for Y, or two for Z.</returns>
        static int SelectLargestCentroidAxis(SoftwareTriangle[] triangles, int[] order, int start, int count) {
            SoftwareTriangle first = triangles[order[start]];
            float minX = first.Centroid.X;
            float maxX = minX;
            float minY = first.Centroid.Y;
            float maxY = minY;
            float minZ = first.Centroid.Z;
            float maxZ = minZ;
            for (int i = 1; i < count; i++) {
                SoftwareTriangle triangle = triangles[order[start + i]];
                if (triangle.Centroid.X < minX) minX = triangle.Centroid.X;
                if (triangle.Centroid.X > maxX) maxX = triangle.Centroid.X;
                if (triangle.Centroid.Y < minY) minY = triangle.Centroid.Y;
                if (triangle.Centroid.Y > maxY) maxY = triangle.Centroid.Y;
                if (triangle.Centroid.Z < minZ) minZ = triangle.Centroid.Z;
                if (triangle.Centroid.Z > maxZ) maxZ = triangle.Centroid.Z;
            }

            float xExtent = maxX - minX;
            float yExtent = maxY - minY;
            float zExtent = maxZ - minZ;
            int axis = 0;
            float selectedExtent = xExtent;
            if (yExtent > selectedExtent) {
                axis = 1;
                selectedExtent = yExtent;
            }
            if (zExtent > selectedExtent) {
                axis = 2;
            }
            return axis;
        }

        /// <summary>
        /// Sorts one order range in place by centroid component and original index.
        /// </summary>
        /// <param name="triangles">Source triangles.</param>
        /// <param name="order">Mutable original-index order.</param>
        /// <param name="start">First order slot.</param>
        /// <param name="count">Number of order slots.</param>
        /// <param name="axis">Centroid axis used as the primary key.</param>
        static void SortTriangleOrder(SoftwareTriangle[] triangles, int[] order, int start, int count, int axis) {
            for (int root = (count / 2) - 1; root >= 0; root--) {
                SiftDownTriangleOrder(triangles, order, start, count, root, axis);
            }
            for (int last = count - 1; last > 0; last--) {
                SwapTriangleOrder(order, start, start + last);
                SiftDownTriangleOrder(triangles, order, start, last, 0, axis);
            }
        }

        /// <summary>
        /// Restores the max-heap property below one relative order slot.
        /// </summary>
        /// <param name="triangles">Source triangles.</param>
        /// <param name="order">Mutable original-index order.</param>
        /// <param name="start">First absolute order slot.</param>
        /// <param name="length">Heap length from the first slot.</param>
        /// <param name="root">Relative root slot to sift down.</param>
        /// <param name="axis">Centroid axis used as the primary key.</param>
        static void SiftDownTriangleOrder(SoftwareTriangle[] triangles, int[] order, int start, int length, int root, int axis) {
            while (root < length / 2) {
                int child = (root * 2) + 1;
                if (child + 1 < length && CompareTriangleOrder(triangles, order[start + child], order[start + child + 1], axis) < 0) {
                    child++;
                }
                if (CompareTriangleOrder(triangles, order[start + root], order[start + child], axis) >= 0) {
                    return;
                }
                SwapTriangleOrder(order, start + root, start + child);
                root = child;
            }
        }

        /// <summary>
        /// Swaps two absolute triangle-order slots without allocating.
        /// </summary>
        /// <param name="order">Mutable original-index order.</param>
        /// <param name="left">First absolute slot.</param>
        /// <param name="right">Second absolute slot.</param>
        static void SwapTriangleOrder(int[] order, int left, int right) {
            int temporary = order[left];
            order[left] = order[right];
            order[right] = temporary;
        }

        /// <summary>
        /// Compares two original triangle indices for one centroid axis.
        /// </summary>
        /// <param name="triangles">Source triangles.</param>
        /// <param name="leftIndex">Left original triangle index.</param>
        /// <param name="rightIndex">Right original triangle index.</param>
        /// <param name="axis">Centroid axis used as the primary key.</param>
        /// <returns>Negative, zero, or positive according to deterministic ordering.</returns>
        static int CompareTriangleOrder(SoftwareTriangle[] triangles, int leftIndex, int rightIndex, int axis) {
            float left = GetCentroidComponent(triangles[leftIndex].Centroid, axis);
            float right = GetCentroidComponent(triangles[rightIndex].Centroid, axis);
            if (left < right) return -1;
            if (left > right) return 1;
            if (leftIndex < rightIndex) return -1;
            if (leftIndex > rightIndex) return 1;
            return 0;
        }

        /// <summary>
        /// Reads one centroid component without allocating or creating a delegate.
        /// </summary>
        /// <param name="centroid">Centroid vector.</param>
        /// <param name="axis">Zero for X, one for Y, or two for Z.</param>
        /// <returns>The selected centroid component.</returns>
        static float GetCentroidComponent(float3 centroid, int axis) {
            if (axis == 0) return centroid.X;
            if (axis == 1) return centroid.Y;
            return centroid.Z;
        }

        /// <summary>
        /// Unions source triangle bounds over one order range.
        /// </summary>
        /// <param name="triangles">Source triangles.</param>
        /// <param name="order">Current original-index order.</param>
        /// <param name="start">First order slot.</param>
        /// <param name="count">Number of order slots.</param>
        /// <returns>Union bounds for the range.</returns>
        static SoftwareBounds ComputeRangeBounds(SoftwareTriangle[] triangles, int[] order, int start, int count) {
            SoftwareTriangle first = triangles[order[start]];
            float3 minimum = first.BoundsMin;
            float3 maximum = first.BoundsMax;
            for (int i = 1; i < count; i++) {
                SoftwareTriangle triangle = triangles[order[start + i]];
                minimum = new float3(
                    Math.Min(minimum.X, triangle.BoundsMin.X),
                    Math.Min(minimum.Y, triangle.BoundsMin.Y),
                    Math.Min(minimum.Z, triangle.BoundsMin.Z));
                maximum = new float3(
                    Math.Max(maximum.X, triangle.BoundsMax.X),
                    Math.Max(maximum.Y, triangle.BoundsMax.Y),
                    Math.Max(maximum.Z, triangle.BoundsMax.Z));
            }
            return new SoftwareBounds(minimum, maximum);
        }

        /// <summary>
        /// Validates the maximum tree depth against the fixed traversal capacity.
        /// </summary>
        /// <param name="depth">Root-relative maximum depth to validate.</param>
        static void ValidateMaximumDepth(int depth) {
            if (depth < 0 || depth >= TraversalStackCapacity) {
                throw new ArgumentOutOfRangeException(nameof(depth), depth, "The software BVH exceeds the fixed traversal stack capacity.");
            }
        }

        /// <summary>
        /// Traverses the compact nodes with caller-owned fixed-capacity scratch and returns the nearest hit.
        /// </summary>
        /// <param name="triangles">The exact triangle array instance supplied to Build; the caller retains ownership and must keep its contents immutable for the BVH lifetime.</param>
        /// <param name="ray">Ray to trace; its direction is used as supplied.</param>
        /// <param name="minimumDistance">Inclusive lower ray-parameter bound.</param>
        /// <param name="maximumDistance">Inclusive upper ray-parameter bound.</param>
        /// <param name="traversalStack">Caller-owned scratch array with at least TraversalStackCapacity entries. The v0 single worker reuses one such stack; this scratch must become worker-local before any future multithreading.</param>
        /// <param name="hit">Nearest intersection details, or default when there is no hit.</param>
        /// <param name="triangleIndex">Nearest original triangle index, or -1 when there is no hit.</param>
        /// <returns>True when one indexed triangle intersects the ray in the supplied range.</returns>
        public bool Intersect(SoftwareTriangle[] triangles, ref SoftwareRay ray, float minimumDistance, float maximumDistance, int[] traversalStack, out SoftwareHit hit, out int triangleIndex) {
            hit = default;
            triangleIndex = -1;
            if (disposed) {
                throw new ObjectDisposedException(nameof(SoftwareBvh));
            }
            if (triangles == null) {
                throw new ArgumentNullException(nameof(triangles));
            }
            if (!ReferenceEquals(triangles, sourceTriangles)) {
                throw new ArgumentException("The supplied triangles must be the exact immutable array used to build this BVH.", nameof(triangles));
            }
            if (traversalStack == null) {
                throw new ArgumentNullException(nameof(traversalStack));
            }
            if (traversalStack.Length < TraversalStackCapacity) {
                throw new ArgumentException("The traversal stack is smaller than the fixed BVH capacity.", nameof(traversalStack));
            }
            if (float.IsNaN(minimumDistance) || float.IsNaN(maximumDistance) || minimumDistance > maximumDistance || maximumDistance < 0f) {
                return false;
            }

            float nearestDistance = maximumDistance;
            int stackCount = 1;
            traversalStack[0] = 0;
            while (stackCount > 0) {
                int nodeIndex = traversalStack[--stackCount];
                if ((uint)nodeIndex >= (uint)nodes.Length) {
                    throw new InvalidOperationException("The software BVH contains an invalid node reference.");
                }

                SoftwareBvhNode node = nodes[nodeIndex];
                SoftwareBounds nodeBounds = node.Bounds;
                if (!TryIntersectBounds(ref ray, ref nodeBounds, nearestDistance, out _)) {
                    continue;
                }

                if (node.IsLeaf) {
                    for (int orderSlot = node.FirstIndex; orderSlot < node.FirstIndex + node.Count; orderSlot++) {
                        if ((uint)orderSlot >= (uint)triangleOrder.Length) {
                            throw new InvalidOperationException("The software BVH contains an invalid triangle-order range.");
                        }
                        int originalIndex = triangleOrder[orderSlot];
                        if ((uint)originalIndex >= (uint)triangles.Length) {
                            throw new InvalidOperationException("The supplied triangles do not match the software BVH order.");
                        }

                        SoftwareTriangle triangle = triangles[originalIndex];
                        if (!IntersectTriangle(ref ray, ref triangle, minimumDistance, nearestDistance, out SoftwareHit candidate)) {
                            continue;
                        }
                        if (triangleIndex < 0 || candidate.Distance < nearestDistance || (candidate.Distance == nearestDistance && originalIndex < triangleIndex)) {
                            nearestDistance = candidate.Distance;
                            hit = candidate;
                            triangleIndex = originalIndex;
                        }
                    }
                    continue;
                }

                int leftIndex = node.FirstIndex;
                int rightIndex = leftIndex + 1;
                if ((uint)leftIndex >= (uint)nodes.Length || (uint)rightIndex >= (uint)nodes.Length) {
                    throw new InvalidOperationException("The software BVH contains an invalid child reference.");
                }

                SoftwareBounds leftBounds = nodes[leftIndex].Bounds;
                SoftwareBounds rightBounds = nodes[rightIndex].Bounds;
                bool leftHit = TryIntersectBounds(ref ray, ref leftBounds, nearestDistance, out float leftEntry);
                bool rightHit = TryIntersectBounds(ref ray, ref rightBounds, nearestDistance, out float rightEntry);
                if (leftHit && rightHit) {
                    if (leftEntry < rightEntry || (leftEntry == rightEntry && leftIndex < rightIndex)) {
                        PushTraversalNode(traversalStack, ref stackCount, rightIndex);
                        PushTraversalNode(traversalStack, ref stackCount, leftIndex);
                    } else {
                        PushTraversalNode(traversalStack, ref stackCount, leftIndex);
                        PushTraversalNode(traversalStack, ref stackCount, rightIndex);
                    }
                } else if (leftHit) {
                    PushTraversalNode(traversalStack, ref stackCount, leftIndex);
                } else if (rightHit) {
                    PushTraversalNode(traversalStack, ref stackCount, rightIndex);
                }
            }

            return triangleIndex >= 0;
        }

        /// <summary>
        /// Intersects a scalar ray with a world-space triangle using Moller-Trumbore arithmetic and returns its ray parameter.
        /// </summary>
        /// <param name="ray">Ray whose direction is used as supplied; it is not normalized.</param>
        /// <param name="triangle">Triangle to test.</param>
        /// <param name="minimumDistance">Inclusive lower ray-parameter bound; this is world-space distance only for a unit direction.</param>
        /// <param name="maximumDistance">Inclusive upper ray-parameter bound; this is world-space distance only for a unit direction.</param>
        /// <param name="hit">Intersection details when the method returns true; default otherwise.</param>
        /// <returns>True when the ray intersects the triangle inside the supplied forward ray-parameter range.</returns>
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
        /// <param name="maximumDistance">Inclusive upper ray-parameter bound; this is world-space distance only for a unit direction.</param>
        /// <returns>True when the forward ray enters the bounds before the supplied ray parameter.</returns>
        public static bool IntersectBounds(ref SoftwareRay ray, ref SoftwareBounds bounds, float maximumDistance) {
            return TryIntersectBounds(ref ray, ref bounds, maximumDistance, out _);
        }

        /// <summary>
        /// Intersects bounds and returns the forward entry parameter for near-first traversal.
        /// </summary>
        /// <param name="ray">Ray whose direction is used as supplied.</param>
        /// <param name="bounds">Axis-aligned bounds to test.</param>
        /// <param name="maximumDistance">Inclusive upper ray-parameter bound.</param>
        /// <param name="entryDistance">Forward ray parameter at the bounds entry.</param>
        /// <returns>True when the forward ray enters the bounds before the supplied parameter.</returns>
        static bool TryIntersectBounds(ref SoftwareRay ray, ref SoftwareBounds bounds, float maximumDistance, out float entryDistance) {
            entryDistance = 0f;
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

            if (nearDistance <= farDistance + BoundsEpsilon && farDistance >= -BoundsEpsilon) {
                entryDistance = nearDistance;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pushes one node onto caller scratch while enforcing the fixed traversal capacity.
        /// </summary>
        /// <param name="traversalStack">Caller-owned traversal scratch.</param>
        /// <param name="stackCount">Current stack count updated in place.</param>
        /// <param name="nodeIndex">Node index to push.</param>
        static void PushTraversalNode(int[] traversalStack, ref int stackCount, int nodeIndex) {
            if (stackCount >= TraversalStackCapacity) {
                throw new InvalidOperationException("The software BVH traversal stack was exhausted.");
            }
            traversalStack[stackCount++] = nodeIndex;
        }

        /// <summary>
        /// Intersects one scalar ray component with one bounds slab and narrows its interval in place.
        /// </summary>
        /// <param name="origin">Ray origin component.</param>
        /// <param name="direction">Ray direction component.</param>
        /// <param name="minimum">Slab minimum.</param>
        /// <param name="maximum">Slab maximum.</param>
        /// <param name="nearDistance">Current near ray-parameter interval, updated on success.</param>
        /// <param name="farDistance">Current far ray-parameter interval, updated on success.</param>
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
