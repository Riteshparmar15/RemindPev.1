# ==============================================================================
# RemindPe - Performance & High-Concurrency Benchmark Suite
# Simulates concurrent API load, calculates p50, p95, p99 latency and throughput
# ==============================================================================

param(
    [string]$BaseUrl = "http://localhost:5033",
    [int]$TotalRequests = 300,
    [int]$Concurrency = 10
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Net.Http

Write-Host "`n=======================================================" -ForegroundColor Cyan
Write-Host " RemindPe Scalability & Concurrency Load Benchmark" -ForegroundColor Cyan
Write-Host " Total Requests: $TotalRequests | Concurrency: $Concurrency" -ForegroundColor Cyan
Write-Host " Target Endpoint: $BaseUrl/api/customers/stats" -ForegroundColor Cyan
Write-Host "=======================================================`n" -ForegroundColor Cyan

$latencies = [System.Collections.Generic.List[double]]::new()
$errors = 0

$startTime = [System.Diagnostics.Stopwatch]::StartNew()

$scriptBlock = {
    param($url, $requestsPerWorker)
    Add-Type -AssemblyName System.Net.Http
    $workerLatencies = [System.Collections.Generic.List[double]]::new()
    $workerErrors = 0
    
    $client = [System.Net.Http.HttpClient]::new()
    $client.Timeout = [TimeSpan]::FromSeconds(10)

    for ($i = 0; $i -lt $requestsPerWorker; $i++) {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        try {
            $resp = $client.GetAsync($url).GetAwaiter().GetResult()
            $sw.Stop()
            if ($resp.IsSuccessStatusCode) {
                $workerLatencies.Add($sw.Elapsed.TotalMilliseconds)
            } else {
                $workerErrors++
            }
        } catch {
            $sw.Stop()
            $workerErrors++
        }
    }
    $client.Dispose()
    return @{ Latencies = $workerLatencies; Errors = $workerErrors }
}

$requestsPerWorker = [math]::Ceiling($TotalRequests / $Concurrency)
$runspacePool = [runspacefactory]::CreateRunspacePool(1, $Concurrency)
$runspacePool.Open()

$tasks = [System.Collections.Generic.List[hashtable]]::new()

for ($c = 0; $c -lt $Concurrency; $c++) {
    $ps = [powershell]::Create()
    $ps.RunspacePool = $runspacePool
    $ps.AddScript($scriptBlock).AddArgument("$BaseUrl/api/customers/stats").AddArgument($requestsPerWorker) | Out-Null
    
    $asyncResult = $ps.BeginInvoke()
    $tasks.Add(@{ PowerShell = $ps; AsyncResult = $asyncResult })
}

foreach ($t in $tasks) {
    $res = $t.PowerShell.EndInvoke($t.AsyncResult)
    $t.PowerShell.Dispose()
    
    if ($res) {
        $latencies.AddRange($res[0].Latencies)
        $errors += $res[0].Errors
    }
}

$runspacePool.Close()
$runspacePool.Dispose()

$startTime.Stop()
$totalSeconds = $startTime.Elapsed.TotalSeconds
$successful = $latencies.Count
$rps = [math]::Round($successful / $totalSeconds, 2)

# Sort latencies for percentiles calculation
$sorted = $latencies | Sort-Object

function Get-Percentile($list, [double]$pct) {
    if ($list.Count -eq 0) { return 0 }
    $index = [math]::Min([int]([math]::Ceiling(($pct / 100.0) * $list.Count) - 1), $list.Count - 1)
    return [math]::Round($list[$index], 2)
}

$p50 = Get-Percentile $sorted 50
$p90 = Get-Percentile $sorted 90
$p95 = Get-Percentile $sorted 95
$p99 = Get-Percentile $sorted 99
$min = if ($sorted.Count -gt 0) { [math]::Round($sorted[0], 2) } else { 0 }
$max = if ($sorted.Count -gt 0) { [math]::Round($sorted[-1], 2) } else { 0 }

Write-Host "BENCHMARK RESULTS:" -ForegroundColor Yellow
Write-Host "  • Total Requests Completed : $successful" -ForegroundColor White
Write-Host "  • Failed / Error Requests  : $errors" -ForegroundColor $(if ($errors -eq 0) { "Green" } else { "Red" })
Write-Host "  • Elapsed Total Time       : $([math]::Round($totalSeconds, 2)) seconds" -ForegroundColor White
Write-Host "  • Throughput               : $rps Requests/sec" -ForegroundColor Green
Write-Host "  • Latency Min              : $min ms" -ForegroundColor White
Write-Host "  • Latency p50 (Median)     : $p50 ms" -ForegroundColor Green
Write-Host "  • Latency p90              : $p90 ms" -ForegroundColor Green
Write-Host "  • Latency p95              : $p95 ms" -ForegroundColor Green
Write-Host "  • Latency p99              : $p99 ms" -ForegroundColor Green
Write-Host "  • Latency Max              : $max ms`n" -ForegroundColor White

@{
    TotalRequests = $successful
    Errors = $errors
    ThroughputRps = $rps
    Latency_p50_ms = $p50
    Latency_p95_ms = $p95
    Latency_p99_ms = $p99
} | ConvertTo-Json
