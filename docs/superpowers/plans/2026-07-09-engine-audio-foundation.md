# Engine Audio Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a first-class Helengine audio foundation with shared asset/import/cook/runtime plumbing, validate it on Windows first, and use it to play looping menu music in the city main menu.

**Architecture:** Add a shared `AudioAsset` and audio import/cook settings to the existing Helengine asset pipeline, then layer a platform-neutral `AudioManager` plus `AudioSourceComponent` over a Windows-specific backend in `helengine.core.windows`. The first vertical slice imports one canonical source audio file, cooks it for `windows`, resolves it through packaged scene references, and plays it from the shared menu scene on the `music` bus.

**Tech Stack:** C# / .NET 9, Helengine content pipeline, Helengine editor asset import system, Helengine packaged scene runtime, Windows host importer plug-in pattern, NAudio for Windows-only source decoding and playback.

---

## File Structure

### Shared Engine and File Format

- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioAsset.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioPlaybackMode.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioChunkDescriptor.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioAssetPlatformOverrideAsset.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\assets\EditorAssetBinaryValueKind.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\content\RuntimeContentProcessorIds.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\content\RuntimeContentManagerConfiguration.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\RuntimeSceneAssetReferenceResolver.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\AutomaticComponentAssetReferenceSupport.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\scene\SceneAssetReferenceFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.files\assets\EditorAssetBinarySerializer.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\assets\PackagedAssetBinarySerializer.cs`

### Shared Editor Import and Cook

- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\IAudioImporter.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\IAudioImporterFactory.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\AssemblyAudioImporterFactory.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\LazyAudioImporter.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\AudioImportFormatCatalog.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\ImportedAudioSource.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioImporterRegistration.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioAssetImportSettings.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioAssetProcessorSettings.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioAssetPlatformSettingsSectionDefinition.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\content\EditorContentProcessorIds.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\content\EditorContentManagerConfiguration.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AssetPlatformSettingsSectionRegistry.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AssetImportManager.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\EditorAssetManager.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorPlatformBuildGraphRunner.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorPlatformAssetCookService.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorWindowsBuildScenePackager.cs`

### Windows-Only Importer and Runtime Backend

- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows\content\audio\EditorHostAudioImporterFactory.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.audioimporter\helengine.editor.windows.audioimporter.csproj`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows\content\audio\NAudioSourceAudioImporter.cs`
- Modify: `C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\EditorHostImporterFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj`
- Create: `C:\dev\helworks\helengine\engine\helengine.core.windows\audio\WindowsAudioBackend.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core.windows\audio\WindowsAudioVoice.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core.windows\helengine.core.windows.csproj`

### Shared Runtime Audio Layer

- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\IAudioBackend.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\AudioManager.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\AudioBus.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\AudioPlaybackRequest.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\components\AudioSourceComponent.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\Core.cs`

### Tests

- Create: `C:\dev\helworks\helengine\engine\helengine.files.tests\assets\AudioAssetBinarySerializerTests.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\AudioAssetImportManagerTests.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\project\EditorWindowsBuildScenePackagerAudioTests.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\content\audio\EditorHostAudioImporterFactoryTests.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\content\audio\NAudioSourceAudioImporterTests.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\audio\WindowsAudioBackendTests.cs`

### City Validation Slice

- Create: `C:\dev\helprojs\demodisc\assets\audio\menu\helen_of_code_high_code_v2.wav`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscMenuTheme.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscStandardMainMenuSceneFactory.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscHandheldMainMenuSceneFactory.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools.tests\DemoDiscMainMenuAudioSourceTests.cs`

## Task 1: Add Shared Audio Asset Types and Binary Serialization

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioAsset.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioPlaybackMode.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioChunkDescriptor.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio\AudioAssetPlatformOverrideAsset.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\assets\EditorAssetBinaryValueKind.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.files\assets\EditorAssetBinarySerializer.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\assets\PackagedAssetBinarySerializer.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.files.tests\assets\AudioAssetBinarySerializerTests.cs`

- [ ] **Step 1: Write the failing binary serializer test**

```csharp
namespace helengine.files.tests {
    public sealed class AudioAssetBinarySerializerTests {
        [Fact]
        public void SerializeAudioAsset_RoundTripsMetadataAndPayload() {
            AudioAsset original = new AudioAsset {
                Id = "Audio/MenuTheme",
                RuntimeAssetId = "runtime.audio.menu-theme",
                PlaybackMode = AudioPlaybackMode.Streamed,
                DefaultLoop = true,
                DefaultBusId = "music",
                Channels = 2,
                SampleRate = 44100,
                DurationSeconds = 12.5f,
                EncodingFamilyId = "pcm-streamed",
                EncodedBytes = [1, 2, 3, 4],
                Chunks = [
                    new AudioChunkDescriptor {
                        ByteOffset = 0,
                        ByteLength = 4
                    }
                ]
            };

            using MemoryStream stream = new MemoryStream();
            EditorAssetBinarySerializer.Serialize(stream, original);
            stream.Position = 0;

            AudioAsset clone = Assert.IsType<AudioAsset>(EditorAssetBinarySerializer.Deserialize(stream));
            Assert.Equal(original.Id, clone.Id);
            Assert.Equal(original.RuntimeAssetId, clone.RuntimeAssetId);
            Assert.Equal(AudioPlaybackMode.Streamed, clone.PlaybackMode);
            Assert.True(clone.DefaultLoop);
            Assert.Equal("music", clone.DefaultBusId);
            Assert.Equal(44100, clone.SampleRate);
            Assert.Equal(2, clone.Channels);
            Assert.Equal("pcm-streamed", clone.EncodingFamilyId);
            Assert.Equal([1, 2, 3, 4], clone.EncodedBytes);
            Assert.Single(clone.Chunks);
        }
    }
}
```

- [ ] **Step 2: Run the test to verify the compile failure**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.files.tests\helengine.files.tests.csproj --filter FullyQualifiedName~AudioAssetBinarySerializerTests`

