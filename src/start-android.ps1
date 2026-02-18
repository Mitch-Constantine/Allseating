param(
    [switch]$Network,
    [switch]$ApiOnly
)
$runApp = -not $ApiOnly

$ErrorActionPreference = "Stop"
$srcRoot = $PSScriptRoot
$apiDir = Join-Path $srcRoot "api\Allseating.Api"
$androidDir = Join-Path $srcRoot "android"

$sdkRoot = $env:ANDROID_HOME
if (-not $sdkRoot) { $sdkRoot = $env:ANDROID_SDK_ROOT }
if (-not $sdkRoot) { $sdkRoot = Join-Path $env:LOCALAPPDATA "Android\Sdk" }
$adb = Join-Path $sdkRoot "platform-tools\adb.exe"
$emulator = Join-Path $sdkRoot "emulator\emulator.exe"

function Invoke-Adb {
    param([string[]]$Arguments)
    $argStr = ($Arguments | ForEach-Object { if ($_ -match '\s') { "`"$_`"" } else { $_ } }) -join " "
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $adb
    $psi.Arguments = $argStr
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $psi
    $p.Start() | Out-Null
    $stdout = $p.StandardOutput.ReadToEnd()
    $stderr = $p.StandardError.ReadToEnd()
    $p.WaitForExit() | Out-Null
    if ($p.ExitCode -ne 0) {
        $msg = if ($stderr.Trim()) { $stderr.Trim() } else { "adb exited with code $($p.ExitCode)" }
        Write-Error "adb exit $($p.ExitCode): $msg"
    }
    $stdout
}

function Test-DeviceConnected {
    if (-not (Test-Path $adb)) { return $false }
    $out = Invoke-Adb -Arguments "devices"
    $lines = $out -split "`n" | Where-Object { $_ -match "^\S+\s+device\s*$" }
    return $lines.Count -gt 0
}

function Get-EmulatorAvds {
    if (-not (Test-Path $emulator)) { return @() }
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $emulator
    $psi.Arguments = "-list-avds"
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $psi
    $p.Start() | Out-Null
    $stdout = $p.StandardOutput.ReadToEnd()
    $stderr = $p.StandardError.ReadToEnd()
    $p.WaitForExit() | Out-Null
    if ($p.ExitCode -ne 0) {
        $msg = if ($stderr.Trim()) { $stderr.Trim() } else { "emulator -list-avds exited with code $($p.ExitCode)" }
        Write-Error "emulator exit $($p.ExitCode): $msg"
    }
    return ($stdout -split "`n" | Where-Object { $_.Trim() })
}

function Start-EmulatorAndWait {
    if (-not (Test-Path $emulator)) { return $false }
    $avdList = Get-EmulatorAvds
    if (-not $avdList -or $avdList.Count -eq 0) { return $false }
    $avd = $avdList[0].Trim()
    Start-Process -FilePath $emulator -ArgumentList "-avd", $avd -WindowStyle Normal
    $deviceWait = 90
    $elapsed = 0
    while ($elapsed -lt $deviceWait) {
        Start-Sleep -Seconds 3
        $elapsed += 3
        if (Test-DeviceConnected) { break }
    }
    if (-not (Test-DeviceConnected)) { return $false }
    $bootWait = 120
    $elapsed = 0
    while ($elapsed -lt $bootWait) {
        $prop = Invoke-Adb -Arguments "shell", "getprop", "sys.boot_completed"
        if ($prop -match "1") { return $true }
        Start-Sleep -Seconds 3
        $elapsed += 3
    }
    return $false
}

function Ensure-DeviceReady {
    if (Test-DeviceConnected) { return $true }
    if (-not $runApp) { return $true }
    return $false
}

$launchPath = Join-Path $apiDir "Properties\launchSettings.json"
$launch = Get-Content -Raw $launchPath | ConvertFrom-Json
$httpUrl = ($launch.profiles.http.applicationUrl).Trim()
$httpPort = ([System.Uri]$httpUrl).Port

$deviceReady = $true
if ($runApp) { $deviceReady = Ensure-DeviceReady }
if ($runApp -and -not $deviceReady) {
    Write-Warning "No device/emulator available; starting backend only. Connect a device or start an emulator, then run with -InstallApp or install from Android Studio."
    $runApp = $false
}

if ($Network) {
    $env:ASPNETCORE_URLS = "http://0.0.0.0:$httpPort"
    $ip = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notmatch 'Loopback' -and $_.IPAddress -notmatch '^169\.' } | Select-Object -First 1).IPAddress
    if ($ip) {
        $buildGradle = Join-Path $androidDir "app\build.gradle.kts"
        if (Test-Path $buildGradle) {
            $baseUrl = "http://${ip}:$httpPort/"
            $replacement = 'buildConfigField("String", "API_BASE_URL", "\"' + $baseUrl + '\")"'
            $content = Get-Content -Raw $buildGradle
            $content = $content -replace 'buildConfigField\("String", "API_BASE_URL", "\\"[^"]*\\""\)', $replacement
            Set-Content -Path $buildGradle -Value $content -NoNewline
        }
    }
}

Write-Host "Building backend..."
Push-Location $apiDir
try {
    dotnet build | Out-Null
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} finally {
    Pop-Location
}

Write-Host "Starting backend..."
$cmd = "cd '$apiDir'; dotnet run --no-build --launch-profile http"
if ($Network) {
    $cmd = "`$env:ASPNETCORE_URLS='http://0.0.0.0:$httpPort'; $cmd"
}
Start-Process powershell -ArgumentList "-NoExit", "-Command", $cmd

$maxWait = 45
$waited = 0
$hostToCheck = "127.0.0.1"
while ($waited -lt $maxWait) {
    try {
        $conn = New-Object System.Net.Sockets.TcpClient($hostToCheck, $httpPort)
        $conn.Close()
        break
    } catch {
        Start-Sleep -Seconds 1
        $waited++
    }
}
if ($waited -ge $maxWait) { Write-Host "Backend did not respond in time." -ForegroundColor Red; exit 1 }
Write-Host "Backend ready." -ForegroundColor Green

if ($runApp) {
    $gradlew = Join-Path $androidDir "gradlew.bat"
    if (!(Test-Path $gradlew)) {
        Push-Location $androidDir
        try {
            $null = Get-Command gradle -ErrorAction SilentlyContinue
            if ($?) { gradle wrapper --gradle-version 8.2 }
        } finally { Pop-Location }
    }
    if (Test-Path $gradlew) {
        $jdk17 = $null
        $candidates = @(
            (Get-ChildItem -Path "$env:USERPROFILE\.jdks" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '17' } | Select-Object -First 1 -ExpandProperty FullName),
            (Get-ChildItem -Path "C:\Program Files\Eclipse Adoptium" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'jdk-17' } | Select-Object -First 1 -ExpandProperty FullName),
            (Get-ChildItem -Path "C:\Program Files\Microsoft" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'jdk-17' } | Select-Object -First 1 -ExpandProperty FullName),
            (Get-ChildItem -Path "C:\Program Files\Java" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match 'jdk-17' } | Select-Object -First 1 -ExpandProperty FullName)
        )
        foreach ($p in $candidates) { if ($p -and (Test-Path (Join-Path $p "bin\java.exe"))) { $jdk17 = $p; break } }
        if ($jdk17) { $env:JAVA_HOME = $jdk17; Write-Host "Using JDK 17: $jdk17" }
        elseif ($env:JAVA_HOME) { Write-Host "Using JAVA_HOME: $env:JAVA_HOME (JDK 17 recommended for Android build)" -ForegroundColor Yellow }
        Write-Host "Installing app on device..."
        Push-Location $androidDir
        try {
            & .\gradlew.bat installDebug
            if ($LASTEXITCODE -ne 0) { Write-Host "App install failed (exit $LASTEXITCODE)." -ForegroundColor Red; exit $LASTEXITCODE }
            if (Test-Path $adb) {
                $null = Invoke-Adb -Arguments "shell", "am", "start", "-n", "com.allseating.android/.MainActivity"
            }
            Write-Host "App installed and launched." -ForegroundColor Green
        } finally { Pop-Location }
    } else {
        Write-Host "gradlew.bat not found; app not installed. Open src\android in Android Studio to generate the wrapper." -ForegroundColor Yellow
    }
}
Write-Host "Done."
