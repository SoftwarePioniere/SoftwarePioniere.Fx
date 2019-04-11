$host.ui.RawUI.WindowTitle = 'build.. ' + (Get-Item -Path "." -Verbose).Name

docker-compose up

dotnet cake --target=BuildTestPackLocalPush --configuration=Debug
#.\build.ps1 -Target BuildTestPackLocalPush -configuration Debug --verbosity=verbose

docker-compose down --v

if ($LASTEXITCODE -ne 0) {
    # Write-Error "Build Error..."
    pause
}
