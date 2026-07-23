$CsvPath = 'C:\dev\helprojs\demodisc\output\ps2-indexed-cubes-regenerated\ps2-menu-idle-soak.csv'
$BootLogPath = 'C:\dev\helprojs\demodisc\output\ps2-indexed-cubes-regenerated\ps2_bootlog.txt'
$ProcessId = 41168

'timestamp,workingSetBytes,cpuSeconds,bootLogLength,lastBootLine' | Set-Content -LiteralPath $CsvPath

for ($Index = 0; $Index -le 60; $Index++) {
    $Process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
    if ($null -eq $Process) {
        break
    }

    $BootLog = Get-Item -LiteralPath $BootLogPath
    $LastBootLine = (Get-Content -LiteralPath $BootLogPath -Tail 1) -replace '[\r\n,]', ' '
    Add-Content -LiteralPath $CsvPath -Value ((Get-Date).ToString('o') + ',' + $Process.WorkingSet64 + ',' + $Process.CPU + ',' + $BootLog.Length + ',' + $LastBootLine)

    if ($Index -lt 60) {
        Start-Sleep -Seconds 30
    }
}
