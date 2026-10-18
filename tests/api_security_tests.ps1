# ==============================================================================
# RemindPe - API Security & Integration Test Suite
# Tests API contracts, Security Headers, Correlation ID, Validation & Edge Cases
# ==============================================================================

param(
    [string]$BaseUrl = "http://localhost:5033",
    [string]$ApiKey = ""
)

$ErrorActionPreference = "Stop"
$passed = 0
$failed = 0

function Assert-Test([string]$TestName, [bool]$Condition, [string]$Details = "") {
    if ($Condition) {
        Write-Host "  ✅ PASS: $TestName" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  ❌ FAIL: $TestName ($Details)" -ForegroundColor Red
        $script:failed++
    }
}

$defaultHeaders = @{}
if ($ApiKey) {
    $defaultHeaders["X-Api-Key"] = $ApiKey
}

Write-Host "`n=======================================================" -ForegroundColor Cyan
Write-Host " Running RemindPe API Security & Integration Tests" -ForegroundColor Cyan
Write-Host " Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "=======================================================`n" -ForegroundColor Cyan

# --- TEST 1: Health Check Endpoint ---
Write-Host "[1] Checking /health Liveness & Database Readiness..." -ForegroundColor Yellow
try {
    $healthRes = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing
    Assert-Test "/health returns 200 OK" ($healthRes.StatusCode -eq 200)
    Assert-Test "/health reports Healthy and SQLite status" ($healthRes.Content -match "Healthy" -or $healthRes.Content -match "online")
} catch {
    Assert-Test "/health endpoint reachable" $false $_.Exception.Message
}

# --- TEST 2: Security Headers & Correlation ID ---
Write-Host "`n[2] Checking Security Headers & Distributed Tracing..." -ForegroundColor Yellow
try {
    $customCorrId = "test-corr-" + [Guid]::NewGuid().ToString("N")
    $headers = @{ "X-Correlation-ID" = $customCorrId }
    if ($ApiKey) { $headers["X-Api-Key"] = $ApiKey }

    $headersRes = Invoke-WebRequest -Uri "$BaseUrl/api/customers/stats" -Headers $headers -UseBasicParsing
    
    $corrEcho = $headersRes.Headers["X-Correlation-ID"]
    Assert-Test "Correlation ID is echoed back" ($corrEcho -eq $customCorrId) "Expected $customCorrId, got $corrEcho"
    
    $contentTypeOpt = $headersRes.Headers["X-Content-Type-Options"]
    Assert-Test "X-Content-Type-Options: nosniff" ($contentTypeOpt -eq "nosniff")
    
    $frameOpt = $headersRes.Headers["X-Frame-Options"]
    Assert-Test "X-Frame-Options: SAMEORIGIN" ($frameOpt -eq "SAMEORIGIN")
    
    $xssOpt = $headersRes.Headers["X-XSS-Protection"]
    Assert-Test "X-XSS-Protection: 1; mode=block" ($xssOpt -match "1;\s*mode=block")
    
    $refOpt = $headersRes.Headers["Referrer-Policy"]
    Assert-Test "Referrer-Policy header present" ($refOpt -ne $null)

    $cspOpt = $headersRes.Headers["Content-Security-Policy"]
    Assert-Test "Content-Security-Policy header present" ($cspOpt -ne $null -and $cspOpt -match "default-src 'self'")
} catch {
    Assert-Test "Security headers verification" $false $_.Exception.Message
}

# --- TEST 3: Dashboard Stats Aggregation API ---
Write-Host "`n[3] Checking /api/customers/stats performance & schema..." -ForegroundColor Yellow
try {
    $stats = Invoke-RestMethod -Uri "$BaseUrl/api/customers/stats" -Headers $defaultHeaders
    Assert-Test "TotalCustomers >= 0" ($stats.totalCustomers -ge 0)
    Assert-Test "TotalReceivable >= 0" ($stats.totalReceivable -ge 0)
    Assert-Test "TotalReceived >= 0" ($stats.totalReceived -ge 0)
    Assert-Test "TotalTransactions >= 0" ($stats.totalTransactions -ge 0)
} catch {
    Assert-Test "/api/customers/stats schema" $false $_.Exception.Message
}