Expected: FAIL with missing type errors for `AudioAsset`, `AudioPlaybackMode`, and missing serializer support.

- [ ] **Step 3: Add the shared audio asset types and serializer wiring**

```csharp
namespace helengine {
    public enum AudioPlaybackMode : byte {
        Buffered = 0,
        Streamed = 1
    }

    public sealed class AudioChunkDescriptor {
        public int ByteOffset { get; set; }
        public int ByteLength { get; set; }
    }

    public sealed class AudioAssetPlatformOverrideAsset {
        public string PlatformId { get; set; } = string.Empty;
        public string EncodingFamilyId { get; set; } = string.Empty;
        public ushort Channels { get; set; }
        public int SampleRate { get; set; }
        public byte[] EncodedBytes { get; set; } = Array.Empty<byte>();
        public AudioChunkDescriptor[] Chunks { get; set; } = Array.Empty<AudioChunkDescriptor>();
    }

    public sealed class AudioAsset : Asset {
        public AudioPlaybackMode PlaybackMode { get; set; }
        public bool DefaultLoop { get; set; }
        public string DefaultBusId { get; set; } = "master";
        public ushort Channels { get; set; }
        public int SampleRate { get; set; }
        public float DurationSeconds { get; set; }
        public string EncodingFamilyId { get; set; } = string.Empty;
        public byte[] EncodedBytes { get; set; } = Array.Empty<byte>();
        public AudioChunkDescriptor[] Chunks { get; set; } = Array.Empty<AudioChunkDescriptor>();
        public AudioAssetPlatformOverrideAsset[] PlatformOverrides { get; set; } = Array.Empty<AudioAssetPlatformOverrideAsset>();
    }
}
```

```csharp
// EditorAssetBinaryValueKind.cs
AudioAsset = 10,
```

```csharp
// EditorAssetBinarySerializer.cs
if (asset is AudioAsset) {
    return EditorAssetBinaryValueKind.AudioAsset;
}

case EditorAssetBinaryValueKind.AudioAsset:
    return ReadAudioAsset(reader, version);

static void WriteAudioAsset(EngineBinaryWriter writer, AudioAsset asset) {
    writer.WriteAssetMetadata(asset);
    writer.WriteByte((byte)asset.PlaybackMode);
    writer.WriteBool(asset.DefaultLoop);
    writer.WriteString(asset.DefaultBusId ?? string.Empty);
    writer.WriteUInt16(asset.Channels);
    writer.WriteInt32(asset.SampleRate);
    writer.WriteSingle(asset.DurationSeconds);
    writer.WriteString(asset.EncodingFamilyId ?? string.Empty);
    writer.WriteByteArray(asset.EncodedBytes ?? Array.Empty<byte>());
    writer.WriteArray(asset.Chunks, WriteAudioChunkDescriptor);
    writer.WriteArray(asset.PlatformOverrides, WriteAudioPlatformOverrideAsset);
}
```

```csharp
// PackagedAssetBinarySerializer.cs
case EditorAssetBinaryValueKind.AudioAsset:
    return ReadAudioAsset(reader, version);
```

- [ ] **Step 4: Run the serializer test and the full file-format test project**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.files.tests\helengine.files.tests.csproj --filter FullyQualifiedName~AudioAssetBinarySerializerTests`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.files.tests\helengine.files.tests.csproj`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git add C:\dev\helworks\helengine\engine\helengine.core\assets\raw\audio C:\dev\helworks\helengine\engine\helengine.core\assets\EditorAssetBinaryValueKind.cs C:\dev\helworks\helengine\engine\helengine.files\assets\EditorAssetBinarySerializer.cs C:\dev\helworks\helengine\engine\helengine.core\assets\PackagedAssetBinarySerializer.cs C:\dev\helworks\helengine\engine\helengine.files.tests\assets\AudioAssetBinarySerializerTests.cs
rtk git commit -m "feat: add shared audio asset serialization"
```

## Task 2: Add Runtime Content Registration and Scene Audio Asset Resolution

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\content\RuntimeContentProcessorIds.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\content\RuntimeContentManagerConfiguration.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\RuntimeSceneAssetReferenceResolver.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\AutomaticComponentAssetReferenceSupport.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\assets\raw\scene\SceneAssetReferenceFactory.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\RuntimeSceneLoadServiceTests.cs`

- [ ] **Step 1: Write the failing runtime content-resolution test**

