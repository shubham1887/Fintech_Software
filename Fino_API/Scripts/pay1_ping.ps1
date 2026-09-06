# =============================================================================
#  Pay1 credential ping - the smallest possible signed call (balance, op 2)
#  ---------------------------------------------------------------------------
#  Prints the exact trans_id, the hash it produced and the raw reply, so an
#  authentication failure can be pinned on the right thing.
#
#  Pay1 signs every call as:   hash_code = sha1(partner_id + trans_id + api_key)
#  A wrong api_key, a wrong partner_id, or a call from an IP Pay1 has not
#  whitelisted all come back the same way:
#      {"status":"failure","errCode":"E005","description":"Authentication Error..."}
#
#  Usage:
#     .\pay1_ping.ps1 -PartnerId P0232050 -ApiKey <api key>
#     .\pay1_ping.ps1 -PartnerId 31       -ApiKey <api key>     # if unsure which id
#
#  Run it ON THE SERVER whose public IP is registered with Pay1 - from anywhere
#  else the same E005 comes back even with a perfect key.
# =============================================================================

param(
    [Parameter(Mandatory = $true)][string] $PartnerId,
    [Parameter(Mandatory = $true)][string] $ApiKey,
    [string] $BaseUrl = 'https://recharges.pay1.in',
    [int]    $Operation = 2
)

function Get-Pay1Hash([string] $partnerId, [string] $transId, [string] $apiKey) {
    $sha1 = [System.Security.Cryptography.SHA1]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes("$partnerId$transId$apiKey")
    ($sha1.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
}

$transId = 'PING' + (Get-Date -Format 'yyMMddHHmmss') + (Get-Random -Minimum 100 -Maximum 999)
$hash = Get-Pay1Hash $PartnerId $transId $ApiKey
$url = "$($BaseUrl.TrimEnd('/'))/apis/receiveApi?partner_id=$PartnerId&operation_type=$Operation&trans_id=$transId&hash_code=$hash"

Write-Host ''
Write-Host 'What is being sent' -ForegroundColor Cyan
Write-Host "  partner_id      : $PartnerId"
Write-Host "  operation_type  : $Operation   (2 = balance, read-only)"
Write-Host "  trans_id        : $transId"
Write-Host "  api key length  : $($ApiKey.Length) chars   (never sent - only hashed)"
Write-Host "  hash input      : $PartnerId + $transId + <api key>"
Write-Host "  hash_code       : $hash"
Write-Host "  url             : $url"

# Our public IP, so it can be compared against what Pay1 has whitelisted.
try {
    $myIp = (Invoke-RestMethod -Uri 'https://api.ipify.org?format=json' -TimeoutSec 15).ip
    Write-Host "  calling from IP : $myIp" -ForegroundColor Yellow
}
catch {
    Write-Host '  calling from IP : (could not determine)' -ForegroundColor DarkYellow
}

Write-Host ''
Write-Host 'Pay1 replied' -ForegroundColor Cyan
try {
    $raw = Invoke-WebRequest -Uri $url -Method Get -TimeoutSec 60 -UseBasicParsing
    $body = $raw.Content
    Write-Host "  $body"

    $json = $null
    try { $json = $body | ConvertFrom-Json } catch { }

    Write-Host ''
    if ($json -and $json.status -eq 'success') {
        Write-Host 'OK - credentials accepted.' -ForegroundColor Green
        if ($json.balance) { Write-Host "Pay1 wallet balance: $($json.balance)" -ForegroundColor Green }
        Write-Host ''
        Write-Host 'Next: put this same API key in the panel -' -ForegroundColor Cyan
        Write-Host '  API Providers -> Pay1 BBPS -> Edit -> Token'
    }
    elseif ($json -and $json.errCode -eq 'E005') {
        Write-Host 'E005 = Pay1 rejected the signature. Three things produce it:' -ForegroundColor Red
        Write-Host '  1. Wrong API key. The API key is NOT the web-panel password - it is'
        Write-Host '     issued separately. Ask Pay1 support for the API key for this account.'
        Write-Host "  2. Wrong partner_id. Try the other identifier you were given"
        Write-Host "     (account id like P0232050, vs the panel partner id like 31)."
        Write-Host "  3. This machine's IP is not on Pay1's whitelist. Give them the IP"
        Write-Host '     printed above (and the production server IP) and retry.'
    }
    else {
        Write-Host 'Call went through but was not a success reply - read the description above.' -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  request failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ''
    Write-Host 'Nothing came back from Pay1 at all - check network/DNS to recharges.pay1.in.' -ForegroundColor Yellow
}
Write-Host ''
