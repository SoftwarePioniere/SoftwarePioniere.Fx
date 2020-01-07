$host.ui.RawUI.WindowTitle = 'env.. ' + (Get-Item -Path "." -Verbose).Name


# if (Test-Path 'temp') {
#   Remove-Item -Path temp -Force -Recurse
# }
  
# New-Item -Path temp -ItemType Directory

docker-compose -p sopfix down --v
docker-compose -p sopfix up 
docker-compose -p sopfix down --v