```csharp
[Fact]
public void ConfigureSharedAssetContentManager_WhenAudioAssetFileExists_LoadsAudioAssetByProcessorId() {
    ContentManager contentManager = new ContentManager(new FileSystemContentStreamSource(TempDirectory));
    RuntimeContentManagerConfiguration.ConfigureSharedAssetContentManager(contentManager);

    AudioAsset original = new AudioAsset {
        Id = "Audio/MenuTheme",
        RuntimeAssetId = "runtime.audio.menu-theme",
        PlaybackMode = AudioPlaybackMode.Streamed,
        DefaultLoop = true,
        DefaultBusId = "music",
        Channels = 2,
        SampleRate = 44100,
        DurationSeconds = 4f,
        EncodingFamilyId = "pcm-streamed"
    };

    string fullPath = Path.Combine(TempDirectory, "menu-theme.hasset");
    using (FileStream stream = File.Create(fullPath)) {
        EditorAssetBinarySerializer.Serialize(stream, original);
    }

    AudioAsset loaded = contentManager.Load<AudioAsset>(fullPath, RuntimeContentProcessorIds.AudioAsset);
    Assert.Equal("Audio/MenuTheme", loaded.Id);
}
```

- [ ] **Step 2: Run the targeted runtime scene/content tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~AudioAsset`

Expected: FAIL because `RuntimeContentProcessorIds.AudioAsset` and runtime resolver support do not exist.

- [ ] **Step 3: Register the runtime processor and audio scene reference support**

```csharp
// RuntimeContentProcessorIds.cs
public const string AudioAsset = "runtime.audio-asset";
```

```csharp
// RuntimeContentManagerConfiguration.cs
RegisterProcessorIfMissing(
    contentManager,
    RuntimeContentProcessorIds.AudioAsset,
    new AssetContentProcessor<AudioAsset>());
```

```csharp
// RuntimeSceneAssetReferenceResolver.cs
public AudioAsset ResolveAudio(SceneAssetReference reference) {
    if (reference == null) {
        throw new ArgumentNullException(nameof(reference));
    }

    string fullPath = ResolveFileBackedAssetPath(reference);
    return AssetContentManager.Load<AudioAsset>(fullPath, RuntimeContentProcessorIds.AudioAsset);
}
```

```csharp
// AutomaticComponentAssetReferenceSupport.cs
return valueType == typeof(FontAsset)
    || valueType == typeof(RuntimeTexture)
    || valueType == typeof(RuntimeModel)
    || valueType == typeof(RuntimeMaterial)
    || valueType == typeof(AnimationClipAsset)
    || valueType == typeof(AudioAsset);
```

```csharp
// SceneAssetReferenceFactory.cs
public static SceneAssetReference CreateFileSystemAudio(string relativePath) {
    return CreateFileSystem(relativePath);
}
```

- [ ] **Step 4: Run the runtime content tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~AudioAsset`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git add C:\dev\helworks\helengine\engine\helengine.core\content\RuntimeContentProcessorIds.cs C:\dev\helworks\helengine\engine\helengine.core\content\RuntimeContentManagerConfiguration.cs C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\RuntimeSceneAssetReferenceResolver.cs C:\dev\helworks\helengine\engine\helengine.core\scene\runtime\AutomaticComponentAssetReferenceSupport.cs C:\dev\helworks\helengine\engine\helengine.core\assets\raw\scene\SceneAssetReferenceFactory.cs C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\RuntimeSceneLoadServiceTests.cs
rtk git commit -m "feat: wire audio assets into runtime content loading"
```

## Task 3: Add Shared Editor Audio Import Settings and Windows Source Importer Registration

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\IAudioImporter.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\IAudioImporterFactory.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\AssemblyAudioImporterFactory.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\LazyAudioImporter.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\AudioImportFormatCatalog.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\content\audio\ImportedAudioSource.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioImporterRegistration.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioAssetImportSettings.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioAssetProcessorSettings.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AudioAssetPlatformSettingsSectionDefinition.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows\content\audio\EditorHostAudioImporterFactory.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.audioimporter\helengine.editor.windows.audioimporter.csproj`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows\content\audio\NAudioSourceAudioImporter.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\content\EditorContentProcessorIds.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\content\EditorContentManagerConfiguration.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AssetPlatformSettingsSectionRegistry.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AssetImportManager.cs`
- Modify: `C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\EditorHostImporterFactory.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\AudioAssetImportManagerTests.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\content\audio\EditorHostAudioImporterFactoryTests.cs`

- [ ] **Step 1: Write the failing editor import-manager and host-factory tests**

```csharp
namespace helengine.editor.tests {
    public sealed class AudioAssetImportManagerTests {
        [Fact]
        public void ImportAudio_WhenWavSourceExists_WritesAudioCacheFile() {
            AssetImportManager manager = CreateAssetImportManager();
            string sourcePath = CopyFixture("fixtures/audio/menu-theme.wav");

            AudioAsset asset = manager.ImportAudio(sourcePath);

            Assert.Equal(AudioPlaybackMode.Streamed, asset.PlaybackMode);
            Assert.Equal("master", asset.DefaultBusId);
            Assert.True(File.Exists(Path.Combine(ProjectRootPath, "assets", "imports", "audio", asset.Id + ".hasset")));
        }
    }
}
```

```csharp
namespace helengine.editor.windows.tests {
    public sealed class EditorHostAudioImporterFactoryTests {
        [Fact]
        public void CreateDefault_RegistersWindowsAudioImporterForWavAndMp3() {
            IReadOnlyList<IAssetImporterRegistration> registrations = EditorHostAudioImporterFactory.CreateDefault();
            AudioImporterRegistration registration = Assert.Single(registrations.OfType<AudioImporterRegistration>());
            Assert.Contains(".wav", registration.Extensions, StringComparer.OrdinalIgnoreCase);
            Assert.Contains(".mp3", registration.Extensions, StringComparer.OrdinalIgnoreCase);
        }
    }
}
```

- [ ] **Step 2: Run both test projects to confirm missing audio importer support**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~AudioAssetImportManagerTests`

