param([string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$taskAppletRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskPublishDirectory = if ($OutputDirectory) { [System.IO.Path]::GetFullPath($OutputDirectory) } else { Join-Path $taskAppletRoot 'publish\Applet.Watch.at365' }
dotnet publish (Join-Path $taskAppletRoot 'Applet.Watch\Applet.Watch.csproj') -c Release -r win-x64 --self-contained true -o $taskPublishDirectory
if ($LASTEXITCODE -ne 0) { throw "Clock Applet publish failed ($LASTEXITCODE)" }
Copy-Item -LiteralPath (Join-Path $taskAppletRoot 'extension.json') -Destination (Join-Path $taskPublishDirectory 'extension.json') -Force
foreach ($taskSymbols in @('AppDock.Runtime.pdb', 'AppDock.SDK.pdb')) {
    $taskSymbolFile = Join-Path $taskPublishDirectory $taskSymbols
    if (Test-Path -LiteralPath $taskSymbolFile -PathType Leaf) { Remove-Item -LiteralPath $taskSymbolFile }
}
Write-Output "Applet output: $taskPublishDirectory"
