$env:POSTBOY_INSTALL_DIR = "C:\Program Files\Postboy"
$env:POSTBOY_SVC_NAME = "Postboy"

Remove-Service $env:POSTBOY_SVC_NAME
Remove-Item -Recurse -Force "$env:UserProfile\AppData\Roaming\Postboy"
Remove-Item -Recurse -Force $env:POSTBOY_INSTALL_DIR