# Lightweight: read only first 16 bytes per .nes file, output mapper, PRG, CHR.
# Usage: .\scan_nes_headers.ps1 -ListFile "C:\...\paths.txt" -Offset 0 -Count 300
param([string[]]$PathList, [string]$ListFile, [int]$Offset = 0, [int]$Count = 500)
if ($ListFile) {
    $all = [System.IO.File]::ReadAllLines($ListFile)
    $end = [Math]::Min($Offset + $Count, $all.Length)
    $PathList = @(); for ($i = $Offset; $i -lt $end; $i++) { $PathList += $all[$i] }
}
$out = @()
foreach ($p in $PathList) {
    $p = $p.Trim()
    if (-not $p -or -not (Test-Path -LiteralPath $p -PathType Leaf)) { continue }
    try {
        $fs = [System.IO.File]::OpenRead($p)
        $h = New-Object byte[] 16
        [void]$fs.Read($h, 0, 16)
        $fs.Close()
        if ($h[0] -ne 0x4E -or $h[1] -ne 0x45 -or $h[2] -ne 0x53 -or $h[3] -ne 0x1A) { continue }
        $mapper = (($h[7] -band 0xF0) -bor (($h[6] -shr 4) -band 0x0F))
        $prg = $h[4]; $chr = $h[5]
        $out += [PSCustomObject]@{ Mapper = $mapper; PRG = $prg; CHR = $chr; Path = $p }
    } catch { }
}
$out
