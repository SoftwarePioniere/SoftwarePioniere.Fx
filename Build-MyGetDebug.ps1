$host.ui.RawUI.WindowTitle = 'build.. ' + (Get-Item -Path "." -Verbose).Name


$subscription = 'sopi-mpn'
$keyVaultName = 'softwarepioniere'
az account set --subscription $subscription
$mygetapikey = (az keyvault secret show --vault-name $keyVaultName --name MyGetApiKey --subscription $subscription --query 'value' --output tsv)

dotnet cake build.cake --target=PublishPackages --verbosity=Verbose --configuration=Debug --mygetapikey=$mygetapikey


if ($LASTEXITCODE -ne 0) {
    # Write-Error "Build Error..."
    pause
}