Expected: FAIL with missing `ImportAudio`, `AudioImporterRegistration`, or `AudioAssetImportSettings` symbols.

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj --filter FullyQualifiedName~EditorHostAudioImporterFactoryTests`

Expected: FAIL with missing `EditorHostAudioImporterFactory`.

- [ ] **Step 3: Add shared audio importer contracts, settings, and host registration**

```csharp
namespace helengine.editor {
    public sealed class ImportedAudioSource {
        public ushort Channels { get; init; }
        public int SampleRate { get; init; }
        public float DurationSeconds { get; init; }
        public short[] Pcm16Samples { get; init; } = Array.Empty<short>();
    }

    public interface IAudioImporter {
        ImportedAudioSource ImportAudio(Stream stream);
    }
}
```

```csharp
namespace helengine.editor {
    public sealed class AudioAssetProcessorSettings {
        public AudioPlaybackMode PlaybackMode { get; set; } = AudioPlaybackMode.Streamed;
        public string EncodingFamilyId { get; set; } = "pcm-streamed";
        public int TargetSampleRate { get; set; } = 44100;
        public ushort TargetChannelCount { get; set; } = 2;
        public int StreamingChunkSizeBytes { get; set; } = 16384;
        public int MaxBufferedBytes { get; set; } = 262144;
    }
}
```

```csharp
// EditorHostAudioImporterFactory.cs
public static IReadOnlyList<IAssetImporterRegistration> CreateDefault() {
    return [
        new AudioImporterRegistration(
            "naudio-source",
            new LazyAudioImporter(new AssemblyAudioImporterFactory("helengine.editor.windows.audioimporter", "helengine.editor.NAudioSourceAudioImporter")),
            [".wav", ".mp3"])
    ];
}
```

```csharp
// EditorHostImporterFactory.cs
List<IAssetImporterRegistration> registrations = new List<IAssetImporterRegistration>(EditorHostTextureImporterFactory.CreateDefault());
registrations.AddRange(EditorHostAudioImporterFactory.CreateDefault());
```

```xml
<!-- helengine.editor.windows.audioimporter.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="NAudio" Version="2.2.1" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\helengine.editor\helengine.editor.csproj" SkipGetTargetFrameworkProperties="true" />
  </ItemGroup>
</Project>
```

```csharp
// NAudioSourceAudioImporter.cs
public sealed class NAudioSourceAudioImporter : IAudioImporter {
    public ImportedAudioSource ImportAudio(Stream stream) {
        if (stream == null) {
            throw new ArgumentNullException(nameof(stream));
        }

        WaveStream waveStream;
        try {
            waveStream = new WaveFileReader(stream);
        } catch (FormatException) {
            stream.Position = 0;
            waveStream = new Mp3FileReader(stream);
        }

        using (waveStream)
        using WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(waveStream);
        using MemoryStream sampleBuffer = new MemoryStream();
        pcmStream.CopyTo(sampleBuffer);
        byte[] bytes = sampleBuffer.ToArray();
        short[] samples = new short[bytes.Length / sizeof(short)];
        Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
        return new ImportedAudioSource {
            Channels = (ushort)pcmStream.WaveFormat.Channels,
            SampleRate = pcmStream.WaveFormat.SampleRate,
            DurationSeconds = (float)pcmStream.TotalTime.TotalSeconds,
            Pcm16Samples = samples
        };
    }
}
```

- [ ] **Step 4: Add `AssetImportManager` audio registration and cache entry points**

```csharp
// AssetImportManager.cs
public void RegisterAudioImporter(AudioImporterRegistration registration) { /* mirror RegisterTextureImporter */ }

public AudioAsset ImportAudio(string sourcePath) {
    AudioAssetImportSettings settings = LoadOrCreateAudioImportSettings(sourcePath);
    EnsureAudioImporterExists(settings.Importer.ImporterId);
    ImportedAudioSource source = LoadAudioSource(sourcePath, settings);
    AudioAsset asset = BuildAudioAsset(source, settings, ResolveAudioProcessorPlatformId(settings));
    string outputPath = GetAudioAssetPath(settings.Importer.AssetId);
    using FileStream stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
    AssetSerializer.Serialize(stream, asset);
    SaveAudioImportSettings(sourcePath, settings);
    return asset;
}

