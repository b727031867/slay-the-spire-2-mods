$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
. (Join-Path $root 'scripts/Read-LogChunk.ps1')
$fixture = Join-Path $root ('.local/incremental-log-tests/' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture -Force | Out-Null
$count = 0
function Check([bool]$condition, [string]$message) {
    if (!$condition) { throw $message }
    $script:count++
    Write-Output "PASS $message"
}
try {
    $log = Join-Path $fixture 'live.log'
    $utf8 = [Text.Encoding]::UTF8

    # No file yet: nothing to read, offset stays put.
    $chunk = Read-LogChunk -Path $log -Offset 0
    Check ($chunk.Text -eq '' -and $chunk.Offset -eq 0 -and $chunk.Pending.Length -eq 0) 'missing log yields no text'

    # A partial line must never be decoded or matched.
    [IO.File]::WriteAllText($log, "first line`nsecond li", $utf8)
    $chunk = Read-LogChunk -Path $log -Offset 0 -Pending $chunk.Pending
    Check ($chunk.Text -eq "first line`n" -and $chunk.Pending.Length -eq 9) 'partial trailing line stays pending'
    Check ($chunk.Offset -eq [IO.File]::ReadAllBytes($log).Length) 'offset advances by all bytes read, including pending'
    Check ($chunk.Text -notmatch '(?m)^second li') 'partial line cannot match a line-anchored pattern'

    # Multi-byte characters split across two reads must survive.
    [IO.File]::AppendAllText($log, "ne`n中文", $utf8)
    $chunk = Read-LogChunk -Path $log -Offset $chunk.Offset -Pending $chunk.Pending
    Check ($chunk.Text -eq "second line`n" -and $chunk.Pending.Length -eq 6) 'previous pending line completes once its newline arrives'
    Check ($chunk.Text -notmatch [char]0xFFFD) 'split multi-byte character not decoded early'

    [IO.File]::AppendAllText($log, "完成`n", $utf8)
    $chunk = Read-LogChunk -Path $log -Offset $chunk.Offset -Pending $chunk.Pending
    Check ($chunk.Text -eq "中文完成`n" -and $chunk.Pending.Length -eq 0) 'multi-byte split decoded without replacement characters'

    # Markers only need to be found in appended text.
    [IO.File]::AppendAllText($log, "[INFO] Steamworks shutdown succeeded!`n", $utf8)
    $chunk = Read-LogChunk -Path $log -Offset $chunk.Offset -Pending $chunk.Pending
    Check ($chunk.Text -match '(?m)^\[INFO\] Steamworks shutdown succeeded!') 'appended marker detected in incremental chunk'

    # No new bytes: empty text, offset unchanged.
    $before = $chunk.Offset
    $chunk = Read-LogChunk -Path $log -Offset $before -Pending $chunk.Pending
    Check ($chunk.Text -eq '' -and $chunk.Offset -eq $before) 'no new bytes yields empty chunk'

    # Truncation restarts from the beginning instead of seeking past the end.
    [IO.File]::WriteAllText($log, "rotated`n", $utf8)
    $chunk = Read-LogChunk -Path $log -Offset $before -Pending $chunk.Pending
    Check ($chunk.Reset -and $chunk.Text -eq "rotated`n" -and $chunk.Offset -eq [IO.File]::ReadAllBytes($log).Length) 'truncated log restarts from start'

    # CRLF at a read boundary: the newline is the split point, so no stray CR is emitted.
    [IO.File]::WriteAllText($log, "alpha`r`nbeta`r" + "`n", $utf8)
    $chunk = Read-LogChunk -Path $log -Offset 0 -Pending ([byte[]]::new(0))
    Check ($chunk.Text -eq "alpha`r`nbeta`r`n" -and $chunk.Pending.Length -eq 0) 'CRLF lines returned whole'

    # Large log: one bounded chunk per call, and repeated calls drain it.
    $many = ((1..3000 | ForEach-Object { "line $_`n" }) -join '')
    [IO.File]::WriteAllText($log, $many, $utf8)
    $chunk = Read-LogChunk -Path $log -Offset 0 -Pending ([byte[]]::new(0)) -MaxBytes 4096
    Check ($chunk.Text.Length -gt 0 -and $chunk.Text.Length -le 4096) 'read size bounded by MaxBytes'
    $total = $chunk.Text.Length
    $guard = 0
    while ($chunk.Offset -lt $utf8.GetByteCount($many) -and $guard++ -lt 100) {
        $chunk = Read-LogChunk -Path $log -Offset $chunk.Offset -Pending $chunk.Pending -MaxBytes 4096
        $total += $chunk.Text.Length
    }
    Check ($total -eq $many.Length) 'repeated bounded reads drain the whole log'
}
finally {
    Remove-Item -LiteralPath $fixture -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Output "INCREMENTAL_LOG_PASS assertions=$count (synthetic logs only; no game launched)"
