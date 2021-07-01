# Changelog

## [2.2.0] - February 15, 2021
### General Changes
- Added improved UI
- Configurable UI Color themes and Localized Strings
- Added sample Sturfee color theme for the UI 
- Added "Look Here" scene with UI to perform re-localization
- Initial Look Up Circle raised by 5 degrees to encourage user to look up  before Scan
- Added "CameraDirection" option for XRCamera to allow use of Front Camera. This feature is limited only to "AR Provider Set"

### Bug Fixes
- Tile Cache is auto cleared when corrupted during tile loading. Avoids the need to reinstall app when tile corruption is encountered
- Re-localization scan (at "Look Here") message from VPS server is not displayed on the screen but instead logged as a warning in console


##[2.1.1] - January 18, 2021
### General Changes
- New data format for sample scenes to avoid pinging VPS in demo mode
- Option to add 360 photo spheres for immersive demo experience              
- Multi-language support added. Use Sturfee/Configure window's Config tab. Currently supported languages : English, Japanese

### Bug Fixes
- Fixes all scan UI related bugs

### Breaking Changes
- Current Sample scene example updated to have new data format
- Sample names used for sample scene is no longer supported from this version of SDK.



##[2.1.0] - November 2, 2020
### General Changes
- Replaces GoogleARCore and UnityArKit plugins with ARFoundation
- Scan restrictions added to start the scan at a slightly higher pitch. UI updated correspondingly   
- Config tab added to Sturfee Configuration Window. You can now configure the tile load size

### Bug Fixes
- N/A



##[2.1.0] - November 2, 2020
### General Changes
- Replaces GoogleARCore and UnityArKit plugins with ARFoundation
- Scan restrictions added to start the scan at a slightly higher pitch. UI updated correspondingly   
- Config tab added to Sturfee Configuration Window. You can now configure the tile load size

### Bug Fixes
- N/A


##[2.0.3] - September 8, 2020
### General Changes
- Added option to Clear TileCache from Sturfee Menu
- Developer portal button added in Sturfee configuration window that links to Developer Portal
- Config tab added to Sturfee Configuration Window. You can now configure the tile load size

### Bug Fixes
- Fixes display grid projector bug where it does not show grid at high altitudes
- Fixes iOS build crash in Xcode during linking


##[2.0.2] - August 30, 2020
### General Changes
- Added support for IL2CPP build
- Building shadows turned OFF
- Tile caching added 
- Sample provider set defaults to a location in San Jose

### Bug Fixes
- N/A



##[2.0.1] - July 19, 2020
### General Changes
- Added support for IL2CPP build
- Exposes Localization request details in `OnFrameCaptured` event           
- Sample Provider Set defaults to a location in San Francisco
- Building and terrain tiles on separate layers
- Tile loading radius increased to 600m

### Bug Fixes
- Stop Scanning button fixed



##[2.0.0] - Deccember 2, 2019
- First Release
