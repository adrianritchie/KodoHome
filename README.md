# Project For KodoHome
This project is a set of applications for interacting with my HomeAssistant.  Some applications may be useful to others

## Application: SeaTemperature

Collect sea temperature for Guernsey and store it in a temperature sensor: `sensor.sea_temperature`

## Development

Update dependencies:
```update_all_dependencies.ps1```

Regenerate HomeAssistant code (fix parameter name duplication `target`):
```dotnet tool run nd-codegen```

Publish update:
```publish.ps1```