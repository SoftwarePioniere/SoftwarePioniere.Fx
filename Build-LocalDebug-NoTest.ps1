$host.ui.RawUI.WindowTitle = 'build.. ' + (Get-Item -Path "." -Verbose).Name

dotnet cake --target=BuildPackLocalPush --configuration=Debug
#.\build.ps1 -Target BuildPackLocalPush -configuration Debug --verbosity=verbose

if ($LASTEXITCODE -ne 0) {
    # Write-Error "Build Error..."
    pause
}
