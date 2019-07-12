$host.ui.RawUI.WindowTitle = 'env.. ' + (Get-Item -Path "." -Verbose).Name

docker-compose -p sopfix up 
docker-compose -p sopfix down --v