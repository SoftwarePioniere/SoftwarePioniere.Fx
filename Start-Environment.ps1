$host.ui.RawUI.WindowTitle = 'env.. ' + (Get-Item -Path "." -Verbose).Name

docker-compose up
docker-compose down --v