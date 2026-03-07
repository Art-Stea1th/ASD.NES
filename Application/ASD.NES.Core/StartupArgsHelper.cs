using System;

namespace ASD.NES.Core {

    /// <summary> Parses command-line args for .nes path (e.g. when app is started by double-clicking a file). </summary>
    public static class StartupArgsHelper {

    /// <summary> Returns the first argument that looks like a .nes file path, or null. Skips args[0] (exe path). </summary>
    public static string GetFirstNesPath(string[] args)
    {
        if (args == null || args.Length < 2) return null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i]?.Trim();
            if (!string.IsNullOrEmpty(a) && a.EndsWith(".nes", StringComparison.OrdinalIgnoreCase))
                return a;
        }
        return null;
    }
    }
}
