$dtosPath = "d:\.Projects\Agitek\AgitekBackend\AgitekBackend\DTOs"
$projectPath = "d:\.Projects\Agitek\AgitekBackend\AgitekBackend"

# 1. Rename directories
Get-ChildItem -Path $dtosPath -Directory | Where-Object { $_.Name -like "-*" } | ForEach-Object {
    $newName = $_.Name.Substring(1)
    Rename-Item -Path $_.FullName -NewName $newName
    Write-Host "Renamed folder $($_.Name) to $newName"
}

# 2. Replace namespaces in all .cs files
$files = Get-ChildItem -Path $projectPath -Recurse -Filter "*.cs"
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "AgitekBackend\.DTOs\._") {
        $newContent = $content -replace "AgitekBackend\.DTOs\._", "AgitekBackend.DTOs."
        Set-Content -Path $file.FullName -Value $newContent
        Write-Host "Updated namespace in $($file.FullName)"
    }
}
