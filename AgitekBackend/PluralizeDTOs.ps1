$dtosPath = "d:\.Projects\Agitek\AgitekBackend\AgitekBackend\DTOs"
$projectPath = "d:\.Projects\Agitek\AgitekBackend\AgitekBackend"

$models = @{
    "User" = "Users"
    "Product" = "Products"
    "Order" = "Orders"
    "Brand" = "Brands"
    "Category" = "Categories"
    "Review" = "Reviews"
    "Voucher" = "Vouchers"
    "Warranty" = "Warranties"
    "Address" = "Addresses"
    "Profile" = "Profiles"
    "OrderDetail" = "OrderDetails"
    "OrderTrackingLog" = "OrderTrackingLogs"
    "ProductImage" = "ProductImages"
    "ProductSpecification" = "ProductSpecifications"
    "Media" = "Medias"
    "AuditLog" = "AuditLogs"
    "LocationCoordinates" = "LocationCoordinates"
}

# 1. Rename directories
foreach ($key in $models.Keys) {
    $plural = $models[$key]
    $oldPath = Join-Path $dtosPath $key
    $newPath = Join-Path $dtosPath $plural
    if (Test-Path $oldPath) {
        if ($oldPath -ne $newPath) {
            Rename-Item -Path $oldPath -NewName $plural
            Write-Host "Renamed folder $key to $plural"
        }
    }
}

# 2. Replace namespaces in all .cs files and fix Mapper CreateMap
$files = Get-ChildItem -Path $projectPath -Recurse -Filter "*.cs"
foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false

    foreach ($key in $models.Keys) {
        $plural = $models[$key]
        
        # Replace namespace references
        $pattern = "AgitekBackend\.DTOs\.$key(?!\w)"
        if ($content -match $pattern) {
            $content = $content -replace $pattern, "AgitekBackend.DTOs.$plural"
            $modified = $true
        }
        
        # Replace AgitekBackend.Models.[Model] with [Model] in Mappers
        $patternModel = "AgitekBackend\.Models\.$key(?!\w)"
        if ($file.Name -like "*Mapper.cs" -and $content -match $patternModel) {
            $content = $content -replace $patternModel, $key
            $modified = $true
        }
    }
    
    # Specific fix for ProfileMapper to avoid ambiguity since we removed ProfileModel
    if ($file.Name -eq "ProfileMapper.cs") {
        if ($content -match "using ProfileModel = AgitekBackend.Models.Profile;") {
            $content = $content -replace "using ProfileModel = AgitekBackend.Models.Profile;", "using Profile = AgitekBackend.Models.Profile;"
            $modified = $true
        }
        if ($content -match "ProfileModel") {
            $content = $content -replace "ProfileModel", "Profile"
            $modified = $true
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated $($file.FullName)"
    }
}
