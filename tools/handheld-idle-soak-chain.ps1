$ThreeDsCsvPath = 'C:\dev\helprojs\demodisc\output\3ds-nav-leak-20260719b\3ds-menu-idle-soak.csv'
$DsOutputPath = 'C:\dev\helprojs\demodisc\output\ds-nav-leak-20260719b'
$DsCsvPath = Join-Path $DsOutputPath 'ds-menu-idle-soak.csv'
$DsRomPath = Join-Path $DsOutputPath 'helengine_ds.nds'
$DsEmulatorPath = 'C:\dev\helworks\emus\desmume-0.9.13-win64\DeSmuME_0.9.13_x64.exe'

while ($true) {
    if (Test-Path -LiteralPath $ThreeDsCsvPath) {
        $ThreeDsSamples = @(Import-Csv -LiteralPath $ThreeDsCsvPath)
        if ($ThreeDsSamples.Count -ge 61) {
            break
        }
    }

    Start-Sleep -Seconds 30
}

Get-Process -Name azahar -ErrorAction SilentlyContinue | Stop-Process -Force
$DsProcess = Start-Process -FilePath $DsEmulatorPath -ArgumentList @($DsRomPath) -PassThru
Start-Sleep -Seconds 5

'timestamp,workingSetBytes,privateBytes,cpuSeconds' | Set-Content -LiteralPath $DsCsvPath

for ($Index = 0; $Index -le 60; $Index++) {
    $Process = Get-Process -Id $DsProcess.Id -ErrorAction SilentlyContinue
    if ($null -eq $Process) {
        break
    }

    Add-Content -LiteralPath $DsCsvPath -Value ((Get-Date).ToString('o') + ',' + $Process.WorkingSet64 + ',' + $Process.PrivateMemorySize64 + ',' + $Process.CPU)

    if ($Index -lt 60) {
        Start-Sleep -Seconds 30
    }
}
