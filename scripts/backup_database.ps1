# ==============================================================================
# RemindPe - Production SQLite Database Hot Backup Script
# Performs safe online snapshot backup of khata.db with SQLite VACUUM INTO
# and PRAGMA integrity_check verification
# ==============================================================================

param(
    [string]$SourceDb = "khata.db",
    [string]$BackupDirectory = "backups"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SourceDb)) {
    Write-Error "Database file '$SourceDb' not found in current directory."
    exit 1
}

if (-not (Test-Path $BackupDirectory)) {
    New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null
    Write-Host "Created backup directory: $BackupDirectory" -ForegroundColor Cyan
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$destBackup = Join-Path $BackupDirectory "khata_backup_$timestamp.db"
$destBackupAbs = (Resolve-Path $BackupDirectory).Path + "\khata_backup_$timestamp.db"

Write-Host "Creating online zero-lock snapshot backup of '$SourceDb' -> '$destBackup'..." -ForegroundColor Yellow

$backupSuccess = $false

# 1. Preferred Method: In-process SQLite VACUUM INTO with integrity check via RemindPe CLI
try {
    $proc = Start-Process -FilePath "dotnet" -ArgumentList "run","--no-build","--","--backup",$destBackupAbs -NoNewWindow -Wait -PassThru
    if ($proc.ExitCode -eq 0 -and (Test-Path $destBackup)) {
        $backupSuccess = $true
        Write-Host "✅ Zero-lock VACUUM snapshot created and integrity verified via SQLite engine." -ForegroundColor Green
    }
}
catch {
    Write-Warning "RemindPe CLI backup unavailable ($($_.Exception.Message)). Falling back to atomic copy..."
}

# 2. Fallback: Safe file copy if CLI not accessible
if (-not $backupSuccess) {
    Copy-Item -Path $SourceDb -Destination $destBackup -Force
    if (Test-Path "$SourceDb-wal") {
        Copy-Item -Path "$SourceDb-wal" -Destination "$destBackup-wal" -Force
    }
    if (Test-Path "$SourceDb-shm") {
        Copy-Item -Path "$SourceDb-shm" -Destination "$destBackup-shm" -Force
    }
    Write-Host "⚠️ Created copy-based backup fallback." -ForegroundColor Yellow
}

$fileSize = (Get-Item $destBackup).Length
Write-Host "✅ Backup completed: $destBackup ($([math]::Round($fileSize / 1KB, 2)) KB)" -ForegroundColor Green

# Retention policy: Keep last 30 backups
$allBackups = Get-ChildItem -Path $BackupDirectory -Filter "khata_backup_*.db" | Sort-Object CreationTime -Descending
if ($allBackups.Count -gt 30) {
    $toDelete = $allBackups | Select-Object -Skip 30
    foreach ($item in $toDelete) {
        Remove-Item $item.FullName -Force -ErrorAction SilentlyContinue
        Remove-Item "$($item.FullName)-wal" -Force -ErrorAction SilentlyContinue
        Remove-Item "$($item.FullName)-shm" -Force -ErrorAction SilentlyContinue
        Write-Host "Pruned old backup: $($item.Name)" -ForegroundColor DarkGray
    }
}
