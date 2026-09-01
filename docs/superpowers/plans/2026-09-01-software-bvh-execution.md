# DemoDisc Deterministic Software BVH Execution Plan

> **Worker boundary:** Root owns this plan and acceptance. A Luna xhigh worker implements it with strict red-green-refactor. Review happens after the implementation commit.

**Goal:** Extend DemoDisc's scalar intersection file with one compact, deterministic BVH that builds during initialization and traverses without per-ray allocation.

**Scope:** Modify `assets/codebase/rendering/SoftwareBvh.cs`; create `assets/codebase/gameplay.tests/SoftwareBvhTests.cs` and its adjacent `.hmeta`. Do not modify HelenEngine, scene ingestion, generated project files, or unrelated DemoDisc code.

**Inputs:** The validated, finite `SoftwareTriangle[]` produced by `SoftwareTraceScene`.

**Outputs:** Compact nodes, one triangle-order array, maximum build depth, caller-scratch nearest-hit traversal, and explicit idempotent release of BVH-owned arrays.

## Fixed design

- `LeafTriangleCapacity` is exactly `4`.
- `TraversalStackCapacity` is exactly `64`.
- Reject null or empty triangle arrays. Do not silently build an empty root.
- Initialize `TriangleOrder` to original triangle indices and sort each node range by the largest centroid-extent axis. Axis ties resolve X, then Y, then Z. Centroid ties resolve by original triangle index.
- Split each interior range at its median. Equal centroids still split because original index is the final ordering key.
- Use a flat depth-first node array. A leaf stores the first `TriangleOrder` slot and count. An interior stores its left child index; its right child is the next index. Reserve both child slots before recursing so this invariant is stable.
- Keep nodes compact: bounds plus two integer payload fields. `IsLeaf` is computed and does not consume stored state.
- Precompute the exact node count from the triangle count and fixed median/leaf rules, then allocate the final node array once. Do not retain lists, temporary node arrays, or comparer closures.
- Builder-time allocation is allowed. Traversal allocation is not.
- The caller supplies an `int[]` traversal stack of at least `TraversalStackCapacity`. This is the one worker-owned scratch array described by the core plan.
- Traversal returns the nearest hit plus the original triangle index. Equal ray parameters resolve to the lower original triangle index.
- Test both child bounds, visit the smaller entry parameter first, and push the far child before the near child. Entry ties resolve by child index.
- Never fall back to brute force if the stack is invalid or exhausted. Reject an undersized stack before traversal and guard every push.
- `Dispose()` is idempotent, replaces owned arrays with empty arrays, and prevents subsequent traversal. It never owns or clears the caller's triangle or stack arrays.
- Keep all code in DemoDisc, avoid LINQ, and preserve the scalar intersection API accepted in Task 3.

## Required public surface

Exact names may be adjusted only when existing DemoDisc conventions require it, but the behavior and ownership must remain:

```csharp
public readonly struct SoftwareBvhNode {
    public SoftwareBounds Bounds { get; }
    public int FirstIndex { get; }
    public int Count { get; }
    public bool IsLeaf { get; }
}

public sealed class SoftwareBvh : IDisposable {
    public const int LeafTriangleCapacity = 4;
    public const int TraversalStackCapacity = 64;
    public SoftwareBvhNode[] Nodes { get; }
    public int[] TriangleOrder { get; }
    public int MaximumDepth { get; }

    public static SoftwareBvh Build(SoftwareTriangle[] triangles);

    public bool Intersect(
        SoftwareTriangle[] triangles,
        ref SoftwareRay ray,
        float minimumDistance,
        float maximumDistance,
        int[] traversalStack,
        out SoftwareHit hit,
        out int triangleIndex);
}
```

`FirstIndex` means the first triangle-order slot for a leaf and the left child index for an interior. An interior's right child is `FirstIndex + 1`.

## TDD sequence

### 1. RED: deterministic build contract

Create `SoftwareBvhTests.cs` and its valid unique lowercase 32-hex `.hmeta`. Add tests that initially fail because the build API is absent:

- null and empty input are rejected;
- zero through four triangles produce one leaf;
- five or more triangles split by largest centroid extent;
- axis ties prefer X, then Y, then Z;
- equal centroids resolve by original triangle index;
- repeated builds produce identical node fields and `TriangleOrder` arrays;
- every original triangle occurs exactly once;
- every leaf maps to a contiguous order range of one through four entries;
- parent bounds contain both children and all leaf triangles;
- `MaximumDepth` matches the actual flat tree.

Run only `SoftwareBvhTests` and capture the expected failure before production edits.

### 2. GREEN: compact deterministic builder

Implement the node representation, exact node-count prepass, deterministic range sorting, median recursion, bounds union, and depth tracking. Add XML documentation for every member, including private helpers.

Do not use LINQ. Do not use filesystem APIs, renderer APIs, `RuntimeModel`, `MeshComponent`, or GPU-backed assets.

### 3. RED/GREEN: nearest-hit traversal

Add tests for:

- nearest hit agrees with a brute-force loop for at least 1,024 deterministic hash-generated rays;
- nearest of multiple collinear triangles wins even when its leaf is visited second;
- equal-parameter hits select the lower original triangle index;
- a miss returns `false`, default hit, and triangle index `-1`;
- caller minimum and maximum parameters are respected;
- an undersized or null traversal stack is rejected before traversal;
- traversal performs no brute-force fallback when stack validation fails;
- disposed traversal is rejected, repeated disposal is harmless, and caller arrays remain intact.

The brute-force oracle must call the already-approved `IntersectTriangle` routine and apply the same lower-index tie rule. The 1,024 rays must come from a deterministic integer hash, not `Random`.

Refactor AABB testing so traversal can receive each child's entry parameter without changing the accepted public `IntersectBounds` behavior.

### 4. Depth-cap validation

The median builder is balanced: for CLR array sizes, its depth cannot naturally reach 64. Do not pretend a real triangle fixture can do so. Factor the depth-cap check into a private validation helper called by `Build`, and exercise that otherwise-unreachable guard through reflection with depth `TraversalStackCapacity`. Assert that depth `TraversalStackCapacity - 1` is accepted and depth `TraversalStackCapacity` is rejected.

This verifies the required guard without weakening the median-split invariant or adding a test-only public API.

### 5. Verification

Run:

```powershell
rtk dotnet test user_settings/generated_code/editor-command/EditorFull/projects/gameplay.tests/gameplay.tests.csproj --no-restore --filter "FullyQualifiedName~SoftwareBvhTests|FullyQualifiedName~SoftwareIntersectionTests" -v:minimal
rtk dotnet build user_settings/generated_code/editor-command/EditorFull/projects/gameplay/gameplay.csproj --no-restore -v:minimal
rtk rg -n "System\.Linq|new int\[|stackalloc|RuntimeModel|RenderManager3D|MeshComponent|File\.Write|OpenWrite" assets/codebase/rendering/SoftwareBvh.cs
rtk git diff --check
```

The `new int[` scan may match builder-owned `TriangleOrder` allocation. It must not match the traversal method or helpers reachable from traversal.

### 6. Commit and report

Commit only:

- `assets/codebase/rendering/SoftwareBvh.cs`
- `assets/codebase/gameplay.tests/SoftwareBvhTests.cs`
- `assets/codebase/gameplay.tests/SoftwareBvhTests.cs.hmeta`

Commit message: `Add deterministic bounded software BVH`.

Report the RED evidence, focused result count, gameplay build result, allocation/forbidden scan, commit hash, and exact changed files.
