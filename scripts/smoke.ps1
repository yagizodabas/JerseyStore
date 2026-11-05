# Requires a running instance. Invalid requests must be rejected before database access.
param([string]$BaseUrl = 'http://localhost:5062')
$ErrorActionPreference = 'Stop'
$valid = @{ customerName = 'Test'; address = 'Test address'; phone = '+905551112233'; items = @(@{ productId = 1; quantity = 1 }) }
$cases = @(
    @{ items = @() },
    @{ items = @($null) },
    @{ quantity = -1 },
    @{ quantity = 0 },
    @{ quantity = 101 },
    @{ productId = 0 },
    @{ jerseyNumber = 100 },
    @{ customerName = ' ' },
    @{ address = '' },
    @{ phone = '' },
    @{ printName = ('x' * 41) }
)
foreach ($case in $cases) {
    $body = $valid | ConvertTo-Json -Depth 10 | ConvertFrom-Json -AsHashtable
    foreach ($key in $case.Keys) {
        if ($key -in @('quantity', 'productId', 'jerseyNumber', 'printName')) { $body.items[0][$key] = $case[$key] }
        else { $body[$key] = $case[$key] }
    }
    $response = Invoke-WebRequest -Uri "$BaseUrl/api/store/order" -Method Post -ContentType 'application/json' -Body ($body | ConvertTo-Json -Depth 10) -SkipHttpErrorCheck -TimeoutSec 15
    if ($response.StatusCode -ne 400) { throw "Expected 400 for $($case.Keys), got $($response.StatusCode)" }
}
$response = Invoke-WebRequest -Uri $BaseUrl -TimeoutSec 15
if ($response.StatusCode -ne 200 -or $response.Content -notmatch 'id="products"') { throw 'Catalog page failed' }
foreach ($path in @('/Products/Index', '/Orders/Cart', '/Home/Privacy', '/css/site.css', '/js/catalog.js', '/js/cart-store.js', '/js/checkout.js')) {
    $page = Invoke-WebRequest -Uri "$BaseUrl$path" -TimeoutSec 15
    if ($page.StatusCode -ne 200) { throw "Missing page or asset: $path" }
}
Write-Output "$($cases.Count) invalid request cases, catalog, page routes and static assets passed."
