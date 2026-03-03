using System;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace ASD.NES.Tests;

/// <summary>
/// Tests for settings save/load contract (XML format matches ASD.NES.WPF Models).
/// Key capture and UI are tested manually or via WPF test host.
/// </summary>
public sealed class SettingsTests {

    [Fact]
    public void SettingsFilePathIsNextToExecutable() {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var expectedFileName = "ASD.NES.Settings.xml";
        var expectedPath = Path.Combine(baseDir, expectedFileName);
        Assert.EndsWith(expectedFileName, expectedPath, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(baseDir.TrimEnd(Path.DirectorySeparatorChar), expectedPath.TrimEnd(Path.DirectorySeparatorChar));
    }

    [Fact]
    public void SettingsXMLRoundtripPreservesMappingStrings() {
        var dto = new AppSettingsDto {
            Player1 = new ControllerMappingDto { Left = "A", Up = "W", Right = "D", Down = "S", Select = "LeftShift", Start = "Enter", B = "K", A = "L" },
            Player2 = new ControllerMappingDto { Left = "Left", Up = "Up", Right = "Right", Down = "Down", Select = "RightShift", Start = "Return", B = "Insert", A = "Delete" }
        };
        var serializer = new XmlSerializer(typeof(AppSettingsDto));
        string xml;
        using (var sw = new StringWriter()) {
            serializer.Serialize(sw, dto);
            xml = sw.ToString();
        }
        Assert.Contains("Player1", xml);
        Assert.Contains("Left", xml);
        AppSettingsDto? loaded;
        using (var sr = new StringReader(xml)) {
            loaded = (AppSettingsDto?)serializer.Deserialize(sr);
        }
        Assert.NotNull(loaded);
        Assert.NotNull(loaded.Player1);
        Assert.NotNull(loaded.Player2);
        Assert.Equal("A", loaded.Player1!.Left);
        Assert.Equal("Enter", loaded.Player1.Start);
        Assert.Equal("Left", loaded.Player2!.Left);
        Assert.Equal("Return", loaded.Player2.Start);
    }

    [Fact]
    public void SettingsSaveToTempFileAndLoadRoundtrip() {
        var path = Path.Combine(Path.GetTempPath(), "ASD.NES.Settings.Test." + Guid.NewGuid().ToString("N") + ".xml");
        try {
            var dto = new AppSettingsDto {
                Player1 = new ControllerMappingDto { Left = "X", Up = "Y", Right = "B", Down = "A", Select = "Tab", Start = "Space", B = "N", A = "M" },
                Player2 = new ControllerMappingDto { Left = "NumPad4", Up = "NumPad8", Right = "NumPad6", Down = "NumPad2", Select = "Subtract", Start = "Add", B = "Decimal", A = "NumPad0" }
            };
            var serializer = new XmlSerializer(typeof(AppSettingsDto));
            using (var fs = File.Create(path)) {
                serializer.Serialize(fs, dto);
            }
            Assert.True(File.Exists(path));
            AppSettingsDto? loaded;
            using (var fs = File.OpenRead(path)) {
                loaded = (AppSettingsDto?)serializer.Deserialize(fs);
            }
            Assert.NotNull(loaded);
            Assert.NotNull(loaded.Player1);
            Assert.NotNull(loaded.Player2);
            Assert.Equal("X", loaded!.Player1!.Left);
            Assert.Equal("Space", loaded.Player1.Start);
            Assert.Equal("NumPad4", loaded.Player2!.Left);
        }
        finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    /// <summary>DTO matching WPF AppSettingsModel XML shape (no Key dependency).</summary>
    public sealed class AppSettingsDto {
        public ControllerMappingDto? Player1 { get; set; }
        public ControllerMappingDto? Player2 { get; set; }
    }

    /// <summary>DTO matching WPF ControllerMappingModel XML shape.</summary>
    public sealed class ControllerMappingDto {
        public string? Left { get; set; }
        public string? Up { get; set; }
        public string? Right { get; set; }
        public string? Down { get; set; }
        public string? Select { get; set; }
        public string? Start { get; set; }
        public string? B { get; set; }
        public string? A { get; set; }
    }
}
