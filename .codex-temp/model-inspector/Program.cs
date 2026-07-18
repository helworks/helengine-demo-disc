using helengine;
using helengine.editor;

if (args.Length != 1) {
    Console.Error.WriteLine("Usage: ModelInspector <model-asset-path>");
    return 1;
}

using FileStream stream = File.OpenRead(args[0]);
ModelAsset model = (ModelAsset)helengine.editor.AssetSerializer.Deserialize(stream);

Console.WriteLine($"Positions={model.Positions?.Length ?? 0}");
Console.WriteLine($"Normals={model.Normals?.Length ?? 0}");
Console.WriteLine($"TexCoords={model.TexCoords?.Length ?? 0}");
Console.WriteLine($"Indices16={model.Indices16?.Length ?? 0}");
Console.WriteLine($"BoundsMin={model.BoundsMin}");
Console.WriteLine($"BoundsMax={model.BoundsMax}");

HashSet<string> uniqueNormals = new HashSet<string>(StringComparer.Ordinal);
if (model.Normals != null) {
    for (int index = 0; index < model.Normals.Length; index++) {
        float3 normal = model.Normals[index];
        uniqueNormals.Add($"{normal.X:0.###},{normal.Y:0.###},{normal.Z:0.###}");
    }
}

Console.WriteLine($"UniqueNormals={uniqueNormals.Count}");
foreach (string normal in uniqueNormals.Take(16)) {
    Console.WriteLine(normal);
}

return 0;
