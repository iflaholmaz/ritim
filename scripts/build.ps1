param([string]$RuntimeDirectory = '')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$releaseVersion = '44.7.0'
$buildRoot = Join-Path $projectRoot 'build'
$appRoot = Join-Path $buildRoot 'Ritim'
$outputRoot = Join-Path $projectRoot 'dist'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Windows .NET Framework C# compiler is required.' }
New-Item -ItemType Directory -Force $buildRoot,$appRoot,$outputRoot | Out-Null
if ($RuntimeDirectory) {
    Copy-Item (Join-Path $RuntimeDirectory '*') $appRoot -Recurse -Force
} else {
    $name = "electron-v$releaseVersion-win32-x64.zip"
    $url = "https://github.com/electron/electron/releases/download/v$releaseVersion"
    $zipPath = Join-Path $buildRoot $name
    Invoke-WebRequest "$url/$name" -OutFile $zipPath
    $checksums = (Invoke-WebRequest "$url/SHASUMS256.txt").Content
    $expected = (($checksums -split "`n" | Where-Object { $_.Trim().EndsWith($name) }) -split '\s+')[0]
    if (!$expected -or (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLower() -ne $expected.ToLower()) { throw 'Electron checksum verification failed.' }
    Expand-Archive -LiteralPath $zipPath -DestinationPath $appRoot -Force
    Rename-Item -LiteralPath (Join-Path $appRoot 'electron.exe') -NewName 'Ritim.exe'
}
$resources = Join-Path $appRoot 'resources/app'
New-Item -ItemType Directory -Force $resources | Out-Null
Copy-Item (Join-Path $projectRoot 'src/*') $resources -Recurse -Force
Copy-Item (Join-Path $projectRoot 'assets/Ritim.ico') $appRoot -Force
Copy-Item (Join-Path $projectRoot 'assets/Ritim-v2.ico') $appRoot -Force
$iconPatch = Join-Path $buildRoot 'IconPatch.exe'
& $compiler /nologo /target:exe "/out:$iconPatch" (Join-Path $PSScriptRoot 'IconPatch.cs')
if ($LASTEXITCODE -ne 0) { throw 'Icon patch compiler failed.' }
& $iconPatch (Join-Path $appRoot 'Ritim.exe') (Join-Path $appRoot 'Ritim.ico')
if ($LASTEXITCODE -ne 0) { throw 'Executable icon update failed.' }
$commonArgs = @('/nologo','/target:winexe',"/win32icon:$(Join-Path $appRoot 'Ritim.ico')",'/reference:System.IO.Compression.dll','/reference:System.IO.Compression.FileSystem.dll','/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll')
$uninstaller = Join-Path $buildRoot 'Ritim-Kaldir.exe'
& $compiler @commonArgs "/out:$uninstaller" (Join-Path $PSScriptRoot 'Installer.cs')
if ($LASTEXITCODE -ne 0) { throw 'Uninstaller compiler failed.' }
$payload = Join-Path $buildRoot 'payload.zip'
Compress-Archive -Path $appRoot -DestinationPath $payload -Force
$installer = Join-Path $outputRoot 'Ritim-Kurulum.exe'
& $compiler @commonArgs "/out:$installer" "/resource:$payload,payload.zip" "/resource:$uninstaller,uninstall.exe" "/resource:$(Join-Path $appRoot 'Ritim.ico'),app.ico" (Join-Path $PSScriptRoot 'Installer.cs')
if ($LASTEXITCODE -ne 0) { throw 'Installer compiler failed.' }
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLower()
Set-Content (Join-Path $outputRoot 'SHA256SUMS.txt') "$hash  Ritim-Kurulum.exe" -Encoding utf8
Write-Output "Installer: $installer"

