# Lightweight: read only first 16 bytes per .nes file and summarize mapper/flags/region.
#
# Examples:
# - Scan a directory recursively:
#   .\scan_nes_headers.ps1 -RootDirs "C:\path\to\roms" -Recurse
# - Scan multiple roots:
#   .\scan_nes_headers.ps1 -RootDirs "e:\Games\NES\EmulatorsPack\NES\roms","e:\Games\NES\NES" -Recurse
# - Scan explicit file list:
#   .\scan_nes_headers.ps1 -ListFile "C:\...\paths.txt" -Offset 0 -Count 300
#
param(
    [string[]]$PathList,
    [string]$ListFile,
    [string[]]$RootDirs,
    [switch]$Recurse,
    [int]$Offset = 0,
    [int]$Count = 500,
    [switch]$SummaryOnly
)

function Get-NesHeaderInfo([string]$p) {
    try {
        $fs = [System.IO.File]::OpenRead($p)
        $h = New-Object byte[] 16
        [void]$fs.Read($h, 0, 16)
        $fs.Close()
        if ($h[0] -ne 0x4E -or $h[1] -ne 0x45 -or $h[2] -ne 0x53 -or $h[3] -ne 0x1A) { return $null }

        $byte6 = $h[6]
        $byte7 = $h[7]
        $mapper = (($byte7 -band 0xF0) -bor (($byte6 -shr 4) -band 0x0F))
        $prg = [int]$h[4]
        $chr = [int]$h[5]

        $fourScreen = (($byte6 -band 0x08) -ne 0)
        $mirroring = if ($fourScreen) { "FourScreen" } elseif (($byte6 -band 0x01) -ne 0) { "Vertical" } else { "Horizontal" }
        $trainer = (($byte6 -band 0x04) -ne 0)
        $battery = (($byte6 -band 0x02) -ne 0)

        $flags9 = $h[9]
        $flags10 = $h[10]
        $palFrom9 = (($flags9 -band 0x01) -ne 0)
        $palFrom10 = (($flags10 -band 0x03) -eq 2)
        $region = if ($palFrom9 -or $palFrom10) { "PAL" } else { "NTSC" }

        return [PSCustomObject]@{
            Mapper     = $mapper
            PRG        = $prg
            CHR        = $chr
            Mirroring  = $mirroring
            Trainer    = $trainer
            Battery    = $battery
            Flags6     = ('0x{0:X2}' -f $byte6)
            Flags7     = ('0x{0:X2}' -f $byte7)
            Flags9     = ('0x{0:X2}' -f $flags9)
            Flags10    = ('0x{0:X2}' -f $flags10)
            Region     = $region
            Path       = $p
        }
    } catch {
        return $null
    }
}

if ($ListFile) {
    $all = [System.IO.File]::ReadAllLines($ListFile)
    $end = [Math]::Min($Offset + $Count, $all.Length)
    $PathList = @(); for ($i = $Offset; $i -lt $end; $i++) { $PathList += $all[$i] }
}

if (-not $PathList -and $RootDirs) {
    $paths = @()
    foreach ($rd in $RootDirs) {
        if (-not $rd) { continue }
        if (-not (Test-Path -LiteralPath $rd)) { continue }
        $gci = if ($Recurse) {
            Get-ChildItem -LiteralPath $rd -Recurse -File -Filter *.nes
        } else {
            Get-ChildItem -LiteralPath $rd -File -Filter *.nes
        }
        $paths += $gci | Select-Object -ExpandProperty FullName
    }
    $PathList = $paths
}

$out = @()
foreach ($p in $PathList) {
    $p = $p.Trim()
    if (-not $p -or -not (Test-Path -LiteralPath $p -PathType Leaf)) { continue }
    $info = Get-NesHeaderInfo $p
    if ($info -ne $null) { $out += $info }
}

if ($SummaryOnly) {
    $out |
        Group-Object Mapper,Region,Mirroring |
        Sort-Object Count -Descending |
        Select-Object Count, Name
    return
}

$out
