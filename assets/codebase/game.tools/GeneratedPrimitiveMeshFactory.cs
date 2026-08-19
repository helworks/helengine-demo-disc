using helengine.editor;

namespace city.game.tools {
    /// <summary>
    /// Builds single-sided primitive meshes for generated gameplay models from the engine's shared gizmo mesh
    /// factory, so generators never hand-roll vertex winding.
    /// </summary>
    public static class GeneratedPrimitiveMeshFactory {
        /// <summary>
        /// Builds one single-sided cylinder aligned to +Y with its base at Y=0.
        /// </summary>
        /// <param name="radius">Cylinder radius in world units.</param>
        /// <param name="height">Cylinder height in world units.</param>
        /// <param name="radialSteps">Segment count used for roundness.</param>
        /// <returns>Single-sided cylinder model.</returns>
        public static ModelAsset CreateSingleSidedCylinderY(float radius, float height, int radialSteps) {
            ModelAsset engineCylinder = TransformGizmoMeshFactory.CreateCylinder(radius, height, radialSteps);
            return new ModelAsset {
                Id = engineCylinder.Id,
                Positions = engineCylinder.Positions,
                Normals = engineCylinder.Normals,
                TexCoords = engineCylinder.TexCoords,
                Indices16 = ExtractSingleSidedTriangles(engineCylinder.Indices16, radialSteps),
                BoundsMin = engineCylinder.BoundsMin,
                BoundsMax = engineCylinder.BoundsMax,
                Submeshes = engineCylinder.Submeshes
            };
        }

        /// <summary>
        /// Builds one single-sided box with its base at Y=0, centered in X and Z.
        /// </summary>
        /// <param name="width">Box width in world units.</param>
        /// <param name="height">Box height in world units.</param>
        /// <param name="depth">Box depth in world units.</param>
        /// <returns>Single-sided box model.</returns>
        public static ModelAsset CreateBox(float width, float height, float depth) {
            return TransformGizmoMeshFactory.CreateBox(width, height, depth);
        }

        /// <summary>
        /// Appends one primitive model into shared mesh buffers, applying one transform to positions and one
        /// rotation-only transform to normals.
        /// </summary>
        /// <param name="positions">Destination position buffer.</param>
        /// <param name="normals">Destination normal buffer.</param>
        /// <param name="texCoords">Destination texture-coordinate buffer.</param>
        /// <param name="indices">Destination index buffer.</param>
        /// <param name="primitive">Primitive model to append.</param>
        /// <param name="transformPosition">Position transform applied to every appended vertex.</param>
        /// <param name="transformNormal">Rotation-only transform applied to every appended normal.</param>
        public static void Append(
            List<float3> positions,
            List<float3> normals,
            List<float2> texCoords,
            List<ushort> indices,
            ModelAsset primitive,
            Func<float3, float3> transformPosition,
            Func<float3, float3> transformNormal) {
            if (positions == null) {
                throw new ArgumentNullException(nameof(positions));
            } else if (normals == null) {
                throw new ArgumentNullException(nameof(normals));
            } else if (texCoords == null) {
                throw new ArgumentNullException(nameof(texCoords));
            } else if (indices == null) {
                throw new ArgumentNullException(nameof(indices));
            } else if (primitive == null) {
                throw new ArgumentNullException(nameof(primitive));
            } else if (transformPosition == null) {
                throw new ArgumentNullException(nameof(transformPosition));
            } else if (transformNormal == null) {
                throw new ArgumentNullException(nameof(transformNormal));
            } else if (primitive.Indices16 == null) {
                throw new InvalidOperationException("Primitive models must provide 16-bit triangle indices.");
            }

            int vertexOffset = positions.Count;
            for (int index = 0; index < primitive.Positions.Length; index++) {
                positions.Add(transformPosition(primitive.Positions[index]));
                normals.Add(transformNormal(primitive.Normals[index]));
                texCoords.Add(primitive.TexCoords[index]);
            }

            for (int index = 0; index < primitive.Indices16.Length; index++) {
                indices.Add((ushort)(vertexOffset + primitive.Indices16[index]));
            }
        }

        /// <summary>
        /// Extracts one single-sided triangle set from the engine cylinder's paired double-sided triangles,
        /// flipping the cap ranges so every face points outward.
        /// </summary>
        /// <param name="sourceIndices">Double-sided triangle indices emitted by the engine cylinder helper.</param>
        /// <param name="radialSteps">Radial step count the cylinder was built with.</param>
        /// <returns>Single-sided outward-facing triangle indices.</returns>
        static ushort[] ExtractSingleSidedTriangles(ushort[] sourceIndices, int radialSteps) {
            if (sourceIndices == null) {
                throw new InvalidOperationException("Engine cylinder generation must provide triangle indices.");
            } else if (sourceIndices.Length % 6 != 0) {
                throw new InvalidOperationException("Expected the engine cylinder helper to emit paired double-sided triangles.");
            }

            ushort[] singleSidedIndices = new ushort[sourceIndices.Length / 2];
            int writeIndex = 0;
            for (int sourceIndex = 0; sourceIndex < sourceIndices.Length; sourceIndex += 6) {
                singleSidedIndices[writeIndex++] = sourceIndices[sourceIndex];
                singleSidedIndices[writeIndex++] = sourceIndices[sourceIndex + 1];
                singleSidedIndices[writeIndex++] = sourceIndices[sourceIndex + 2];
            }

            int sideTriangleCount = radialSteps * 2;
            int bottomCapTriangleStart = sideTriangleCount;
            int topCapTriangleStart = sideTriangleCount + radialSteps;
            FlipTriangleRange(singleSidedIndices, bottomCapTriangleStart, radialSteps);
            FlipTriangleRange(singleSidedIndices, topCapTriangleStart, radialSteps);

            return singleSidedIndices;
        }

        /// <summary>
        /// Reverses the winding of one triangle range in place.
        /// </summary>
        /// <param name="indices">Triangle index buffer to mutate.</param>
        /// <param name="triangleStart">First triangle to flip.</param>
        /// <param name="triangleCount">Number of triangles to flip.</param>
        static void FlipTriangleRange(ushort[] indices, int triangleStart, int triangleCount) {
            for (int triangleIndex = triangleStart; triangleIndex < triangleStart + triangleCount; triangleIndex++) {
                int indexOffset = triangleIndex * 3;
                ushort b = indices[indexOffset + 1];
                indices[indexOffset + 1] = indices[indexOffset + 2];
                indices[indexOffset + 2] = b;
            }
        }
    }
}
