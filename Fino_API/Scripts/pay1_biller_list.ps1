# =============================================================================
#  Pay1 biller catalogue -> ready-to-paste SQL mapping rows
#  ---------------------------------------------------------------------------
#  Calls Pay1's Biller Info API (operation_type 7) and prints every biller with
#  its product_id, plus the VALUES block for @Map in pay1_bbps_seed.sql.
#
#  This is an operator tool, not part of the application - it exists so nobody
#  has to hand-copy a few hundred product ids out of a browser.
#
#  Usage:
#     .\pay1_biller_list.ps1 -PartnerId P0232050 -ApiKey <api key>
#     .\pay1_biller_list.ps1 -PartnerId P0232050 -ApiKey <api key> -Filter electricity
#
#  Note: Pay1 only answers from whitelisted IPs, so run it on the server whose
#  IP you registered with them.
# =============================================================================

param(
    [Parameter(Mandatory = $true)][string] $PartnerId,
    [Parameter(Mandatory = $true)][string] $ApiKey,
    [string] $BaseUrl = 'https://recharges.pay1.in',
    [string] $Filter = ''
)

function Get-Pay1Hash([string] $partnerId, [string] $transId, [string] $apiKey) {
    # hash_code = sha1(partner_id + trans_id + api_key), lower-case hex
    $sha1 = [System.Security.Cryptography.SHA1]::Create()
    $bytes = [System.Text.Encoding]::UTF8.GetBytes("$partnerId$transId$apiKey")
    ($sha1.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join ''
}

$transId = 'BI' + (Get-Date -Format 'yyMMddHHmmss') + (Get-Random -Minimum 100 -Maximum 999)
$hash = Get-Pay1Hash $PartnerId $transId $ApiKey
$url = "$($BaseUrl.TrimEnd('/'))/apis/receiveApi?partner_id=$PartnerId&operation_type=7&trans_id=$transId&hash_code=$hash"

Write-Host "Fetching biller catalogue..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri $url -Method Get -TimeoutSec 90
}
catch {
    Write-Error "Pay1 call failed: $($_.Exception.Message)"
    exit 1
}

if ($response.status -ne 'success') {
    Write-Error "Pay1 returned: $($response | ConvertTo-Json -Depth 4 -Compress)"
    exit 1
}

$billers = $response.description.data
if (-not $billers) { $billers = $response.data }
if (-not $billers) { Write-Error 'No biller list in the response.'; exit 1 }

if ($Filter) { $billers = $billers | Where-Object { $_.Name -like "*$Filter*" } }

Write-Host ''
Write-Host ("{0,-8} {1,-55} {2,-10} {3}" -f 'ProdID', 'Biller', 'BillFetch', 'Inputs') -ForegroundColor Yellow
Write-Host ('-' * 110)

foreach ($b in $billers | Sort-Object Name) {
    # Every input the biller asks for, in order. The first is the consumer number;
    # the next two become Pay1's "param" and "param1" (our Param1 / Param2).
    $inputs = @()
    if ($b.fields) {
        foreach ($name in ($b.fields.PSObject.Properties.Name | Sort-Object { [int]$_ })) {
            $f = $b.fields.$name
            if ($f.param -eq 'amount') { continue }
            $inputs += "$($f.label) [$($f.param)]"
        }
    }
    Write-Host ("{0,-8} {1,-55} {2,-10} {3}" -f $b.product_id, $b.Name, $b.bill_fetch, ($inputs -join ' | '))
}

Write-Host ''
Write-Host "Billers listed: $($billers.Count)" -ForegroundColor Cyan
Write-Host ''
Write-Host '--- paste into @Map in pay1_bbps_seed.sql, after setting each Opcode ---' -ForegroundColor Yellow
Write-Host 'INSERT INTO @Map (Opcode, ProductID) VALUES'

$rows = foreach ($b in $billers | Sort-Object Name) {
    "    ('<our Opcode>', '$($b.product_id)')   -- $($b.Name)"
}
Write-Host (($rows -join ",`n"))
Write-Host ';'
