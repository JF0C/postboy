$env:POSTBOY_INSTALL_DIR = "C:\Program Files\Postboy"
$env:POSTBOY_SVC_NAME = "Postboy"

dotnet publish -c Debug

New-Item -ItemType Directory -Force -Path $env:POSTBOY_INSTALL_DIR

Copy-Item -Path "Postboy\bin\Debug\net7.0\publish\*" -Destination $env:POSTBOY_INSTALL_DIR -PassThru -Recurse -Force

New-Service -Name "$env:POSTBOY_SVC_NAME" `
	-BinaryPathName "$env:POSTBOY_INSTALL_DIR\Postboy.exe --UserAppDir=$env:USERPROFILE\AppData\Roaming" `
	-Description 'kind of like postman, but cooler' `
	-DisplayName "$env:POSTBOY_SVC_NAME" `
	-StartupType 'Automatic'

Start-Service -Name $env:POSTBOY_SVC_NAME
