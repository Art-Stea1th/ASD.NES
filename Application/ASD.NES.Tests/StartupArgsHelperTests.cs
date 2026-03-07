using System;
using ASD.NES.Core;
using Xunit;

namespace ASD.NES.Tests;

public sealed class StartupArgsHelperTests
{
    [Fact]
    public void GetFirstNesPath_ReturnsNull_WhenArgsNull()
    {
        Assert.Null(StartupArgsHelper.GetFirstNesPath(null));
    }

    [Fact]
    public void GetFirstNesPath_ReturnsNull_WhenOnlyExePath()
    {
        Assert.Null(StartupArgsHelper.GetFirstNesPath(new[] { @"C:\App\ASD.NES.WPF.exe" }));
    }

    [Fact]
    public void GetFirstNesPath_ReturnsPath_WhenSecondArgIsNes()
    {
        var path = @"D:\Games\game.nes";
        var args = new[] { @"C:\App\ASD.NES.WPF.exe", path };
        Assert.Equal(path, StartupArgsHelper.GetFirstNesPath(args));
    }

    [Fact]
    public void GetFirstNesPath_ReturnsPath_WhenThirdArgIsNes()
    {
        var path = @"E:\ROMs\Super.nes";
        var args = new[] { @"C:\App\ASD.NES.WPF.exe", "--some-flag", path };
        Assert.Equal(path, StartupArgsHelper.GetFirstNesPath(args));
    }

    [Fact]
    public void GetFirstNesPath_ReturnsFirstNes_WhenMultipleNes()
    {
        var first = @"C:\a.nes";
        var args = new[] { @"C:\App\ASD.NES.WPF.exe", first, @"C:\b.nes" };
        Assert.Equal(first, StartupArgsHelper.GetFirstNesPath(args));
    }

    [Fact]
    public void GetFirstNesPath_ReturnsNull_WhenNoNesExtension()
    {
        var args = new[] { @"C:\App\ASD.NES.WPF.exe", @"C:\file.txt" };
        Assert.Null(StartupArgsHelper.GetFirstNesPath(args));
    }

    [Fact]
    public void GetFirstNesPath_IgnoresExePath()
    {
        var args = new[] { @"C:\App\my.nes.exe" };
        Assert.Null(StartupArgsHelper.GetFirstNesPath(args));
    }
}
