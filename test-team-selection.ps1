# Test script for Team Selection API
$uri = "https://localhost:60339/api/team/process"
$headers = @{
    "Content-Type" = "application/json"
}

$body = @"
[
    {
        "position": "midfielder",
        "mainSkill": "speed",
        "numberOfPlayers": 1
    },
    {
        "position": "defender",
        "mainSkill": "strength",
        "numberOfPlayers": 2
    }
]
"@

Write-Host "Sending request to $uri..."
try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body -ErrorAction Stop
    Write-Host "Response received:"
    $response | ConvertTo-Json -Depth 10
}
catch {
    Write-Host "Error: $_"
    if ($_.ErrorDetails) {
        Write-Host "Details: $($_.ErrorDetails.Message)"
    }
    elseif ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $reader.BaseStream.Position = 0
        $reader.DiscardBufferedData()
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response body: $responseBody"
    }
}
