using city.game.tools;

if (args.Length != 1) {
    Console.Error.WriteLine("Usage: GoldenCoinRegenerator <project-root>");
    return 1;
}

SplitPlayGoldenCoinAssetGenerator generator = new SplitPlayGoldenCoinAssetGenerator();
generator.Generate(args[0]);
Console.WriteLine("GoldenCoin regenerated.");
return 0;
