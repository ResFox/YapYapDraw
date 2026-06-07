param(
    [Parameter(Mandatory = $true)][string]$ZipPath,
    [Parameter(Mandatory = $true)][string]$IconPath
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::Open($ZipPath, 'Update')
$existing = $archive.GetEntry('images/icon.png')
if ($existing) { $existing.Delete() }
[IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $IconPath, 'images/icon.png') | Out-Null
$archive.Dispose()
