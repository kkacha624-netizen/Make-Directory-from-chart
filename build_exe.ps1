$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourcePath = Join-Path $root "src\DirectoryTreeBuilder.cs"
$exePath = Join-Path $root "DirectoryTreeBuilder.exe"

if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "src\DirectoryTreeBuilder.cs was not found."
}

$cscCandidates = @(
    (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
    (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe")
)

$csc = $cscCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
if (-not $csc) {
    throw ".NET Framework C# compiler was not found."
}

& $csc `
    /nologo `
    /target:exe `
    /codepage:65001 `
    /out:$exePath `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    $sourcePath

if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed."
}

if ((-not (Test-Path -LiteralPath $exePath -PathType Leaf)) -or ((Get-Item -LiteralPath $exePath).Length -eq 0)) {
    throw "DirectoryTreeBuilder.exe was not created."
}

Write-Host "Created: $exePath"
