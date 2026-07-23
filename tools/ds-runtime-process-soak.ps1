param(
    [Parameter(Mandatory = $true)]
    [int]$ProcessId,

    [Parameter(Mandatory = $true)]
    [string]$CsvPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

'timestamp,workingSetBytes,privateBytes,cpuSeconds' | Set-Content -LiteralPath $CsvPath

for ($Index = 0; $Index -le 60; $Index++) {
    $Process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
    if ($null -eq $Process) {
        break
    }

    Add-Content -LiteralPath $CsvPath -Value ((Get-Date).ToString('o') + ',' + $Process.WorkingSet64 + ',' + $Process.PrivateMemorySize64 + ',' + $Process.CPU)

    if ($Index -lt 60) {
        Start-Sleep -Seconds 30
    }
}
