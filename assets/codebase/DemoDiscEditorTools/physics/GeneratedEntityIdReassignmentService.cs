using System.Reflection;
using helengine.editor;
namespace DemoDisc.EditorTools {
    /// <summary>Assigns scene-local identifiers while keeping references within generated editor subtrees connected.</summary>
    public sealed class GeneratedEntityIdReassignmentService {
        /// <summary>Plans fresh IDs before rewriting each reference once, retaining references outside the subtree.</summary>
        public void Reassign(EditorEntity root, Func<uint> allocate, Func<EditorEntity, EntitySaveComponent> resolveSave) {
            Dictionary<uint, uint> ids = new Dictionary<uint, uint>();
            List<EditorEntity> entities = new List<EditorEntity>();
            Assign(root ?? throw new ArgumentNullException(nameof(root)), allocate, resolveSave, ids, entities);
            HashSet<SceneEntityReference> references = new HashSet<SceneEntityReference>();
            foreach (EditorEntity entity in entities) {
                foreach (Component component in entity.Components) {
                    foreach (PropertyInfo property in component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        if (!property.CanRead) { continue; }
                        if (property.PropertyType == typeof(SceneEntityReference)) {
                            Remap((SceneEntityReference)property.GetValue(component), ids, references);
                        } else if (property.PropertyType == typeof(SceneEntityReference[])) {
                            foreach (SceneEntityReference reference in (SceneEntityReference[])property.GetValue(component) ?? []) { Remap(reference, ids, references); }
                        }
                    }
                }
            }
        }
        /// <summary>Assigns identifiers across the tree and records the original identity of existing entities.</summary>
        void Assign(EditorEntity entity, Func<uint> allocate, Func<EditorEntity, EntitySaveComponent> resolveSave, Dictionary<uint, uint> ids, List<EditorEntity> entities) {
            EntitySaveComponent save = resolveSave(entity);
            uint previous = save.EntityId;
            save.EntityId = allocate();
            if (previous != 0u) { ids.Add(previous, save.EntityId); }
            entities.Add(entity);
            foreach (Entity child in entity.Children ?? []) {
                if (child is EditorEntity editorChild) { Assign(editorChild, allocate, resolveSave, ids, entities); }
            }
        }
        /// <summary>Updates one borrowed reference once, avoiding repeated remaps when components share it.</summary>
        void Remap(SceneEntityReference reference, Dictionary<uint, uint> ids, HashSet<SceneEntityReference> references) {
            if (reference != null && references.Add(reference) && ids.TryGetValue(reference.EntityId, out uint replacement)) { reference.EntityId = replacement; }
        }
    }
}
