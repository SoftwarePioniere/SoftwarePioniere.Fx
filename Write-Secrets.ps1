[Cmdletbinding()]
Param(   
   
)

$secretId = 'sopitest'
$subscription = 'sopi-mpn'
$keyVaultName = 'softwarepioniere'

# =========================================================
function secret([String] $key , [String] $val) {
    $key = $key.Replace("__",":")

    Write-Host "Set Secret: $key // $val"
    dotnet user-secrets set $key $val --id $secretId
}
# =========================================================

(dotnet user-secrets clear --id $secretId)

az account show

if ($LASTEXITCODE -eq 1) {
    az login
}


$x = (az keyvault secret show --vault-name $keyVaultName --name SopiTestCosmosDbPrimaryKey --subscription $subscription --query 'value' --output tsv)
secret 'AZURECOSMOSDB__AUTHKEY' $x

$x = (az keyvault secret show --vault-name $keyVaultName --name SopiTestCosmosDbEndpoint --subscription $subscription --query 'value' --output tsv)
secret 'AZURECOSMOSDB__ENDPOINTURL' $x

$x = (az keyvault secret show --vault-name $keyVaultName --name SopiTestAzureAdClientId --subscription $subscription --query 'value' --output tsv)
secret 'AZUREADCLIENT__CLIENTID' $x

$x = (az keyvault secret show --vault-name $keyVaultName --name SopiTestAzureAdClientSecret --subscription $subscription --query 'value' --output tsv)
secret 'AZUREADCLIENT__CLIENTSECRET' $x

$x = (az keyvault secret show --vault-name $keyVaultName --name SopiTestAuth0ClientId --subscription $subscription --query 'value' --output tsv)
secret 'AUTH0CLIENT__CLIENTID' $x

$x = (az keyvault secret show --vault-name $keyVaultName --name SopiTestAuth0ClientSecret --subscription $subscription --query 'value' --output tsv)
secret 'AUTH0CLIENT__CLIENTSECRET' $x