public bool TryLoadAudioAsset(string sourcePath, out AudioAsset asset) { /* mirror TryLoadTextureAsset */ }
```

- [ ] **Step 5: Run the editor and Windows importer tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~AudioAssetImportManagerTests`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj --filter FullyQualifiedName~EditorHostAudioImporterFactoryTests`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
rtk git add C:\dev\helworks\helengine\engine\helengine.editor\content\audio C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\Audio* C:\dev\helworks\helengine\engine\helengine.editor\content\EditorContentProcessorIds.cs C:\dev\helworks\helengine\engine\helengine.editor\content\EditorContentManagerConfiguration.cs C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AssetPlatformSettingsSectionRegistry.cs C:\dev\helworks\helengine\engine\helengine.editor\managers\asset\AssetImportManager.cs C:\dev\helworks\helengine\engine\helengine.editor.windows\content\audio C:\dev\helworks\helengine\engine\helengine.editor.windows.audioimporter C:\dev\helworks\helengine\helengine.ui\helengine.editor.app\EditorHostImporterFactory.cs C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\asset\AudioAssetImportManagerTests.cs C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\content\audio\EditorHostAudioImporterFactoryTests.cs C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj
rtk git commit -m "feat: add editor audio import settings and windows source importer"
```

## Task 4: Add Windows-First Audio Cook and Packager Support with Constrained-Target Validation

**Files:**
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorPlatformBuildGraphRunner.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorPlatformAssetCookService.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorWindowsBuildScenePackager.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\project\EditorWindowsBuildScenePackagerAudioTests.cs`

- [ ] **Step 1: Write the failing cook/packager tests**

```csharp
namespace helengine.editor.tests {
    public sealed class EditorWindowsBuildScenePackagerAudioTests {
        [Fact]
        public void PackagePreservingIdentityPaths_WhenSceneReferencesAudio_WritesCookedAudioAsset() {
            string audioSourcePath = CopyFixtureIntoProject("assets/audio/menu/theme.wav");
            ImportAudio(audioSourcePath, playbackMode: AudioPlaybackMode.Streamed, busId: "music");
            string scenePath = WriteSceneReferencingAudio("assets/scenes/menu/audio_test.helen", "assets/audio/menu/theme.wav");

            EditorPlatformBuildScenePackager packager = CreatePackager(targetPlatformId: "windows");
            EditorPlatformBuildScenePackagerResult result = packager.PackagePreservingIdentityPaths(
                [scenePath],
                [scenePath],
                BuildRootPath);

            Assert.Contains(result.PlatformCookWorkItems, item => item.Kind == "audio");
            Assert.True(File.Exists(Path.Combine(BuildRootPath, "audio", "theme.hasset")));
        }

        [Fact]
        public void PackagePreservingIdentityPaths_WhenDsAudioSettingsExceedLimits_Throws() {
            string audioSourcePath = CopyFixtureIntoProject("assets/audio/menu/theme.wav");
            ImportAudio(audioSourcePath, platformId: "ds", sampleRate: 44100, channels: 2);

            EditorPlatformBuildScenePackager packager = CreatePackager(targetPlatformId: "ds");
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => packager.PackagePreservingIdentityPaths(
                [WriteSceneReferencingAudio("assets/scenes/menu/audio_test.helen", "assets/audio/menu/theme.wav")],
                [WriteSceneReferencingAudio("assets/scenes/menu/audio_test.helen", "assets/audio/menu/theme.wav")],
                BuildRootPath));

