$host.ui.RawUI.WindowTitle = 'build.. ' + (Get-Item -Path "." -Verbose).Name


dotnet tool restore
dotnet tool run dotnet-cake --version
dotnet tool run dotnet-cake build.cake --bootstrap
dotnet tool run dotnet-cake build.cake --target=PublishLocalPackages --verbosity=Verbose --skiptest --configuration=Debug

if ($LASTEXITCODE -ne 0) {
    # Write-Error "Build Error..."
    pause
}
