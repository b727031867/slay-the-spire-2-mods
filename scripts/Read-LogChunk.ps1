# Incremental log reader used by Invoke-ModTests to poll a live game log.
# Only complete lines are decoded: every returned Text chunk starts at a line boundary,
# so markers and ERROR lines can never be split across polls. Incomplete trailing bytes
# stay in Pending (as bytes) so multi-byte characters and CRLF pairs survive partial reads.
function Read-LogChunk {
    param(
        [Parameter(Mandatory)][string]$Path,
        [long]$Offset = 0,
        [byte[]]$Pending = [byte[]]::new(0),
        [int]$MaxBytes = 1048576
    )
    $result = [pscustomobject]@{ Text=''; Offset=$Offset; Pending=$Pending; Reset=$false }
    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) { return $result }
    $stream = [IO.File]::Open($Path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
    try {
        # A truncated or rotated log restarts the scan from the beginning.
        if ($stream.Length -lt $Offset) {
            $Offset = 0
            $Pending = [byte[]]::new(0)
            $result.Reset = $true
        }
        $null = $stream.Seek($Offset, [IO.SeekOrigin]::Begin)
        $remaining = $stream.Length - $Offset
        $buffer = [byte[]]::new([int][Math]::Min([long]$MaxBytes, $remaining))
        if ($buffer.Length -eq 0) {
            $result.Offset = $Offset
            $result.Pending = $Pending
            return $result
        }
        $read = $stream.Read($buffer, 0, $buffer.Length)
        $Offset += $read
        $combined = [byte[]]::new($Pending.Length + $read)
        [Array]::Copy($Pending, 0, $combined, 0, $Pending.Length)
        [Array]::Copy($buffer, 0, $combined, $Pending.Length, $read)
        $lastLineBreak = -1
        for ($i = $combined.Length - 1; $i -ge 0; $i--) { if ($combined[$i] -eq 10) { $lastLineBreak = $i; break } }
        if ($lastLineBreak -ge 0) {
            $result.Text = [Text.Encoding]::UTF8.GetString($combined, 0, $lastLineBreak + 1)
            $Pending = [byte[]]::new($combined.Length - $lastLineBreak - 1)
            [Array]::Copy($combined, $lastLineBreak + 1, $Pending, 0, $Pending.Length)
            # Strip a UTF-8 BOM only when the scan starts at the file start.
            if ($Offset -eq $read -and $result.Text.Length -gt 0 -and $result.Text[0] -eq [char]0xFEFF)
            { $result.Text = $result.Text.Substring(1) }
        } else {
            $Pending = $combined
        }
    } finally { $stream.Dispose() }
    $result.Offset = $Offset
    $result.Pending = $Pending
    $result
}