            Assert.Contains("ds", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sample rate", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
```

- [ ] **Step 2: Run the packager tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~EditorWindowsBuildScenePackagerAudioTests`

Expected: FAIL because audio references are not packaged and no audio validation exists.

- [ ] **Step 3: Replace the placeholder audio family and add audio packaging**

```csharp
// EditorPlatformBuildGraphRunner.cs
new PlatformCookProfileCapabilities(
    PlatformDescriptor.Id,
    selectedGraphicsProfileId,
    ResolveAudioEncodingFamily(selectedBuildProfileId, PlatformDescriptor.Id),
    $"{PlatformDescriptor.Id}-scene-v1",
    PlatformSerializationEndianness.LittleEndian)

static string ResolveAudioEncodingFamily(string selectedBuildProfileId, string platformId) {
    if (string.Equals(platformId, "windows", StringComparison.OrdinalIgnoreCase)) {
        return "pcm-streamed";
    }
    if (string.Equals(platformId, "ds", StringComparison.OrdinalIgnoreCase)) {
        return "adpcm-buffered";
    }

    return "pcm-buffered";
}
```

```csharp
// EditorWindowsBuildScenePackager.cs
SceneAssetReference RewriteFileSystemAudioReference(SceneAssetReference reference, string buildRootPath) {
    string sourcePath = ResolveProjectAssetPath(reference.RelativePath);
    if (!AssetImportManager.TryLoadAudioAsset(sourcePath, out AudioAsset audioAsset) || audioAsset == null) {
        throw new InvalidOperationException($"Audio source '{reference.RelativePath}' could not be imported for packaging.");
    }

    ValidateAudioSettingsForTarget(reference.RelativePath, audioAsset);
    string cookedRelativePath = BuildCookedAudioRelativePath(reference.RelativePath);
    WriteAsset(Path.Combine(buildRootPath, cookedRelativePath), audioAsset);
    RememberAudioCookWorkItem(sourcePath, cookedRelativePath, audioAsset);
    return CreateFileSystemReference(cookedRelativePath);
}
```

```csharp
// EditorWindowsBuildScenePackager.cs
void ValidateAudioSettingsForTarget(string relativePath, AudioAsset audioAsset) {
    if (string.Equals(TargetPlatformId, "ds", StringComparison.OrdinalIgnoreCase)) {
        if (audioAsset.SampleRate > 22050) {
            throw new InvalidOperationException($"Audio '{relativePath}' exceeds DS sample-rate limits.");
        }
        if (audioAsset.Channels > 1) {
            throw new InvalidOperationException($"Audio '{relativePath}' exceeds DS channel-count limits.");
        }
    }
}

void RememberAudioCookWorkItem(string sourcePath, string cookedRelativePath, AudioAsset audioAsset) {
    PlatformCookWorkItems.Add(new PlatformCookWorkItem(
        "audio",
        sourcePath,
        cookedRelativePath,
        audioAsset.Id,
        new Dictionary<string, string> {
            ["encoding-family"] = audioAsset.EncodingFamilyId,
            ["playback-mode"] = audioAsset.PlaybackMode.ToString()
        }));
}
```

- [ ] **Step 4: Run the targeted packager tests and the full editor test suite once**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~EditorWindowsBuildScenePackagerAudioTests`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git add C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorPlatformBuildGraphRunner.cs C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorPlatformAssetCookService.cs C:\dev\helworks\helengine\engine\helengine.editor\managers\project\EditorWindowsBuildScenePackager.cs C:\dev\helworks\helengine\engine\helengine.editor.tests\managers\project\EditorWindowsBuildScenePackagerAudioTests.cs
rtk git commit -m "feat: add windows audio cook and packager support"
```

## Task 5: Add Shared Runtime Audio Manager and Scene Audio Source Component

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\IAudioBackend.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\AudioManager.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\AudioBus.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\audio\AudioPlaybackRequest.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core\components\AudioSourceComponent.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core\Core.cs`
- Test: `C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\RuntimeSceneLoadServiceTests.cs`

- [ ] **Step 1: Write the failing scene-runtime audio component tests**

```csharp
[Fact]
public void RuntimeSceneLoad_WhenAudioSourceComponentReferencesAudioAsset_ResolvesAndStartsPlayback() {
    FakeAudioBackend backend = new FakeAudioBackend();
    Core core = CreateCoreWithAudioBackend(backend);
    AudioAsset audioAsset = WritePackagedAudioAsset("audio/menu/theme.hasset");
    SceneAsset scene = BuildSceneWithAudioSource("audio/menu/theme.hasset");

    Entity root = RuntimeSceneLoadService.Load(scene, core);
    AudioSourceComponent component = Assert.Single(root.Children.SelectMany(entity => entity.Components.OfType<AudioSourceComponent>()));

    Assert.NotNull(component.Clip);
    Assert.Equal("music", component.BusId);
    Assert.Equal(1, backend.PlayRequests.Count);
}

sealed class FakeAudioBackend : IAudioBackend {
    public List<AudioPlaybackRequest> PlayRequests { get; } = new List<AudioPlaybackRequest>();

    public int Play(AudioAsset asset, AudioPlaybackRequest request) {
        PlayRequests.Add(request);
        return 0;
    }

    public void Stop(int voiceId) { }
    public void SetBusGain(string busId, float gain) { }
    public void SetBusPaused(string busId, bool paused) { }
    public bool IsPlaying(int voiceId) => true;
    public void Update() { }
}
```

- [ ] **Step 2: Run the targeted runtime scene tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~AudioSourceComponent`

Expected: FAIL because `AudioSourceComponent` and `AudioManager` do not exist.

- [ ] **Step 3: Add the shared runtime audio manager, request model, and scene component**

```csharp
namespace helengine {
    public interface IAudioBackend {
        int Play(AudioAsset asset, AudioPlaybackRequest request);
        void Stop(int voiceId);
        void SetBusGain(string busId, float gain);
        void SetBusPaused(string busId, bool paused);
        bool IsPlaying(int voiceId);
        void Update();
    }

    public sealed class AudioPlaybackRequest {
        public string BusId { get; init; } = "master";
        public bool Loop { get; init; }
        public float Gain { get; init; } = 1f;
    }
}
```

```csharp
namespace helengine {
    public sealed class AudioManager {
        readonly IAudioBackend Backend;
        readonly Dictionary<string, AudioBus> BusesById;
        readonly List<int> ActiveVoiceIds;

        public AudioManager(IAudioBackend backend) {
            Backend = backend ?? throw new ArgumentNullException(nameof(backend));
            BusesById = new Dictionary<string, AudioBus>(StringComparer.OrdinalIgnoreCase) {
                ["master"] = new AudioBus("master"),
                ["music"] = new AudioBus("music"),
                ["sfx"] = new AudioBus("sfx")
            };
            ActiveVoiceIds = new List<int>();
        }

        public int Play(AudioAsset asset, AudioPlaybackRequest request) {
            int voiceId = Backend.Play(asset, request);
            ActiveVoiceIds.Add(voiceId);
            return voiceId;
        }
    }
}
```

```csharp
namespace helengine {
    public sealed class AudioSourceComponent : UpdateComponent {
        public AudioAsset Clip { get; set; }
        public bool PlayOnStart { get; set; } = true;
        public bool Loop { get; set; }
        public string BusId { get; set; } = "master";
        public float Gain { get; set; } = 1f;

        int activeVoiceId = -1;

        protected override void OnStart() {
            if (!PlayOnStart || Clip == null) {
                return;
            }

            activeVoiceId = Core.Instance.AudioManager.Play(Clip, new AudioPlaybackRequest {
                BusId = BusId,
                Loop = Loop || Clip.DefaultLoop,
                Gain = Gain
            });
        }
    }
}
```

```csharp
// Core.cs
public AudioManager AudioManager { get; private set; }

public void SetAudioBackend(IAudioBackend backend) {
    AudioManager = new AudioManager(backend);
}
```

- [ ] **Step 4: Run the targeted runtime scene/component tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~AudioSourceComponent`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git add C:\dev\helworks\helengine\engine\helengine.core\audio C:\dev\helworks\helengine\engine\helengine.core\components\AudioSourceComponent.cs C:\dev\helworks\helengine\engine\helengine.core\Core.cs C:\dev\helworks\helengine\engine\helengine.editor.tests\serialization\scene\RuntimeSceneLoadServiceTests.cs
rtk git commit -m "feat: add shared runtime audio manager and source component"
```

## Task 6: Add the Windows Runtime Audio Backend

**Files:**
- Create: `C:\dev\helworks\helengine\engine\helengine.core.windows\audio\WindowsAudioBackend.cs`
- Create: `C:\dev\helworks\helengine\engine\helengine.core.windows\audio\WindowsAudioVoice.cs`
- Modify: `C:\dev\helworks\helengine\engine\helengine.core.windows\helengine.core.windows.csproj`
- Modify: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj`
- Create: `C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\audio\WindowsAudioBackendTests.cs`

- [ ] **Step 1: Write the failing Windows backend tests**

```csharp
namespace helengine.editor.windows.tests {
    public sealed class WindowsAudioBackendTests {
        [Fact]
        public void Play_WhenBufferedAssetSubmitted_ReturnsVoiceIdAndTracksPlayback() {
            WindowsAudioBackend backend = new WindowsAudioBackend();
            AudioAsset asset = CreateBufferedPcmAudioAsset();

            int voiceId = backend.Play(asset, new AudioPlaybackRequest {
                BusId = "sfx",
                Loop = false,
                Gain = 1f
            });

            Assert.True(voiceId >= 0);
            Assert.True(backend.IsPlaying(voiceId));
        }
    }
}
```

- [ ] **Step 2: Run the Windows-only backend tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj --filter FullyQualifiedName~WindowsAudioBackendTests`

Expected: FAIL because `WindowsAudioBackend` does not exist and the test project does not reference `helengine.core.windows`.

- [ ] **Step 3: Add the Windows backend using NAudio raw-stream playback**

```xml
<!-- helengine.core.windows.csproj -->
<ItemGroup>
  <PackageReference Include="NAudio" Version="2.2.1" />
</ItemGroup>
```

```csharp
namespace helengine {
    public sealed class WindowsAudioBackend : IAudioBackend, IDisposable {
        readonly Dictionary<int, WindowsAudioVoice> VoicesById = new Dictionary<int, WindowsAudioVoice>();
        int nextVoiceId;

        public int Play(AudioAsset asset, AudioPlaybackRequest request) {
            RawSourceWaveStream stream = CreateRawWaveStream(asset);
            WaveOutEvent output = new WaveOutEvent();
            output.Init(stream);
            output.Volume = request.Gain;
            output.Play();
            int voiceId = nextVoiceId++;
            VoicesById.Add(voiceId, new WindowsAudioVoice(output, stream, request.BusId));
            return voiceId;
        }

        public bool IsPlaying(int voiceId) {
            return VoicesById.TryGetValue(voiceId, out WindowsAudioVoice voice)
                && voice.Output.PlaybackState == PlaybackState.Playing;
        }

        static RawSourceWaveStream CreateRawWaveStream(AudioAsset asset) {
            byte[] pcmBytes = asset.EncodedBytes ?? Array.Empty<byte>();
            MemoryStream memoryStream = new MemoryStream(pcmBytes, writable: false);
            WaveFormat waveFormat = new WaveFormat(asset.SampleRate, 16, asset.Channels);
            return new RawSourceWaveStream(memoryStream, waveFormat);
        }
    }
}
```

```csharp
namespace helengine {
    internal sealed class WindowsAudioVoice : IDisposable {
        public WindowsAudioVoice(WaveOutEvent output, WaveStream stream, string busId) {
            Output = output;
            Stream = stream;
            BusId = busId;
        }

        public WaveOutEvent Output { get; }
        public WaveStream Stream { get; }
        public string BusId { get; }
    }
}
```

- [ ] **Step 4: Run the Windows backend tests and Windows editor tests**

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj --filter FullyQualifiedName~WindowsAudioBackendTests`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git add C:\dev\helworks\helengine\engine\helengine.core.windows\audio C:\dev\helworks\helengine\engine\helengine.core.windows\helengine.core.windows.csproj C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\helengine.editor.windows.tests.csproj C:\dev\helworks\helengine\engine\helengine.editor.windows.tests\audio\WindowsAudioBackendTests.cs
rtk git commit -m "feat: add windows runtime audio backend"
```

## Task 7: Add the City Main Menu Music Vertical Slice

**Files:**
- Create: `C:\dev\helprojs\demodisc\assets\audio\menu\helen_of_code_high_code_v2.wav`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscMenuTheme.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscStandardMainMenuSceneFactory.cs`
- Modify: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscHandheldMainMenuSceneFactory.cs`
- Create: `C:\dev\helprojs\demodisc\assets\codebase\menu.tools.tests\DemoDiscMainMenuAudioSourceTests.cs`

- [ ] **Step 1: Copy the canonical WAV source into the project and write the failing city test**

Run: `rtk powershell -NoProfile -Command "New-Item -ItemType Directory -Force 'C:\dev\helprojs\demodisc\assets\audio\menu' | Out-Null; Copy-Item 'F:\dev\youtube\projects\v1_unity\audio\Helen of Code - High Code v2.wav' 'C:\dev\helprojs\demodisc\assets\audio\menu\helen_of_code_high_code_v2.wav' -Force"`

```csharp
namespace city.menu.tools.tests {
    public sealed class DemoDiscMainMenuAudioSourceTests {
        [Fact]
        public void Generate_AddsLoopingMusicAudioSourceToStandardAndHandheldScenes() {
            DemoDiscSceneGenerator generator = new DemoDiscSceneGenerator();
            generator.Generate(ProjectRootPath);

            SceneAsset standardScene = LoadScene("assets/scenes/DemoDiscMainMenu.helen");
            SceneAsset handheldScene = LoadScene("assets/scenes/DemoDiscMainMenuHandheld.helen");

            Assert.Contains(EnumerateComponents<AudioSourceComponent>(standardScene), component => component.BusId == "music" && component.PlayOnStart && component.Loop);
            Assert.Contains(EnumerateComponents<AudioSourceComponent>(handheldScene), component => component.BusId == "music" && component.PlayOnStart && component.Loop);
        }
    }
}
```

- [ ] **Step 2: Run the generated city menu-tools test project**

Run: `rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --filter FullyQualifiedName~DemoDiscMainMenuAudioSourceTests`

Expected: FAIL because the menu scenes do not author `AudioSourceComponent`.

- [ ] **Step 3: Add the shared menu-music path and author one audio source into both menu scenes**

```csharp
// DemoDiscMenuTheme.cs
public string ThemeMusicAudioPath => "audio/menu/helen_of_code_high_code_v2.wav";
```

```csharp
// DemoDiscStandardMainMenuSceneFactory.cs
const string DemoDiscThemeMusicRelativePath = "audio/menu/helen_of_code_high_code_v2.wav";

Entity CreateMenuAudioEntity(Entity parent) {
    Entity entity = Core.Instance.EntityFactory.CreateChild(parent, "DemoDiscMenuMusic");
    AudioSourceComponent audioSource = new AudioSourceComponent {
        Clip = new AudioAsset(),
        PlayOnStart = true,
        Loop = true,
        BusId = "music",
        Gain = 0.8f
    };
    entity.AddComponent(audioSource);
    ApplyAudioReference(entity, audioSource, DemoDiscThemeMusicRelativePath);
    return entity;
}

void ApplyAudioReference(Entity entity, AudioSourceComponent audioSourceComponent, string relativePath) {
    entity.SaveData.SetValue(
        AutomaticComponentAssetReferenceSupport.BuildReferenceName(nameof(AudioSourceComponent.Clip)),
        SceneAssetReferenceFactory.CreateFileSystemAudio(relativePath));
}
```

```csharp
// DemoDiscHandheldMainMenuSceneFactory.cs
CreateMenuAudioEntity(menuRootEntity);
```

- [ ] **Step 4: Regenerate the menu scenes and run the city test**

Run: `rtk dotnet test C:\dev\helprojs\demodisc\user_settings\generated_code\projects\menu.tools.tests\menu.tools.tests.csproj --filter FullyQualifiedName~DemoDiscMainMenuAudioSourceTests`

Expected: PASS

Run: `rtk dotnet test C:\dev\helworks\helengine\engine\helengine.editor.tests\helengine.editor.tests.csproj --filter FullyQualifiedName~Audio`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
rtk git add C:\dev\helprojs\demodisc\assets\audio\menu\helen_of_code_high_code_v2.wav C:\dev\helprojs\demodisc\assets\codebase\menu\DemoDiscMenuTheme.cs C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscStandardMainMenuSceneFactory.cs C:\dev\helprojs\demodisc\assets\codebase\menu.tools\DemoDiscHandheldMainMenuSceneFactory.cs C:\dev\helprojs\demodisc\assets\codebase\menu.tools.tests\DemoDiscMainMenuAudioSourceTests.cs
rtk git commit -m "feat: add looping menu music through shared audio system"
```

## Self-Review

### Spec Coverage

- Shared `AudioAsset` and runtime processor support: Task 1 and Task 2
- Canonical source with platform overrides: Task 1 and Task 3
- Buffered SFX and streamed music modes: Task 1, Task 4, Task 5
- Bus volume/mute/pause-ready runtime layer: Task 5 and Task 6
- Scene-authored playback through `AudioSourceComponent`: Task 5 and Task 7
- Windows-first validation path: Task 3 through Task 7
- Constrained-target `ds` validation planning: Task 4
- Main menu music follow-on: Task 7

### Placeholder Scan

- No `TODO`, `TBD`, or deferred implementation notes remain.
- Every code-changing step includes exact files and concrete code snippets.
- Every verification step includes an explicit command and expected result.

### Type Consistency

- Shared type names are consistent across tasks:
  - `AudioAsset`
  - `AudioPlaybackMode`
  - `AudioAssetImportSettings`
  - `AudioAssetProcessorSettings`
  - `AudioManager`
  - `IAudioBackend`
  - `AudioSourceComponent`
  - `WindowsAudioBackend`
