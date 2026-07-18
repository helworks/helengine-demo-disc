using System.Reflection;

namespace city.rendering.tools {
    /// <summary>
    /// Clones generated editor-scene entity graphs in memory so handheld scaffold generation can mutate private copies without round-tripping through scene load services.
    /// </summary>
    public sealed class GeneratedSceneEntityCloneService {
        /// <summary>
        /// Clones the supplied generated scene roots, including editor save metadata required by the scene save pipeline.
        /// </summary>
        /// <param name="sourceRoots">Generated scene roots that should be cloned.</param>
        /// <returns>Detached cloned roots that can be safely mutated and saved.</returns>
        public EditorEntity[] CloneRoots(IReadOnlyList<Entity> sourceRoots) {
            if (sourceRoots == null) {
                throw new ArgumentNullException(nameof(sourceRoots));
            } else if (Core.Instance == null) {
                throw new InvalidOperationException("Cloning generated scene roots requires an active editor core.");
            } else if (Core.Instance.EntityFactory == null) {
                throw new InvalidOperationException("Cloning generated scene roots requires Core.Instance.EntityFactory.");
            }

            List<EditorEntity> clonedRoots = new List<EditorEntity>(sourceRoots.Count);
            for (int index = 0; index < sourceRoots.Count; index++) {
                if (sourceRoots[index] is not EditorEntity sourceRootEntity) {
                    throw new InvalidOperationException("Generated scene roots must be editor entities.");
                }

                if (IsConsoleCameraLightInstructionsBlueprintRoot(sourceRootEntity)) {
                    continue;
                }

                clonedRoots.Add(CloneEntityRecursive(sourceRootEntity));
            }

            return clonedRoots.ToArray();
        }