# --- TEST 4: Customers List API ---
Write-Host "`n[4] Checking /api/customers retrieval..." -ForegroundColor Yellow
$customers = @()
try {
    $customers = Invoke-RestMethod -Uri "$BaseUrl/api/customers" -Headers $defaultHeaders
    Assert-Test "Customers list returned as array" ($customers -is [System.Array])
    Assert-Test "At least 1 customer exists in database" ($customers.Length -gt 0)
} catch {
    Assert-Test "/api/customers retrieval" $false $_.Exception.Message
}

# --- TEST 5: Negative Validation - Customer Creation ---
Write-Host "`n[5] Checking Input Validation & Negative Boundaries..." -ForegroundColor Yellow
try {
    # Empty name should fail 400
    $body = @{ name = ""; phone = "9876543210" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/api/customers" -Method POST -Body $body -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Empty name rejected" $false "Should have failed with 400"
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Assert-Test "Empty name rejected with 400 Bad Request" ($statusCode -eq 400)
}

try {
    # Invalid short phone should fail 400
    $body = @{ name = "Test Client"; phone = "123" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/api/customers" -Method POST -Body $body -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Invalid phone rejected" $false "Should have failed with 400"
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Assert-Test "Invalid short phone rejected with 400" ($statusCode -eq 400)
}

# --- TEST 6: Negative Validation - Transactions ---
Write-Host "`n[6] Checking Transaction Input Validation & Negative Bounds..." -ForegroundColor Yellow
try {
    # Zero / Negative amount should fail
    $body = @{ customerId = 1; amount = 0; type = "Credit"; note = "Zero test" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/api/transactions" -Method POST -Body $body -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Zero amount transaction rejected" $false "Should have failed with 400"
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Assert-Test "Zero amount rejected with 400 Bad Request" ($statusCode -eq 400)
}

try {
    # Non-existent customer should fail with 404
    $body = @{ customerId = 999999; amount = 500; type = "Credit"; note = "Ghost test" } | ConvertTo-Json
    Invoke-RestMethod -Uri "$BaseUrl/api/transactions" -Method POST -Body $body -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Non-existent customer rejected" $false "Should have failed with 404"
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Assert-Test "Non-existent customer returns 404 Not Found" ($statusCode -eq 404)
}

# --- TEST 7: CRUD Lifecycle & Phase 2 Features (Atomic Opening Balance, Duplicate Check, PUT, GET Transactions) ---
Write-Host "`n[7] Testing Customer & Transaction CRUD Lifecycle..." -ForegroundColor Yellow
try {
    $randomSuffix = (Get-Random -Minimum 100000 -Maximum 999999)
    $custName = "AutoTest Client $randomSuffix"
    $uniquePhone = "9" + (Get-Random -Minimum 100000000 -Maximum 999999999)

    # 7.1: Create customer with atomic opening balance
    $body = @{ 
        name = $custName
        phone = $uniquePhone
        openingBalance = 1500
        openingType = "Credit"
    } | ConvertTo-Json
    
    $newCust = Invoke-RestMethod -Uri "$BaseUrl/api/customers" -Method POST -Body $body -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Customer created with atomic opening balance" ($newCust.id -gt 0 -and $newCust.netBalance -eq 1500)

    # 7.2: Duplicate phone check rejection
    try {
        $dupBody = @{ name = "Duplicate Client"; phone = $uniquePhone } | ConvertTo-Json
        Invoke-RestMethod -Uri "$BaseUrl/api/customers" -Method POST -Body $dupBody -ContentType "application/json" -Headers $defaultHeaders
        Assert-Test "Duplicate phone rejected" $false "Should have failed with 400"
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Assert-Test "Duplicate phone correctly rejected with 400" ($statusCode -eq 400)
    }

    # 7.3: Update customer (PUT /api/customers/{id})
    $updatedName = "$custName - Updated"
    $updateBody = @{ name = $updatedName; phone = $uniquePhone } | ConvertTo-Json
    $updatedCust = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$($newCust.id)" -Method PUT -Body $updateBody -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Customer updated via PUT" ($updatedCust.name -eq $updatedName)

    # 7.4: Add Debit Transaction (payment received)
    $txBody = @{ customerId = $newCust.id; amount = 500; type = "Debit"; note = "Payment test" } | ConvertTo-Json
    $newTx = Invoke-RestMethod -Uri "$BaseUrl/api/transactions" -Method POST -Body $txBody -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Debit transaction recorded" ($newTx.id -gt 0 -and $newTx.amount -eq 500)

    # 7.5: Verify updated net balance (1500 credit - 500 debit = 1000)
    $custDetail = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$($newCust.id)" -Headers $defaultHeaders
    Assert-Test "Net balance accurately computed" ($custDetail.netBalance -eq 1000)

    # 7.6: Edit Transaction (PUT /api/transactions/{id})
    $editTxBody = @{ amount = 600; type = "Debit"; note = "Corrected payment" } | ConvertTo-Json
    $editedTx = Invoke-RestMethod -Uri "$BaseUrl/api/transactions/$($newTx.id)" -Method PUT -Body $editTxBody -ContentType "application/json" -Headers $defaultHeaders
    Assert-Test "Transaction edited via PUT" ($editedTx.amount -eq 600 -and $editedTx.note -eq "Corrected payment")

    $custDetailAfterEdit = Invoke-RestMethod -Uri "$BaseUrl/api/customers/$($newCust.id)" -Headers $defaultHeaders
    Assert-Test "Customer balance updated after transaction edit" ($custDetailAfterEdit.netBalance -eq 900)

    # 7.7: Cross-customer transactions endpoint (GET /api/transactions)
    $txList = Invoke-RestMethod -Uri "$BaseUrl/api/transactions?customerId=$($newCust.id)&page=1&pageSize=10" -Headers $defaultHeaders
    Assert-Test "GET /api/transactions returns paginated result" ($txList.totalCount -ge 2 -and $txList.items.Count -ge 2)

    # 7.8: Settings Endpoint (GET /api/settings and PUT /api/settings)
    $settings = Invoke-RestMethod -Uri "$BaseUrl/api/settings" -Headers $defaultHeaders
    Assert-Test "GET /api/settings returns valid merchant profile" ($settings.storeName.Length -gt 0 -and $settings.currency.Length -gt 0)

    $updateSettingsBody = @{
        storeName = "Apex Supermarket"
        ownerName = "Ritesh Parmar"
        phone = "9876543210"
        upiId = "apex@oksbi"
        currency = "INR"
        defaultReminderTemplate = "hinglish"
        receiptFooter = "Thank you for shopping with Apex Supermarket!"
    } | ConvertTo-Json
    $utf8Body = [System.Text.Encoding]::UTF8.GetBytes($updateSettingsBody)
    $updatedSettings = Invoke-RestMethod -Uri "$BaseUrl/api/settings" -Method PUT -Body $utf8Body -ContentType "application/json; charset=utf-8" -Headers $defaultHeaders
    Assert-Test "PUT /api/settings updates store name" ($updatedSettings.storeName -eq "Apex Supermarket" -and $updatedSettings.upiId -eq "apex@oksbi")

    # 7.9: Reports Endpoint (GET /api/reports/summary)
    $reportSummary = Invoke-RestMethod -Uri "$BaseUrl/api/reports/summary" -Headers $defaultHeaders
    Assert-Test "GET /api/reports/summary returns verified ledger figures" ($reportSummary.transactionCount -ge 0 -and $reportSummary.totalCreditGiven -ge 0)

    # 7.10: Dashboard Stats Endpoint (GET /api/dashboard/stats)
    $dashStats = Invoke-RestMethod -Uri "$BaseUrl/api/dashboard/stats" -Headers $defaultHeaders
    Assert-Test "GET /api/dashboard/stats returns real KPI counts" ($dashStats.totalCustomers -ge 1 -and $dashStats.totalReceivable -ge 0)

    # 7.11: Delete Customer with cascade
    $delCustRes = Invoke-WebRequest -Uri "$BaseUrl/api/customers/$($newCust.id)" -Method DELETE -Headers $defaultHeaders -UseBasicParsing
    Assert-Test "Customer deleted with cascade" ($delCustRes.StatusCode -eq 204)

} catch {
    Assert-Test "CRUD Lifecycle completed" $false $_.Exception.Message
}

Write-Host "`n=======================================================" -ForegroundColor Cyan
Write-Host " TEST EXECUTION SUMMARY: Passed: $passed, Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host "=======================================================`n" -ForegroundColor Cyan

if ($failed -gt 0) {
    exit 1
}
