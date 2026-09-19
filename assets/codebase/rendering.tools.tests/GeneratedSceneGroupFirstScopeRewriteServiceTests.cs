using helengine;
using helengine.editor;

namespace city.tests {
    /// <summary>
    /// Covers <see cref="city.rendering.tools.GeneratedSceneGroupFirstScopeRewriteService"/>: when a generated
    /// subtree adopts the group-first level order, every scope that names a platform must be re-pathed through
    /// that platform's group chain, because an override only applies to a build target whose path it prefixes.
    /// <para>
    /// This rewrite belongs in the engine, not in the project. The editor owns the level order and the group
    /// tree, so relocating an authored override when an entity's level order changes — inserting the group chain
    /// the new order demands — is engine behaviour that every project needs. When the engine grows that,
    /// <c>GeneratedSceneGroupFirstScopeRewriteService</c> and this test class are to be deleted.
    /// </para>
    /// </summary>
    public sealed class GeneratedSceneGroupFirstScopeRewriteServiceTests {
        /// <summary>
        /// Component key used by the component-existence and property-override fixtures.
        /// </summary>
        const string ComponentKey = "test-component-key";

        /// <summary>
        /// Property path marked as overridden by the per-component property fixture.
        /// </summary>
        const string OverriddenPropertyPath = "Size";

        /// <summary>
        /// Rewrites every store on one entity and reports the group-prefixed paths each now holds.
        /// </summary>
        [Fact]
        public void Rewrite_moves_every_platform_scope_onto_its_group_chain() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity entity = CreateEntityWithDefaultOrderOverrides(authoringSession, out Component component);
            EntitySaveComponent saveComponent = FindSaveComponent(entity);

            CreateRewriteService().RewriteSubtree(entity);

            Assert.Equal(
                "group:consoles/platform:ps2",
                SceneOverrideScopePath.Format(Assert.Single(saveComponent.EnumerateExistencePlatformOverrides()).Scope));
            Assert.Equal(
                "group:desktop/platform:windows/buildconfig:release",
                SceneOverrideScopePath.Format(Assert.Single(saveComponent.EnumerateTransformPlatformOverrides()).Scope));
            Assert.Equal(
                "group:consoles/platform:n64",
                Assert.Single(saveComponent.EnumerateComponentPlatformOverrides()).Scope.ToString());
            Assert.True(saveComponent.TryGetComponentState(component, out EntityComponentSaveState componentSaveState));
            Assert.Equal(
                "group:consoles/platform:ps2",
                Assert.Single(componentSaveState.EnumeratePlatformOverrides()).Scope.ToString());
        }

        /// <summary>
        /// The old one-step paths must be gone, otherwise a stale entry could out-rank the rewritten one.
        /// </summary>
        [Fact]
        public void Rewrite_leaves_nothing_behind_at_the_one_step_platform_paths() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity entity = CreateEntityWithDefaultOrderOverrides(authoringSession, out Component component);
            EntitySaveComponent saveComponent = FindSaveComponent(entity);

            CreateRewriteService().RewriteSubtree(entity);

