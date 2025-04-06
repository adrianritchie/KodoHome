# load variables from .env file
$envFilePath = ".env"
if (Test-Path $envFilePath) {
    $envContent = Get-Content $envFilePath
    foreach ($line in $envContent) {
        if ($line -match '^(.*?)=(.*)$') {
            $name = $matches[1]
            $value = $matches[2]
            Set-Variable -Name $name -Value $value -Scope Script
        }
    }
}
else {
    Write-Host "No .env file found. Skipping environment variable loading."
}

# Point to the HA PowerSHell Module
Unblock-File .\Home-Assistant\Home-Assistant.psd1
Unblock-File .\Home-Assistant\Home-Assistant.psm1
Import-Module .\Home-Assistant

New-HomeAssistantSession -ip  $ip -port $port -token $token

Invoke-HomeAssistantService -service HASSIO.ADDON_STOP -json $json

Remove-Item -Recurse -Force \\homeassistant.local\config\netdaemon5\*
dotnet publish -c Release .\KodoHome\KodoHome.csproj -o \\homeassistant.local\config\netdaemon5

Invoke-HomeAssistantService -service HASSIO.ADDON_START -json $json