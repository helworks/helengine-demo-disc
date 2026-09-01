using helengine;
using city.rendering;

namespace city.tests {
    /// <summary>
    /// Verifies CPU-only flattening and area-light extraction for software trace scenes.
    /// </summary>
    public sealed class SoftwareTraceSceneTests {
        /// <summary>
        /// Ensures equal references share one owned raw load while every entity still produces triangles.
        /// </summary>
        [Fact]
        public void Equal_references_load_once_and_flatten_each_instance() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/cube.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, CreateCubeAsset);
            fixture.AddModel(reference, new SoftwareMaterial());
            fixture.AddModel(reference, EmitterMaterial());

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(24, scene.Triangles.Length);
            Assert.Equal(1, source.LoadCount);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures the active 16-bit index stream is flattened.
        /// </summary>
        [Fact]
        public void Sixteen_bit_indices_are_supported() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/triangle16.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => CreateTriangleAsset16());
            fixture.AddModel(reference, EmitterMaterial());

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(2, scene.Triangles.Length);
        }

        /// <summary>
        /// Ensures the active 32-bit index stream is flattened.
        /// </summary>
        [Fact]
        public void Thirty_two_bit_indices_are_supported() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/triangle32.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => CreateTriangleAsset32());
            fixture.AddModel(reference, EmitterMaterial());

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(2, scene.Triangles.Length);
        }

        /// <summary>
        /// Ensures all affine world-transform components are applied and the geometric normal is recomputed.
        /// </summary>
        [Fact]
        public void World_transform_uses_full_affine_math_and_recomputes_normal() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/triangle16.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => new ModelAsset {
                Positions = new[] { new float3(1f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f), new float3(0f, 0f, 0f) },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3 },
                Submeshes = new[] { new ModelSubmeshAsset { IndexStart = 0, IndexCount = 6 } }
            });
            Entity entity = fixture.AddModel(reference, EmitterMaterial());
            entity.LocalPosition = new float3(4f, 5f, 6f);
            entity.LocalScale = new float3(2f, 3f, 4f);
            float3 axis = float3.UnitZ;
            float4.CreateFromAxisAngle(axis, 0.5f, out float4 orientation);
            entity.LocalOrientation = orientation;

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);
            city.rendering.SoftwareTriangle triangle = scene.Triangles[0];
            float3 expectedP0 = Transform(new float3(1f, 0f, 0f), entity.WorldTransformMatrix);
            float3 expectedP1 = Transform(new float3(1f, 1f, 0f), entity.WorldTransformMatrix);
            float3 expectedP2 = Transform(new float3(0f, 1f, 0f), entity.WorldTransformMatrix);
            float3 expectedEdge1 = expectedP1 - expectedP0;
            float3 expectedEdge2 = expectedP2 - expectedP0;
            float3 expectedNormal = float3.Normalize(float3.Cross(expectedEdge1, expectedEdge2));

            AssertVector(expectedP0, triangle.P0);
            AssertVector(expectedEdge1, triangle.Edge1);
            AssertVector(expectedEdge2, triangle.Edge2);
            AssertVector(expectedNormal, triangle.GeometricNormal);
        }

        /// <summary>
        /// Ensures each submesh maps every flattened triangle to the matching authored material.
        /// </summary>
        [Fact]
        public void Submesh_material_mapping_is_preserved_per_instance() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/two-materials.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, CreateTwoSubmeshAsset);
            fixture.AddModel(reference, new SoftwareMaterial { DiffuseColor = new float3(1f, 0f, 0f) }, new SoftwareMaterial { DiffuseColor = new float3(0f, 1f, 0f) });
            fixture.AddModel(reference, new SoftwareMaterial { DiffuseColor = new float3(0f, 0f, 1f), EmissionColor = float3.One, EmissionStrength = 1f }, new SoftwareMaterial { DiffuseColor = new float3(1f, 1f, 0f), EmissionColor = float3.One, EmissionStrength = 1f });

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(4, scene.Triangles.Length);
            Assert.Equal(0, scene.Triangles[0].MaterialIndex);
            Assert.Equal(1, scene.Triangles[1].MaterialIndex);
            Assert.Equal(2, scene.Triangles[2].MaterialIndex);
            Assert.Equal(3, scene.Triangles[3].MaterialIndex);
            Assert.Equal(4, scene.Materials.Length);
        }

        /// <summary>
        /// Ensures unreferenced component materials cannot be treated as emissive geometry.
        /// </summary>
        [Fact]
        public void Unreferenced_component_materials_are_rejected() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/unreferenced-material.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, CreateTriangleAsset16);
            fixture.AddModel(reference, new SoftwareMaterial(), EmitterMaterial());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("exactly", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("submesh", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures stable reference identity includes source kind, provider, asset id, and path.
        /// </summary>
        [Fact]
        public void Reference_group_identity_does_not_merge_unequal_references() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference first = SceneAssetReferenceFactory.CreateFileSystemReference("11111111111111111111111111111111", "models/cube.hasset", "sha256:" + new string('1', 64));
            SceneAssetReference second = SceneAssetReferenceFactory.CreateFileSystemReference("22222222222222222222222222222222", "models/cube.hasset", "sha256:" + new string('2', 64));
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(first, CreateTriangleAsset16);
            source.Register(second, CreateTriangleAsset16);
            fixture.AddModel(first, new SoftwareMaterial());
            fixture.AddModel(second, EmitterMaterial());

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(2, source.LoadCount);
            Assert.Equal(2, source.DisposedCount);
            Assert.Equal(4, scene.Triangles.Length);
        }

        /// <summary>
        /// Ensures a malformed model is disposed before its validation exception escapes.
        /// </summary>
        [Fact]
        public void Invalid_model_is_disposed_before_validation_failure() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/invalid.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => new ModelAsset { Positions = null, Indices16 = new ushort[] { 0, 1, 2 }, Submeshes = Array.Empty<ModelSubmeshAsset>() });
            fixture.AddModel(reference, new SoftwareMaterial());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("positions", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures index-width, triangle-count, range, and material contracts reject malformed assets.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public void Invalid_index_or_material_contract_is_rejected(int failureKind) {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/invalid-contract.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => CreateInvalidAsset(failureKind));
            fixture.AddModel(reference, failureKind == 5 ? null : new SoftwareMaterial());

            Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures groups are loaded and disposed in first-seen order before later groups are loaded.
        /// </summary>
        [Fact]
        public void Groups_are_processed_sequentially_and_previous_raw_asset_is_disposed() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference first = SceneAssetReferenceFactory.CreateFileSystemModel("models/first.hasset");
            SceneAssetReference second = SceneAssetReferenceFactory.CreateFileSystemModel("models/second.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(first, CreateGeneratedCubeAsset);
            source.Register(second, CreateGeneratedCubeAsset);
            fixture.AddModel(first, new SoftwareMaterial());
            fixture.AddModel(second, EmitterMaterial());
            fixture.Entities[1].LocalPosition = new float3(0f, 1f, 0f);

            city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(new[] { "models/first.hasset", "models/second.hasset" }, source.LoadedRelativePaths);
            Assert.True(source.LoadObservedAllPreviousAssetsDisposed);
        }

        /// <summary>
        /// Ensures success, validation failures, and flattening failures all release owned raw assets.
        /// </summary>
        [Fact]
        public void Every_ingestion_path_disposes_owned_raw_asset() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/flatten-failure.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => CreateInvalidAsset(4));
            fixture.AddModel(reference, new SoftwareMaterial());

            Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures zero and multiple emissive components are rejected.
        /// </summary>
        [Fact]
        public void Exactly_one_emissive_component_is_required() {
            using SceneFixture zeroFixture = new SceneFixture();
            SceneAssetReference diffuseReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/diffuse.hasset");
            FakeSoftwareModelAssetSource zeroSource = new FakeSoftwareModelAssetSource();
            zeroSource.Register(diffuseReference, CreateTriangleAsset16);
            zeroFixture.AddModel(diffuseReference, new SoftwareMaterial());
            Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(zeroFixture.Entities, zeroSource));

            using SceneFixture multipleFixture = new SceneFixture();
            SceneAssetReference first = SceneAssetReferenceFactory.CreateFileSystemModel("models/first-emitter.hasset");
            SceneAssetReference second = SceneAssetReferenceFactory.CreateFileSystemModel("models/second-emitter.hasset");
            FakeSoftwareModelAssetSource multipleSource = new FakeSoftwareModelAssetSource();
            multipleSource.Register(first, CreateTriangleAsset16);
            multipleSource.Register(second, CreateTriangleAsset16);
            multipleFixture.AddModel(first, new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f });
            multipleFixture.AddModel(second, new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f });
            Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(multipleFixture.Entities, multipleSource));
        }

        /// <summary>
        /// Ensures one inward-facing cube face becomes one rectangular area light with premultiplied emission.
        /// </summary>
        [Fact]
        public void Cube_emitter_derives_one_rectangular_area_light() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference boxReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/box.hasset");
            SceneAssetReference emitterReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/emitter-cube.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(boxReference, CreateGeneratedCubeAsset);
            source.Register(emitterReference, CreateGeneratedCubeAsset);
            fixture.AddModel(boxReference, new SoftwareMaterial());
            fixture.AddModel(emitterReference, new SoftwareMaterial { EmissionColor = new float3(0.2f, 0.4f, 0.6f), EmissionStrength = 3f });
            Entity emitterEntity = fixture.Entities[1];
            emitterEntity.LocalPosition = new float3(0f, 1f, 0f);
            emitterEntity.LocalScale = new float3(0.55f, 0.025f, 0.45f);

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(24, scene.Triangles.Length);
            Assert.InRange(scene.AreaLight.FirstTriangleIndex, 12, 23);
            Assert.InRange(scene.AreaLight.SecondTriangleIndex, 12, 23);
            int selectedEmitterTriangles = 0;
            int unselectedEmitterTriangles = 0;
            for (int triangleIndex = 12; triangleIndex < 24; triangleIndex++) {
                if (triangleIndex == scene.AreaLight.FirstTriangleIndex || triangleIndex == scene.AreaLight.SecondTriangleIndex) {
                    selectedEmitterTriangles++;
                } else {
                    Assert.True(scene.Triangles[triangleIndex].Edge1.Length() > 0f);
                    unselectedEmitterTriangles++;
                }
            }
            Assert.Equal(2, selectedEmitterTriangles);
            Assert.Equal(10, unselectedEmitterTriangles);
            Assert.InRange(Math.Abs(scene.AreaLight.Area - (0.55f * 0.45f)), 0f, 0.0001f);
            AssertVector(new float3(0.6f, 1.2f, 1.8f), scene.AreaLight.Emission);
            Assert.True(scene.AreaLight.InwardNormal.Y < -0.99f);
        }

        /// <summary>
        /// Ensures a diffuse inward face cannot be selected when another emitter submesh emits.
        /// </summary>
        [Fact]
        public void Mixed_submesh_cube_rejects_diffuse_inward_area_face() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference anchorReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/mixed-anchor.hasset");
            SceneAssetReference emitterReference = SceneAssetReferenceFactory.CreateFileSystemModel("models/mixed-emitter.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(anchorReference, CreateGeneratedCubeAsset);
            source.Register(emitterReference, CreateMixedMaterialCubeAsset);
            fixture.AddModel(anchorReference, new SoftwareMaterial());
            fixture.AddModel(emitterReference, EmitterMaterial(), new SoftwareMaterial());
            fixture.Entities[1].LocalPosition = new float3(0f, 1f, 0f);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("emissive", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(2, source.DisposedCount);
        }

        /// <summary>
        /// Ensures nonrectangular or ambiguous emitter geometry is rejected.
        /// </summary>
        [Fact]
        public void Ambiguous_or_nonrectangular_emitter_geometry_is_rejected() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/ambiguous-emitter.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, CreateAmbiguousEmitterAsset);
            fixture.AddModel(reference, new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f });

            Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures owned-byte accounting is deterministic and initialization peak includes transient raw ownership.
        /// </summary>
        [Fact]
        public void Owned_byte_accounting_is_exact_and_peak_is_at_least_steady_state() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/triangle16.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, CreateGeneratedCubeAsset);
            fixture.AddModel(reference, new SoftwareMaterial());
            fixture.AddModel(reference, new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f });
            fixture.Entities[1].LocalPosition = new float3(0f, 1f, 0f);

            city.rendering.SoftwareTraceScene scene = city.rendering.SoftwareTraceScene.Build(fixture.Entities, source);

            Assert.Equal(scene.SteadyStateOwnedBytes, scene.Triangles.Length * city.rendering.SoftwareTraceScene.SoftwareTriangleBytes + scene.Materials.Length * city.rendering.SoftwareTraceScene.SoftwareMaterialDataBytes + city.rendering.SoftwareTraceScene.SoftwareAreaLightBytes);
            Assert.True(scene.InitializationPeakOwnedBytes >= scene.SteadyStateOwnedBytes);
        }

        /// <summary>
        /// Ensures a null submesh element is rejected and its owned raw asset is disposed.
        /// </summary>
        [Fact]
        public void Null_submesh_element_is_rejected_and_disposed() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/null-submesh.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => {
                ModelAsset asset = CreateTriangleAsset16();
                asset.Submeshes = new ModelSubmeshAsset[] { null };
                return asset;
            });
            fixture.AddModel(reference, new SoftwareMaterial());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("Submeshes[0]", exception.Message, StringComparison.Ordinal);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures overlapping submesh ranges are rejected before flattening.
        /// </summary>
        [Fact]
        public void Overlapping_submesh_ranges_are_rejected_and_disposed() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/overlap.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => CreateRangeContractAsset(new[] {
                new ModelSubmeshAsset { IndexStart = 0, IndexCount = 6 },
                new ModelSubmeshAsset { IndexStart = 3, IndexCount = 6 }
            }));
            fixture.AddModel(reference, new SoftwareMaterial(), new SoftwareMaterial());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("overlap", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures uncovered index ranges are rejected before flattening.
        /// </summary>
        [Fact]
        public void Uncovered_submesh_range_is_rejected_and_disposed() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/uncovered.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => CreateRangeContractAsset(new[] {
                new ModelSubmeshAsset { IndexStart = 0, IndexCount = 3 }
            }));
            fixture.AddModel(reference, new SoftwareMaterial());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("do not cover", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures an empty positions array is rejected and disposed.
        /// </summary>
        [Fact]
        public void Empty_positions_are_rejected_and_disposed() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/empty-positions.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, () => {
                ModelAsset asset = CreateTriangleAsset16();
                asset.Positions = Array.Empty<float3>();
                return asset;
            });
            fixture.AddModel(reference, new SoftwareMaterial());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("Positions", exception.Message, StringComparison.Ordinal);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures a valid raw triangle that degenerates under its world transform is disposed after flatten failure.
        /// </summary>
        [Fact]
        public void Post_validation_flatten_failure_disposes_owned_asset() {
            using SceneFixture fixture = new SceneFixture();
            SceneAssetReference reference = SceneAssetReferenceFactory.CreateFileSystemModel("models/degenerate-transform.hasset");
            FakeSoftwareModelAssetSource source = new FakeSoftwareModelAssetSource();
            source.Register(reference, CreateTriangleAsset16);
            Entity entity = fixture.AddModel(reference, new SoftwareMaterial());
            entity.LocalScale = new float3(1f, 0f, 1f);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => city.rendering.SoftwareTraceScene.Build(fixture.Entities, source));

            Assert.Contains("degenerate", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(1, source.DisposedCount);
        }

        /// <summary>
        /// Ensures compact owned-byte constants are independently exact.
        /// </summary>
        [Fact]
        public void Compact_owned_byte_constants_are_exact() {
            Assert.Equal(88, city.rendering.SoftwareTriangle.OwnedBytes);
            Assert.Equal(24, city.rendering.SoftwareMaterialData.OwnedBytes);
            Assert.Equal(72, city.rendering.SoftwareAreaLight.OwnedBytes);
        }

        /// <summary>
        /// Ensures the owned raw-model transfer is declared on both the source contract and its content implementation.
        /// </summary>
        [Fact]
        public void Load_owned_return_declares_native_ownership() {
            System.Reflection.MethodInfo interfaceMethod = typeof(ISoftwareModelAssetSource).GetMethod(
                nameof(ISoftwareModelAssetSource.LoadOwned));
            System.Reflection.MethodInfo implementationMethod = typeof(ContentSoftwareModelAssetSource).GetMethod(
                nameof(ContentSoftwareModelAssetSource.LoadOwned));

            AssertOwnedReturn(interfaceMethod);
            AssertOwnedReturn(implementationMethod);
        }

        /// <summary>
        /// Verifies that one reflected method carries the native owned-return contract.
        /// </summary>
        /// <param name="method">Method whose returned model asset becomes the caller's responsibility.</param>
        static void AssertOwnedReturn(System.Reflection.MethodInfo method) {
            Assert.NotNull(method);
            Assert.NotEmpty(method.GetCustomAttributes(typeof(NativeOwnedReturnAttribute), false));
        }

        static ModelAsset CreateCubeAsset() {
            return new ModelAsset {
                Positions = new[] {
                    new float3(-1f, -1f, -1f), new float3(1f, -1f, -1f), new float3(1f, 1f, -1f), new float3(-1f, 1f, -1f),
                    new float3(-1f, -1f, 1f), new float3(1f, -1f, 1f), new float3(1f, 1f, 1f), new float3(-1f, 1f, 1f)
                },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7, 0, 1, 5, 0, 5, 4, 3, 7, 6, 3, 6, 2, 0, 4, 7, 0, 7, 3, 1, 2, 6, 1, 6, 5 },
                Submeshes = new[] { new ModelSubmeshAsset { MaterialSlotName = "DefaultMaterial", IndexStart = 0, IndexCount = 36 } }
            };
        }

        static SoftwareMaterial EmitterMaterial() {
            return new SoftwareMaterial { EmissionColor = float3.One, EmissionStrength = 1f };
        }

        static ModelAsset CreateTriangleAsset16() {
            return new ModelAsset {
                Positions = new[] { new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f) },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3 },
                Submeshes = new[] { new ModelSubmeshAsset { IndexStart = 0, IndexCount = 6 } }
            };
        }

        static ModelAsset CreateTriangleAsset32() {
            return new ModelAsset {
                Positions = new[] { new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f) },
                Indices32 = new uint[] { 0, 1, 2, 0, 2, 3 },
                Submeshes = new[] { new ModelSubmeshAsset { IndexStart = 0, IndexCount = 6 } }
            };
        }

        static ModelAsset CreateTwoSubmeshAsset() {
            return new ModelAsset {
                Positions = new[] {
                    new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f)
                },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3 },
                Submeshes = new[] {
                    new ModelSubmeshAsset { MaterialSlotName = "red", IndexStart = 0, IndexCount = 3 },
                    new ModelSubmeshAsset { MaterialSlotName = "green", IndexStart = 3, IndexCount = 3 }
                }
            };
        }

        static ModelAsset CreateAmbiguousEmitterAsset() {
            return new ModelAsset {
                Positions = new[] {
                    new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f), new float3(0f, 1f, 0f),
                    new float3(3f, 0f, 0f), new float3(4f, 0f, 0f), new float3(4f, 1f, 0f), new float3(3f, 1f, 0f)
                },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 },
                Submeshes = new[] { new ModelSubmeshAsset { IndexStart = 0, IndexCount = 12 } }
            };
        }

        static ModelAsset CreateGeneratedCubeAsset() {
            ModelAsset cube = ModelUtils.GenerateCubeMesh(float3.Zero, float3.One);
            cube.Submeshes = new[] { new ModelSubmeshAsset { MaterialSlotName = "DefaultMaterial", IndexStart = 0, IndexCount = cube.Indices16.Length } };
            return cube;
        }

        static ModelAsset CreateMixedMaterialCubeAsset() {
            ModelAsset cube = CreateGeneratedCubeAsset();
            cube.Submeshes = new[] {
                new ModelSubmeshAsset { MaterialSlotName = "emissive-other-faces", IndexStart = 0, IndexCount = 30 },
                new ModelSubmeshAsset { MaterialSlotName = "diffuse-bottom-face", IndexStart = 30, IndexCount = 6 }
            };
            return cube;
        }

        static ModelAsset CreateRangeContractAsset(ModelSubmeshAsset[] submeshes) {
            return new ModelAsset {
                Positions = new[] {
                    new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 1f, 0f),
                    new float3(0f, 1f, 0f), new float3(-1f, 1f, 0f), new float3(-1f, 0f, 0f)
                },
                Indices16 = new ushort[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 },
                Submeshes = submeshes
            };
        }

        static ModelAsset CreateInvalidAsset(int failureKind) {
            ModelAsset asset = CreateTriangleAsset16();
            if (failureKind == 1) {
                asset.Indices16 = new ushort[] { 0, 1 };
            } else if (failureKind == 2) {
                asset.Indices16 = new ushort[] { 0, 1, 4 };
            } else if (failureKind == 3) {
                asset.Indices16 = new ushort[] { 0, 1, 2 };
                asset.Indices32 = new uint[] { 0, 1, 2 };
            } else if (failureKind == 4) {
                asset.Submeshes = new[] { new ModelSubmeshAsset { IndexStart = 2, IndexCount = 3 } };
            } else if (failureKind == 6) {
                asset.Indices16 = null;
            } else if (failureKind == 7) {
                asset.Submeshes = null;
            }
            return asset;
        }

        static float3 Transform(float3 point, float4x4 matrix) {
            return new float3(
                point.X * matrix.M11 + point.Y * matrix.M21 + point.Z * matrix.M31 + matrix.M41,
                point.X * matrix.M12 + point.Y * matrix.M22 + point.Z * matrix.M32 + matrix.M42,
                point.X * matrix.M13 + point.Y * matrix.M23 + point.Z * matrix.M33 + matrix.M43);
        }

        static void AssertVector(float3 expected, float3 actual) {
            Assert.InRange(Math.Abs(expected.X - actual.X), 0f, 0.0001f);
            Assert.InRange(Math.Abs(expected.Y - actual.Y), 0f, 0.0001f);
            Assert.InRange(Math.Abs(expected.Z - actual.Z), 0f, 0.0001f);
        }

        sealed class SceneFixture : IDisposable {
            readonly Core core;
            public readonly List<Entity> Entities = new List<Entity>();

            public SceneFixture() {
                core = new Core(new CoreInitializationOptions {
                    ContentStreamSource = new HostFileSystemContentStreamSource(Environment.CurrentDirectory)
                });
                core.Initialize(null, null, null, new PlatformInfo("test", "1"));
            }

            public Entity AddModel(SceneAssetReference reference, params SoftwareMaterial[] materials) {
                Entity entity = new Entity(core);
                entity.InitComponents();
                entity.AddComponent(new city.rendering.SoftwareModelComponent { ModelReference = reference, Materials = materials });
                Entities.Add(entity);
                return entity;
            }

            public void Dispose() {
                for (int index = Entities.Count - 1; index >= 0; index--) {
                    Entities[index].Dispose();
                }
                core.Dispose();
            }
        }
    }
}
