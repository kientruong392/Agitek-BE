$mappersPath = "d:\.Projects\Agitek\AgitekBackend\AgitekBackend\DTOs\Mappers"
$files = Get-ChildItem -Path $mappersPath -Filter "*Mapper.cs"

foreach ($file in $files) {
    # Extract model name from filename, e.g., "UserMapper.cs" -> "User"
    $modelName = $file.Name.Replace("Mapper.cs", "")
    
    # Skip ProfileMapper because we already aliased it as ProfileModel
    if ($modelName -eq "Profile") {
        continue
    }

    $content = Get-Content $file.FullName -Raw
    
    # Replace CreateMap<Create[Model]DTO, [Model]>();
    $content = $content -replace ", $modelName>", ", AgitekBackend.Models.$modelName>"
    
    # Replace CreateMap<[Model], [Model]DTO>();
    $content = $content -replace "<$modelName, ", "<AgitekBackend.Models.$modelName, "
    
    Set-Content -Path $file.FullName -Value $content
    Write-Host "Fixed namespace collision in $($file.Name)"
}
