HandyCommandy.Args cmdArgs = new HandyCommandy.ArgBuilder()
    .Option("--episode <number>", "Download episode No. <number>")
    .Option("--keep", "Keeps temporary files")
    .Option("--ratio [ratio]", "Either 16:9, or a custom ratio")
    .Run(args);
Console.WriteLine($"Episode: {cmdArgs.Episode}");
Console.WriteLine($"Keep: {cmdArgs.Keep}");
Console.WriteLine($"Ratio: {cmdArgs.Ratio ?? "(default)"}");