        /// <summary>
        /// Identifies the console-only instruction Blueprint root that must not be copied into Nintendo DS companion roots.
        /// </summary>
        /// <param name="rootEntity">Generated root being considered for a handheld clone.</param>
        /// <returns>True when the root is the console camera/light Blueprint instance.</returns>
        static bool IsConsoleCameraLightInstructionsBlueprintRoot(EditorEntity rootEntity) {
            if (rootEntity == null || rootEntity.Components == null) {
                return false;
            }

            for (int index = 0; index < rootEntity.Components.Count; index++) {
                if (rootEntity.Components[index] is BlueprintInstanceComponent blueprintInstance
                    && string.Equals(
                        blueprintInstance.BlueprintAssetPath,
                        ConsoleCameraLightInstructionsAssetCatalog.ConsoleCameraLightInstructionsBlueprintRelativePath,
                        StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clones one editor-entity subtree, including visible components and the hidden editor save metadata needed for later serialization.
        /// </summary>
        /// <param name="sourceEntity">Source entity subtree that should be cloned.</param>
        /// <returns>Detached cloned entity subtree.</returns>
        EditorEntity CloneEntityRecursive(EditorEntity sourceEntity) {
            if (sourceEntity == null) {
                throw new ArgumentNullException(nameof(sourceEntity));
            }

            EditorEntity clonedEntity = Core.Instance.EntityFactory.Create(sourceEntity.Name) as EditorEntity;
            if (clonedEntity == null) {
                throw new InvalidOperationException("EntityFactory.Create must return an EditorEntity for generated scene cloning.");
            }

            CopyEntityProperties(sourceEntity, clonedEntity);

            Dictionary<Component, Component> clonedComponentsBySourceComponent = new Dictionary<Component, Component>();
            CloneVisibleComponents(sourceEntity, clonedEntity, clonedComponentsBySourceComponent);
            CloneEntitySaveMetadata(sourceEntity, clonedEntity, clonedComponentsBySourceComponent);

            if (sourceEntity.Children != null) {
                for (int index = 0; index < sourceEntity.Children.Count; index++) {
                    if (sourceEntity.Children[index] is not EditorEntity sourceChildEntity) {
                        continue;
                    }

                    clonedEntity.AddChild(CloneEntityRecursive(sourceChildEntity));
                }
            }

            return clonedEntity;
        }

        /// <summary>
        /// Copies the non-component editor entity properties that should survive generated scene cloning.
        /// </summary>
        /// <param name="sourceEntity">Source entity whose properties should be copied.</param>
        /// <param name="clonedEntity">Cloned entity receiving the copied properties.</param>
        static void CopyEntityProperties(EditorEntity sourceEntity, EditorEntity clonedEntity) {
            if (sourceEntity == null) {
                throw new ArgumentNullException(nameof(sourceEntity));
            } else if (clonedEntity == null) {
                throw new ArgumentNullException(nameof(clonedEntity));
            }

            clonedEntity.Name = sourceEntity.Name;
            clonedEntity.Hidden = sourceEntity.Hidden;
            clonedEntity.InternalEntity = sourceEntity.InternalEntity;
            clonedEntity.Enabled = sourceEntity.Enabled;
            clonedEntity.Static = sourceEntity.Static;
            clonedEntity.LayerMask = sourceEntity.LayerMask;
            clonedEntity.LocalPosition = sourceEntity.LocalPosition;
            clonedEntity.LocalScale = sourceEntity.LocalScale;
            clonedEntity.LocalOrientation = sourceEntity.LocalOrientation;
        }

        /// <summary>
        /// Clones the visible components attached to one entity and records the source-to-clone mapping required by save-state cloning.
        /// </summary>
        /// <param name="sourceEntity">Source entity whose visible components should be cloned.</param>
        /// <param name="clonedEntity">Cloned entity receiving the cloned components.</param>
        /// <param name="clonedComponentsBySourceComponent">Source-to-cloned component mapping built during the clone pass.</param>
        static void CloneVisibleComponents(
            EditorEntity sourceEntity,
            EditorEntity clonedEntity,
            Dictionary<Component, Component> clonedComponentsBySourceComponent) {
            if (sourceEntity == null) {
                throw new ArgumentNullException(nameof(sourceEntity));
            } else if (clonedEntity == null) {
                throw new ArgumentNullException(nameof(clonedEntity));
            } else if (clonedComponentsBySourceComponent == null) {
                throw new ArgumentNullException(nameof(clonedComponentsBySourceComponent));
            }

            if (sourceEntity.Components == null) {
                return;
            }

            for (int index = 0; index < sourceEntity.Components.Count; index++) {
                Component sourceComponent = sourceEntity.Components[index];
                if (sourceComponent == null || sourceComponent is IEditorHiddenComponent) {
                    continue;
                }

                Component clonedComponent = CloneComponent(sourceComponent);
                clonedEntity.AddComponent(clonedComponent);
                clonedComponentsBySourceComponent.Add(sourceComponent, clonedComponent);
            }
        }

        /// <summary>
        /// Clones the hidden editor save metadata attached to one entity so the cloned graph can be serialized without re-inferring asset references or component keys.
        /// </summary>
        /// <param name="sourceEntity">Source entity that owns the original editor save metadata.</param>
        /// <param name="clonedEntity">Cloned entity receiving the copied editor save metadata.</param>
        /// <param name="clonedComponentsBySourceComponent">Source-to-cloned component mapping built for the entity.</param>
        static void CloneEntitySaveMetadata(
            EditorEntity sourceEntity,
            EditorEntity clonedEntity,
            Dictionary<Component, Component> clonedComponentsBySourceComponent) {
            if (sourceEntity == null) {
                throw new ArgumentNullException(nameof(sourceEntity));
            } else if (clonedEntity == null) {
                throw new ArgumentNullException(nameof(clonedEntity));
            } else if (clonedComponentsBySourceComponent == null) {
                throw new ArgumentNullException(nameof(clonedComponentsBySourceComponent));
            }

            EntitySaveComponent sourceSaveComponent = FindRequiredSaveComponent(sourceEntity);
            EntitySaveComponent clonedSaveComponent = FindRequiredSaveComponent(clonedEntity);
            clonedSaveComponent.EntityId = sourceSaveComponent.EntityId;
            clonedSaveComponent.ActiveTransformPlatformId = sourceSaveComponent.ActiveTransformPlatformId;
            clonedSaveComponent.HasCommonTransformSnapshot = sourceSaveComponent.HasCommonTransformSnapshot;
            clonedSaveComponent.CommonLocalPositionSnapshot = sourceSaveComponent.CommonLocalPositionSnapshot;
            clonedSaveComponent.CommonLocalScaleSnapshot = sourceSaveComponent.CommonLocalScaleSnapshot;
            clonedSaveComponent.CommonLocalOrientationSnapshot = sourceSaveComponent.CommonLocalOrientationSnapshot;

            foreach (KeyValuePair<Component, Component> componentPair in clonedComponentsBySourceComponent) {
                if (!sourceSaveComponent.TryGetComponentState(componentPair.Key, out EntityComponentSaveState sourceComponentSaveState)) {
                    continue;
                }

                EntityComponentSaveState clonedComponentSaveState = CloneComponentSaveState(sourceComponentSaveState);
                clonedSaveComponent.GetOrCreateComponentState(componentPair.Value).ComponentKey = clonedComponentSaveState.ComponentKey;
                ApplyComponentSaveState(clonedSaveComponent.GetOrCreateComponentState(componentPair.Value), clonedComponentSaveState);
            }

            foreach (SceneEntityPlatformExistenceOverrideAsset existenceOverride in sourceSaveComponent.EnumerateExistencePlatformOverrides()) {
                if (existenceOverride == null || string.IsNullOrWhiteSpace(existenceOverride.PlatformId)) {
                    continue;
                }

                clonedSaveComponent.SetExistencePlatformOverride(existenceOverride.PlatformId, new SceneEntityPlatformExistenceOverrideAsset {
                    PlatformId = existenceOverride.PlatformId,
                    Exists = existenceOverride.Exists
                });
            }

            foreach (SceneEntityPlatformTransformOverrideAsset transformOverride in sourceSaveComponent.EnumerateTransformPlatformOverrides()) {
                if (transformOverride == null || string.IsNullOrWhiteSpace(transformOverride.PlatformId)) {
                    continue;
                }

                clonedSaveComponent.SetTransformPlatformOverride(transformOverride.PlatformId, new SceneEntityPlatformTransformOverrideAsset {
                    PlatformId = transformOverride.PlatformId,
                    HasLocalPositionOverride = transformOverride.HasLocalPositionOverride,
                    LocalPosition = transformOverride.LocalPosition,
                    HasLocalScaleOverride = transformOverride.HasLocalScaleOverride,
                    LocalScale = transformOverride.LocalScale,
                    HasLocalOrientationOverride = transformOverride.HasLocalOrientationOverride,
                    LocalOrientation = transformOverride.LocalOrientation
                });
            }

            foreach (EntityPlatformComponentOverrideState componentOverride in sourceSaveComponent.EnumerateComponentPlatformOverrides()) {
                if (componentOverride == null || string.IsNullOrWhiteSpace(componentOverride.PlatformId)) {
                    continue;
                }

                ApplyPlatformComponentOverrideState(
                    clonedSaveComponent.GetOrCreateComponentPlatformOverride(componentOverride.PlatformId),
                    componentOverride);
            }
        }

        /// <summary>
        /// Clones one detached component instance by copying its public readable and writable properties.
        /// </summary>
        /// <param name="sourceComponent">Source component that should be cloned.</param>
        /// <returns>Detached cloned component instance.</returns>
        static Component CloneComponent(Component sourceComponent) {
            if (sourceComponent == null) {
                throw new ArgumentNullException(nameof(sourceComponent));
            }

            Component clonedComponent = Activator.CreateInstance(sourceComponent.GetType()) as Component;
            if (clonedComponent == null) {
                throw new InvalidOperationException($"Component type '{sourceComponent.GetType().FullName}' could not be instantiated for generated scene cloning.");
            }

            PropertyInfo[] properties = sourceComponent.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int index = 0; index < properties.Length; index++) {
                PropertyInfo property = properties[index];
                if (!property.CanRead || !property.CanWrite) {
                    continue;
                }
                if (property.GetIndexParameters().Length != 0) {
                    continue;
                }
                if (string.Equals(property.Name, nameof(Component.Parent), StringComparison.Ordinal)) {
                    continue;
                }
                if (property.GetMethod == null || property.SetMethod == null) {
                    continue;
                }
                if (!property.GetMethod.IsPublic || !property.SetMethod.IsPublic) {
                    continue;
                }

                property.SetValue(clonedComponent, property.GetValue(sourceComponent));
            }

            return clonedComponent;
        }

        /// <summary>
        /// Clones one component save-state container, including asset references and per-platform override payloads.
        /// </summary>
        /// <param name="sourceSaveState">Source save-state container that should be cloned.</param>
        /// <returns>Detached cloned save-state container.</returns>
        static EntityComponentSaveState CloneComponentSaveState(EntityComponentSaveState sourceSaveState) {
            if (sourceSaveState == null) {
                throw new ArgumentNullException(nameof(sourceSaveState));
            }

            EntityComponentSaveState clonedSaveState = new EntityComponentSaveState {
                ComponentKey = sourceSaveState.ComponentKey
            };
            ApplyComponentSaveState(clonedSaveState, sourceSaveState);
            return clonedSaveState;
        }

        /// <summary>
        /// Copies one component save-state payload into an existing destination save-state container.
        /// </summary>
        /// <param name="destinationSaveState">Destination save-state container receiving the copied metadata.</param>
        /// <param name="sourceSaveState">Source save-state container that owns the metadata.</param>
        static void ApplyComponentSaveState(EntityComponentSaveState destinationSaveState, EntityComponentSaveState sourceSaveState) {
            if (destinationSaveState == null) {
                throw new ArgumentNullException(nameof(destinationSaveState));
            } else if (sourceSaveState == null) {
                throw new ArgumentNullException(nameof(sourceSaveState));
            }

            destinationSaveState.ComponentKey = sourceSaveState.ComponentKey;
            foreach (KeyValuePair<string, SceneAssetReference> assetReference in sourceSaveState.EnumerateNamedAssetReferences()) {
                destinationSaveState.SetAssetReference(assetReference.Key, assetReference.Value);
            }

            foreach (EntityComponentPlatformOverrideState platformOverride in sourceSaveState.EnumeratePlatformOverrides()) {
                if (platformOverride == null || string.IsNullOrWhiteSpace(platformOverride.PlatformId)) {
                    continue;
                }

                destinationSaveState.SetPlatformOverride(platformOverride.PlatformId, CloneComponentPlatformOverrideState(platformOverride));
            }
        }

        /// <summary>
        /// Clones one per-platform component override payload.
        /// </summary>
        /// <param name="sourceOverrideState">Source override payload that should be cloned.</param>
        /// <returns>Detached cloned override payload.</returns>
        static EntityComponentPlatformOverrideState CloneComponentPlatformOverrideState(EntityComponentPlatformOverrideState sourceOverrideState) {
            if (sourceOverrideState == null) {
                throw new ArgumentNullException(nameof(sourceOverrideState));
            }

            EntityComponentPlatformOverrideState clonedOverrideState = new EntityComponentPlatformOverrideState {
                PlatformId = sourceOverrideState.PlatformId,
                Payload = sourceOverrideState.Payload != null ? (byte[])sourceOverrideState.Payload.Clone() : Array.Empty<byte>()
            };
            foreach (KeyValuePair<string, SceneAssetReference> assetReference in sourceOverrideState.EnumerateNamedAssetReferences()) {
                clonedOverrideState.SetAssetReference(assetReference.Key, assetReference.Value);
            }
            foreach (string propertyOverride in sourceOverrideState.EnumeratePropertyOverrides()) {
                clonedOverrideState.SetPropertyOverride(propertyOverride);
            }
            foreach (KeyValuePair<string, string> memberValue in sourceOverrideState.EnumerateMemberValues()) {
                clonedOverrideState.SetMemberValue(memberValue.Key, memberValue.Value);
            }

            return clonedOverrideState;
        }

        /// <summary>
        /// Clones one platform component override container, including detached added components and their save-state metadata.
        /// </summary>
        /// <param name="sourceOverrideState">Source platform component override container that should be cloned.</param>
        /// <returns>Detached cloned platform component override container.</returns>
        static EntityPlatformComponentOverrideState ClonePlatformComponentOverrideState(EntityPlatformComponentOverrideState sourceOverrideState) {
            if (sourceOverrideState == null) {
                throw new ArgumentNullException(nameof(sourceOverrideState));
            }

            EntityPlatformComponentOverrideState clonedOverrideState = new EntityPlatformComponentOverrideState {
                PlatformId = sourceOverrideState.PlatformId
            };
            ApplyPlatformComponentOverrideState(clonedOverrideState, sourceOverrideState);
            return clonedOverrideState;
        }

        /// <summary>
        /// Copies one platform component override container into an existing destination instance.
        /// </summary>
        /// <param name="destinationOverrideState">Destination override container receiving the copied data.</param>
        /// <param name="sourceOverrideState">Source override container that owns the data.</param>
        static void ApplyPlatformComponentOverrideState(
            EntityPlatformComponentOverrideState destinationOverrideState,
            EntityPlatformComponentOverrideState sourceOverrideState) {
            if (destinationOverrideState == null) {
                throw new ArgumentNullException(nameof(destinationOverrideState));
            } else if (sourceOverrideState == null) {
                throw new ArgumentNullException(nameof(sourceOverrideState));
            }

            destinationOverrideState.PlatformId = sourceOverrideState.PlatformId;
            foreach (string removedComponentKey in sourceOverrideState.EnumerateRemovedComponentKeys()) {
                destinationOverrideState.MarkComponentRemoved(removedComponentKey);
            }
            foreach (EntityPlatformAddedComponentState addedComponentState in sourceOverrideState.EnumerateAddedComponents()) {
                if (addedComponentState == null || addedComponentState.Component == null || addedComponentState.SaveState == null) {
                    continue;
                }

                destinationOverrideState.SetAddedComponent(new EntityPlatformAddedComponentState {
                    ComponentKey = addedComponentState.ComponentKey,
                    Component = CloneComponent(addedComponentState.Component),
                    SaveState = CloneComponentSaveState(addedComponentState.SaveState)
                });
            }
        }

        /// <summary>
        /// Resolves the hidden editor save component attached to one editor entity.
        /// </summary>
        /// <param name="entity">Entity that should own one hidden editor save component.</param>
        /// <returns>Resolved hidden editor save component.</returns>
        static EntitySaveComponent FindRequiredSaveComponent(EditorEntity entity) {
            if (entity == null) {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Components != null) {
                for (int index = 0; index < entity.Components.Count; index++) {
                    if (entity.Components[index] is EntitySaveComponent saveComponent) {
                        return saveComponent;
                    }
                }
            }

            throw new InvalidOperationException("Generated scene entities must carry an EntitySaveComponent.");
        }
    }
}
