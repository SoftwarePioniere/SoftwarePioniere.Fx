$host.ui.RawUI.WindowTitle = 'build.. ' + (Get-Item -Path "." -Verbose).Name

$env:SOPI_TESTS_EVENTSTORE__TCPPORT=1193
$env:SOPI_TESTS_EVENTSTORE__HTTPPORT=2193
$env:SOPI_TESTS_EVENTSTORE__EXTSECURETCPPORT=1195
$env:SOPI_TESTS_MONGODB__PORT=27097

dotnet tool restore
dotnet tool run dotnet-cake --version
dotnet tool run dotnet-cake build.cake --bootstrap
dotnet tool run dotnet-cake build.cake --target=PublishLocalPackages --verbosity=Verbose --configuration=Debug
#.\build.ps1 -Target BuildTestPackLocalPush -configuration Debug --verbosity=verbose

if ($LASTEXITCODE -ne 0) {
    # Write-Error "Build Error..."
    pause
}
