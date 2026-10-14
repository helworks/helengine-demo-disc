using helengine.editor;
using System.Text.Json;
using System.IO.Compression;
using System.Security.Cryptography;
namespace DemoDisc.EditorTools {
    /// <summary>Validates the exported Android scene graph without launching a player or emulator.</summary>
    public sealed class ValidateAndroidDemoDiscCommand : IEditorCommand {
        /// <summary>Gets the command identifier used for package reference checks.</summary>
        public string CommandId => "menu.validate-android-demo-disc";
        /// <summary>Gets the validation command label.</summary>
        public string DisplayName => "Validate Android DemoDisc";
        /// <summary>Checks all catalog scenes, their entity references and the single Tilt gameplay controller/camera invariant.</summary>
        /// <param name="context">Authoring context that supplies the current script type resolver.</param>
        public void Execute(IEditorCommandContext context) {
            string reportPath = Environment.GetEnvironmentVariable("HELENGINE_ANDROID_BUILD_REPORT_PATH");
            if (string.IsNullOrWhiteSpace(reportPath)) { throw new InvalidOperationException("HELENGINE_ANDROID_BUILD_REPORT_PATH must identify the verified Android APK build report."); }
            using JsonDocument report = JsonDocument.Parse(File.ReadAllText(reportPath));
            string apkPath = report.RootElement.GetProperty("apkPath").GetString();
            using (FileStream apk = File.OpenRead(apkPath)) {
                string hash = Convert.ToHexString(SHA256.HashData(apk)).ToLowerInvariant();
                if (hash != report.RootElement.GetProperty("sha256").GetString()) { throw new InvalidOperationException("The APK does not match its verified build report."); }
            }
            using ZipArchive archive = ZipFile.OpenRead(apkPath);
            using Stream manifestStream = (archive.GetEntry("assets/android-content.json") ?? throw new InvalidOperationException("APK has no authored content manifest.")).Open();
            using JsonDocument manifest = JsonDocument.Parse(manifestStream);
            ComponentPersistenceRegistry registry = GeneratedScenePersistenceRegistryFactory.Create(context.ScriptTypeResolver);
            int sceneCount = 0;
            int referenceCount = 0;
            foreach (JsonElement entry in manifest.RootElement.GetProperty("Scenes").EnumerateArray()) {
                string sceneId = entry.GetProperty("SceneId").GetString();
                string relativePath = entry.GetProperty("SourceIdentity").GetString();
                using Stream source = (archive.GetEntry("assets/" + relativePath) ?? throw new InvalidOperationException("APK is missing scene " + relativePath)).Open();
                using MemoryStream stream = new MemoryStream();
                source.CopyTo(stream);
                stream.Position = 0;
                SceneAsset scene = (SceneAsset)global::helengine.AssetSerializer.Deserialize(stream);
                List<SceneEntityAsset> entities = new List<SceneEntityAsset>();
                HashSet<uint> ids = new HashSet<uint>();
                foreach (SceneEntityAsset root in scene.RootEntities) { Collect(root, entities, ids); }
                Dictionary<uint, SceneEntityAsset> entitiesById = entities.ToDictionary(entity => entity.Id);
                int sessions = 0;
                int cameras = 0;
                foreach (SceneEntityAsset entity in entities) {
                    foreach (SceneComponentAssetRecord record in entity.Components) {
                        if (record.ComponentTypeId == "helengine.CameraComponent") { cameras++; }
                        bool tiltSession = record.ComponentTypeId == "DemoDisc.TiltPlay.TiltTrialSessionComponent, TiltPlay";
                        if (tiltSession) { sessions++; }
                        referenceCount += ValidateReferences(sceneId, record, registry, entitiesById, tiltSession);
                    }
                }
                if (sceneId.StartsWith("tilt_trial_level_", StringComparison.Ordinal) && (sessions != 1 || cameras != 1)) {
                    throw new InvalidOperationException($"{sceneId} must contain one gameplay session and one camera; found {sessions} sessions and {cameras} cameras.");
                }
                sceneCount++;
                Console.WriteLine($"Validated Android scene {sceneId}: entities={entities.Count} cameras={cameras} sessions={sessions}.");
            }
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(reportPath), "android-content-validation.json"), JsonSerializer.Serialize(new {
                ApkSha256 = report.RootElement.GetProperty("sha256").GetString(), SceneCount = sceneCount, EntityReferenceCount = referenceCount,
                ValidatedAtUtc = DateTime.UtcNow.ToString("o"), MissingReferencesDetected = false, TiltPresentationValidated = true
            }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Android package reference validation passed: scenes={sceneCount} references={referenceCount}.");
        }
        /// <summary>Collects each authored entity and rejects ambiguous or missing identifiers.</summary>
        void Collect(SceneEntityAsset entity, List<SceneEntityAsset> entities, HashSet<uint> ids) {
            if (entity.Id == 0u || !ids.Add(entity.Id)) { throw new InvalidOperationException($"Cooked scene entity ID {entity.Id} must be nonzero and unique."); }
            entities.Add(entity);
            foreach (SceneEntityAsset child in entity.Children) { Collect(child, entities, ids); }
        }
        /// <summary>Reads only reference fields, avoiding asset materialization or gameplay callbacks during the check.</summary>
        int ValidateReferences(string sceneId, SceneComponentAssetRecord record, ComponentPersistenceRegistry registry, Dictionary<uint, SceneEntityAsset> entitiesById, bool tiltSession) {
            Type componentType = registry.ResolveComponentType(record.ComponentTypeId);
            ScriptComponentReflectionSchema schema = new ScriptComponentReflectionSchemaBuilder().Build(componentType);
            Dictionary<string, Type> fields = schema.Members.Where(member => member.ValueType == typeof(SceneEntityReference) || member.ValueType == typeof(SceneEntityReference[])).ToDictionary(member => member.Name, member => member.ValueType);
            if (fields.Count == 0) { return 0; }
            using MemoryStream payload = new MemoryStream(record.Payload, false);
            using EngineBinaryReader reader = EngineBinaryReader.Create(payload, EngineBinaryEndianness.LittleEndian);
            byte version = reader.ReadByte();
            if (version != AutomaticScriptComponentRuntimeDeserializer.CurrentVersion) { throw new InvalidOperationException("Cooked reference component must use the current runtime payload: " + record.ComponentTypeId); }
            int fieldCount = reader.ReadInt32();
            if (fieldCount != schema.Members.Count) { throw new InvalidOperationException("Cooked component member count differs from its script schema: " + record.ComponentTypeId); }
            ScenePersistenceValueWalker walker = new ScenePersistenceValueWalker(new EditorScenePersistenceAssetValueReader(), "Android package validation");
            int count = 0;
            HashSet<string> boundRequiredFields = new HashSet<string>();
            for (int index = 0; index < fieldCount; index++) {
                ScriptComponentReflectionMember member = schema.Members[index];
                string fieldName = member.Name;
                if (AutomaticComponentAssetReferenceSupport.IsSupportedAssetReferenceType(member.ValueType)) {
                    SceneComponentBinaryFieldEncoding.ReadOptionalReference(reader);
                    continue;
                } else if (AutomaticComponentAssetReferenceSupport.IsSupportedAssetReferenceArrayType(member.ValueType)) {
                    int assets = reader.ReadInt32();
                    for (int assetIndex = 0; assetIndex < assets; assetIndex++) { SceneComponentBinaryFieldEncoding.ReadOptionalReference(reader); }
                    continue;
                }
                object value = walker.ReadValue(reader, member.ValueType);
                if (!fields.ContainsKey(fieldName)) { continue; }
                SceneEntityReference[] references = member.ValueType == typeof(SceneEntityReference[]) ? (SceneEntityReference[])value ?? [] : [(SceneEntityReference)value];
                foreach (SceneEntityReference reference in references) {
                    if (reference == null || reference.EntityId == 0u) { continue; }
                    if (!entitiesById.TryGetValue(reference.EntityId, out SceneEntityAsset target)) { throw new InvalidOperationException($"{sceneId}: {record.ComponentTypeId}.{fieldName} references missing entity {reference.EntityId}."); }
                    if (tiltSession) { ValidateTiltTarget(sceneId, fieldName, target); }
                    boundRequiredFields.Add(fieldName);
                    count++;
                }
            }
            if (payload.Position != payload.Length) { throw new InvalidOperationException("Reference component contains trailing data: " + record.ComponentTypeId); }
            if (tiltSession) {
                foreach (string name in new[] { "PlayerSphereReference", "GoalEntityReference", "StageRootReference", "TimerTextReference", "CoinTextReference", "StartOverlayReference", "ResultsOverlayReference", "FailOverlayReference" }) {
                    if (!boundRequiredFields.Contains(name)) { throw new InvalidOperationException($"{sceneId}: Tilt session requires its bound {name}."); }
                }
            }
            return count;
        }
        /// <summary>Checks that the three external gameplay bindings point to the intended kind of scene entity.</summary>
        void ValidateTiltTarget(string sceneId, string fieldName, SceneEntityAsset target) {
            string requiredType = fieldName == "PlayerSphereReference" ? "helengine.RigidBody3DComponent"
                : fieldName == "StageRootReference" ? "DemoDisc.TiltPlay.DemoTiltStageComponent, TiltPlay"
                : fieldName == "GoalEntityReference" ? "helengine.SceneEntityTriggerObserverComponent" : string.Empty;
            if (requiredType.Length > 0 && !ContainsComponent(target, requiredType)) { throw new InvalidOperationException($"{sceneId}: {fieldName} must target an entity containing {requiredType}."); }
        }
        /// <summary>Finds a required component within a bound Blueprint instance subtree.</summary>
        bool ContainsComponent(SceneEntityAsset entity, string componentTypeId) {
            if (entity.Components.Any(record => record.ComponentTypeId == componentTypeId)) { return true; }
            foreach (SceneEntityAsset child in entity.Children) { if (ContainsComponent(child, componentTypeId)) { return true; } }
            return false;
        }
    }
}
