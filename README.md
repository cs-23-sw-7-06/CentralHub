# CentralHub

This project contains the components of the CentralHub.
The CentralHub takes measurements from [esp32-trackers](https://github.com/cs-23-sw-7-06/esp32-tracker) and aggregates it to derive a occupancy estimate for a given room.
It contains two servers.


## REST API

The REST API (CentralHub.Api) is called both by the trackers and the Web UI.
It exposes endpoints for managing rooms, trackers, and measurements.
Every 5 minutes it takes all measurements from the trackers and aggregates them to provide occupancy estimation for all registered rooms.


## Web UI

This (CentralHub.WebUI) uses the REST API and provides a user interface for the REST API.
More specifically, it allows the user to manage rooms and trackers, and show historical data for occupancy in a given registered room.


## Building

The project can be build in Visual Studio, any other IDE, or simply using the commands:

```bash
dotnet publish CentralHub.Api -c "Release"
dotnet publish CentralHub.WebUI -c "Release"
```

However, running on archlinux the [CentralHub-PKGBUILD](https://github.com/cs-23-sw-7-06/CentralHub-PKGBUILD) package is recommended instead as it provides preconfigured hardened systemd services for running this system.


### Tests

Unit tests are defined in the CentralHub.Api.Tests project and can be run with the command:

```bash
dotnet test
```