            Assert.False(saveComponent.TryGetExistencePlatformOverride(EditorOverrideScope.ForPlatform("ps2"), out _));
            Assert.False(saveComponent.TryGetTransformPlatformOverride(
                EditorOverrideScope.ForPlatformBuildConfig("windows", "release"),
                out _));
            Assert.False(saveComponent.TryGetComponentPlatformOverride(EditorOverrideScope.ForPlatform("n64"), out _));
            Assert.True(saveComponent.TryGetComponentState(component, out EntityComponentSaveState componentSaveState));
            Assert.False(componentSaveState.HasScopedPlatformOverride(EditorOverrideScope.ForPlatform("ps2")));
        }

        /// <summary>
        /// The payloads must survive the move, not just the paths.
        /// </summary>
        [Fact]
        public void Rewrite_preserves_the_payload_each_moved_override_carried() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity entity = CreateEntityWithDefaultOrderOverrides(authoringSession, out Component component);
            EntitySaveComponent saveComponent = FindSaveComponent(entity);

            CreateRewriteService().RewriteSubtree(entity);

            Assert.False(Assert.Single(saveComponent.EnumerateExistencePlatformOverrides()).Exists);
            SceneEntityPlatformTransformOverrideAsset transformOverride = Assert.Single(saveComponent.EnumerateTransformPlatformOverrides());
            Assert.True(transformOverride.HasLocalPositionOverride);
            Assert.Equal(40f, transformOverride.LocalPosition.X);
            Assert.True(Assert.Single(saveComponent.EnumerateComponentPlatformOverrides()).IsComponentRemoved(ComponentKey));
            Assert.True(saveComponent.TryGetComponentState(component, out EntityComponentSaveState componentSaveState));
            Assert.True(Assert.Single(componentSaveState.EnumeratePlatformOverrides()).HasPropertyOverride(OverriddenPropertyPath));
        }

        /// <summary>
        /// A platform in no group contributes an empty chain, so its one-step path is already the group-first
        /// target path and must be left exactly as authored.
        /// </summary>
        [Fact]
        public void Rewrite_leaves_an_ungrouped_platform_scope_unchanged() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity entity = (EditorEntity)authoringSession.OwningCore.EntityFactory.Create("UngroupedFixture");
            EntitySaveComponent saveComponent = FindSaveComponent(entity);
            saveComponent.GetOrCreateExistencePlatformOverride(EditorOverrideScope.ForPlatform("atari-jaguar")).Exists = false;

            CreateRewriteService().RewriteSubtree(entity);

            Assert.Equal(
                "platform:atari-jaguar",
                SceneOverrideScopePath.Format(Assert.Single(saveComponent.EnumerateExistencePlatformOverrides()).Scope));
        }

        /// <summary>
        /// Common carries no platform step, so it is already valid under the group-first order.
        /// </summary>
        [Fact]
        public void Rewrite_leaves_the_common_scope_unchanged() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity entity = (EditorEntity)authoringSession.OwningCore.EntityFactory.Create("CommonFixture");
            EntitySaveComponent saveComponent = FindSaveComponent(entity);
            saveComponent.GetOrCreateExistencePlatformOverride(EditorOverrideScope.Common).Exists = false;

            CreateRewriteService().RewriteSubtree(entity);

            Assert.Equal(
                SceneOverrideScopePath.CommonLabel,
                SceneOverrideScopePath.Format(Assert.Single(saveComponent.EnumerateExistencePlatformOverrides()).Scope));
        }

        /// <summary>
        /// A second pass must not append the group chain again: the rewritten paths already name their group.
        /// </summary>
        [Fact]
        public void Rewriting_twice_is_a_no_op() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity entity = CreateEntityWithDefaultOrderOverrides(authoringSession, out Component component);
            EntitySaveComponent saveComponent = FindSaveComponent(entity);
            city.rendering.tools.GeneratedSceneGroupFirstScopeRewriteService rewriteService = CreateRewriteService();

            rewriteService.RewriteSubtree(entity);
            rewriteService.RewriteSubtree(entity);

            Assert.Equal(
                "group:consoles/platform:ps2",
                SceneOverrideScopePath.Format(Assert.Single(saveComponent.EnumerateExistencePlatformOverrides()).Scope));
            Assert.Equal(
                "group:desktop/platform:windows/buildconfig:release",
                SceneOverrideScopePath.Format(Assert.Single(saveComponent.EnumerateTransformPlatformOverrides()).Scope));
            Assert.Equal(
                "group:consoles/platform:n64",
                Assert.Single(saveComponent.EnumerateComponentPlatformOverrides()).Scope.ToString());
            Assert.True(saveComponent.TryGetComponentState(component, out EntityComponentSaveState componentSaveState));
            Assert.Equal(
                "group:consoles/platform:ps2",
                Assert.Single(componentSaveState.EnumeratePlatformOverrides()).Scope.ToString());
        }

        /// <summary>
        /// Descendants are rewritten too, because the whole subtree adopts the group-first order together.
        /// </summary>
        [Fact]
        public void Rewrite_reaches_descendants() {
            string projectRootPath = CreateTemporaryProjectRoot();
            using TestGeneratedAssetGraph graph = new TestGeneratedAssetGraph(projectRootPath);
            IEditorProjectAuthoringSession authoringSession = graph.CreateAuthoringSession(projectRootPath);
            EditorEntity rootEntity = (EditorEntity)authoringSession.OwningCore.EntityFactory.Create("ParentFixture");
            EditorEntity childEntity = (EditorEntity)authoringSession.OwningCore.EntityFactory.Create("ChildFixture");
            rootEntity.AddChild(childEntity);
            FindSaveComponent(childEntity).GetOrCreateExistencePlatformOverride(EditorOverrideScope.ForPlatform("ps2")).Exists = false;

            CreateRewriteService().RewriteSubtree(rootEntity);

            Assert.Equal(
                "group:consoles/platform:ps2",
                SceneOverrideScopePath.Format(Assert.Single(FindSaveComponent(childEntity).EnumerateExistencePlatformOverrides()).Scope));
        }

        /// <summary>
        /// Builds the rewrite service over the real demo-disc platform group tree, which is the tree the
        /// generators author against.
        /// </summary>
        /// <returns>Rewrite service bound to the demo-disc project settings.</returns>
        static city.rendering.tools.GeneratedSceneGroupFirstScopeRewriteService CreateRewriteService() {
            return new city.rendering.tools.GeneratedSceneGroupFirstScopeRewriteService(global::city.testing.DemoDiscTestProject.RootPath);
        }

        /// <summary>
        /// Builds one entity carrying an override in each of the four stores, all authored on one-step platform
        /// paths under the project's default level order.
        /// </summary>
        /// <param name="authoringSession">Authoring session that owns the entity factory.</param>
        /// <param name="component">Component whose save state carries the per-component property override.</param>
        /// <returns>Entity holding the default-order override fixtures.</returns>
        static EditorEntity CreateEntityWithDefaultOrderOverrides(IEditorProjectAuthoringSession authoringSession, out Component component) {
            EditorEntity entity = (EditorEntity)authoringSession.OwningCore.EntityFactory.Create("RewriteFixture");
            CameraComponent cameraComponent = new CameraComponent();
            entity.AddComponent(cameraComponent);
            component = cameraComponent;

            EntitySaveComponent saveComponent = FindSaveComponent(entity);
            saveComponent.GetOrCreateExistencePlatformOverride(EditorOverrideScope.ForPlatform("ps2")).Exists = false;

            SceneEntityPlatformTransformOverrideAsset transformOverride = saveComponent.GetOrCreateTransformPlatformOverride(
                EditorOverrideScope.ForPlatformBuildConfig("windows", "release"));
            transformOverride.HasLocalPositionOverride = true;
            transformOverride.LocalPosition = new float3(40f, 0f, 0f);

            saveComponent.GetOrCreateComponentPlatformOverride(EditorOverrideScope.ForPlatform("n64")).MarkComponentRemoved(ComponentKey);

            EntityComponentSaveState componentSaveState = saveComponent.GetOrCreateComponentState(cameraComponent);
            componentSaveState.ComponentKey = ComponentKey;
            componentSaveState.GetOrCreateScopedPlatformOverride(EditorOverrideScope.ForPlatform("ps2")).SetPropertyOverride(OverriddenPropertyPath);
            return entity;
        }

        /// <summary>
        /// Resolves the hidden save component every editor entity carries.
        /// </summary>
        /// <param name="entity">Entity whose save component should be returned.</param>
        /// <returns>Attached hidden save component.</returns>
        static EntitySaveComponent FindSaveComponent(EditorEntity entity) {
            for (int index = 0; index < entity.Components.Count; index++) {
                if (entity.Components[index] is EntitySaveComponent saveComponent) {
                    return saveComponent;
                }
            }

            throw new InvalidOperationException("Editor entities must carry one EntitySaveComponent.");
        }

        /// <summary>
        /// Creates one throwaway project root for the editor core the fixture needs.
        /// </summary>
        /// <returns>Absolute path to a fresh temporary project root.</returns>
        static string CreateTemporaryProjectRoot() {
            return Path.Combine(Path.GetTempPath(), "city-group-first-scope-rewrite-tests", Guid.NewGuid().ToString("N"));
        }
    }
}
