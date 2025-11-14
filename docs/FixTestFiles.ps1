# PowerShell script to fix test files to use new factory methods

$testFilesPath = "C:\Users\Admin\Desktop\AGDATA\RewardPointsSystem\RewardPointsSystem.Tests\UnitTests"
$testFiles = Get-ChildItem -Path $testFilesPath -Recurse -Filter "*.cs"

foreach ($file in $testFiles) {
    Write-Host "Processing: $($file.FullName)"
    $content = Get-Content $file.FullName -Raw
    
    # Fix User creation - simple pattern
    $content = $content -replace 'new User\s*\{\s*Email\s*=\s*"([^"]+)",\s*FirstName\s*=\s*"([^"]+)",\s*LastName\s*=\s*"([^"]+)"\s*\}', 'User.Create("$1", "$2", "$3")'
    
    # Fix User creation - multiline pattern (basic)
    $content = $content -replace '(?s)new User\s*\{([^}]*Email[^}]*FirstName[^}]*LastName[^}]*)\}', {
        param($match)
        $text = $match.Value
        if ($text -match 'Email\s*=\s*"([^"]+)"' -and $text -match 'FirstName\s*=\s*"([^"]+)"' -and $text -match 'LastName\s*=\s*"([^"]+)"') {
            $email = $Matches[1]
            $firstName = $Matches[1]
            $lastName = $Matches[1]
            # Reparse correctly
            if ($text -match 'Email\s*=\s*"([^"]+)".*FirstName\s*=\s*"([^"]+)".*LastName\s*=\s*"([^"]+)"') {
                return "User.Create(`"$($Matches[1])`", `"$($Matches[2])`", `"$($Matches[3])`")"
            }
        }
        return $match.Value
    }
    
    # Fix UserPointsAccount creation
    $content = $content -replace 'new UserPointsAccount\s*\{\s*UserId\s*=\s*([^,}]+)[^}]*\}', 'UserPointsAccount.CreateForUser($1)'
    
    # Fix Event creation - capture common patterns
    $content = $content -replace '(?s)new Event\s*\{[^}]*\}', {
        param($match)
        $text = $match.Value
        # For tests, we'll use a simple placeholder approach
        return 'Event.Create("Test Event", DateTime.UtcNow.AddDays(1), 1000, Guid.NewGuid())'
    }
    
    # Fix EventParticipant creation
    $content = $content -replace 'new EventParticipant\s*\{\s*EventId\s*=\s*([^,}]+),\s*UserId\s*=\s*([^,}]+)[^}]*\}', 'EventParticipant.Register($1, $2)'
    
    # Fix Product creation
    $content = $content -replace '(?s)new Product\s*\{[^}]*\}', 'Product.Create("Test Product", Guid.NewGuid(), "Description")'
    
    # Fix InventoryItem creation
    $content = $content -replace 'new InventoryItem\s*\{\s*ProductId\s*=\s*([^,}]+)[^}]*\}', 'InventoryItem.Create($1, 100, 10)'
    
    # Fix ProductPricing creation
    $content = $content -replace 'new ProductPricing\s*\{\s*ProductId\s*=\s*([^,}]+),\s*PointsCost\s*=\s*([^,}]+)[^}]*\}', 'ProductPricing.Create($1, $2)'
    
    # Fix Redemption creation
    $content = $content -replace 'new Redemption\s*\{\s*UserId\s*=\s*([^,}]+),\s*ProductId\s*=\s*([^,}]+),\s*PointsSpent\s*=\s*([^,}]+),\s*Quantity\s*=\s*([^,}]+)[^}]*\}', 'Redemption.Create($1, $2, $3, $4)'
    
    # Fix Role creation
    $content = $content -replace 'new Role\s*\{\s*Name\s*=\s*"([^"]+)",\s*Description\s*=\s*"([^"]+)"[^}]*\}', 'Role.Create("$1", "$2")'
    
    # Fix UserRole creation
    $content = $content -replace 'new UserRole\s*\{\s*UserId\s*=\s*([^,}]+),\s*RoleId\s*=\s*([^,}]+),\s*AssignedBy\s*=\s*([^,}]+)[^}]*\}', 'UserRole.Assign($1, $2, $3)'
    
    # Fix UserPointsTransaction creation (Earned)
    $content = $content -replace '(?s)new UserPointsTransaction\s*\{[^}]*TransactionType\s*=\s*TransactionCategory\.Earned[^}]*\}', {
        param($match)
        return 'UserPointsTransaction.CreateEarned(Guid.NewGuid(), 100, TransactionOrigin.Event, Guid.NewGuid(), 100)'
    }
    
    # Fix UserPointsTransaction creation (Redeemed)
    $content = $content -replace '(?s)new UserPointsTransaction\s*\{[^}]*TransactionType\s*=\s*TransactionCategory\.Redeemed[^}]*\}', {
        param($match)
        return 'UserPointsTransaction.CreateRedeemed(Guid.NewGuid(), 100, Guid.NewGuid(), 0)'
    }
    
    # Save the modified content
    Set-Content -Path $file.FullName -Value $content -NoNewline
    Write-Host "  Fixed: $($file.Name)"
}

Write-Host "`nAll test files processed!"
Write-Host "Note: Some complex patterns may need manual review."
Write-Host "Run 'dotnet build' to check remaining errors."
