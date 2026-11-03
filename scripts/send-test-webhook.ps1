param(
    [string]$EventId = "evt_$([guid]::NewGuid().ToString('N').Substring(0, 16))",
    [string]$Url = "http://localhost:5183/webhooks/stripe",
    [string]$Secret
)

if (-not $Secret) {
    $line = dotnet user-secrets list --project "$PSScriptRoot\..\src\WebhookReceiver" | Where-Object { $_ -like "Stripe:WebhookSecret = *" }
    if (-not $line) { throw "Stripe:WebhookSecret not found in user secrets. Pass -Secret or set it with dotnet user-secrets." }
    $Secret = ($line -split " = ", 2)[1]
}

$body = @{
    id       = $EventId
    object   = "event"
    type     = "payment_intent.succeeded"
    created  = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    livemode = $false
    data     = @{ object = @{ id = "pi_test"; amount = 2000; currency = "usd" } }
} | ConvertTo-Json -Compress -Depth 5

$t = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$hmac = [System.Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Secret))
$sig = -join ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$t.$body")) | ForEach-Object { $_.ToString("x2") })

Write-Host "POST $Url  ($EventId)"
Invoke-RestMethod -Method Post -Uri $Url -ContentType "application/json" `
    -Headers @{ "Stripe-Signature" = "t=$t,v1=$sig" } -Body ([Text.Encoding]::UTF8.GetBytes($body))
