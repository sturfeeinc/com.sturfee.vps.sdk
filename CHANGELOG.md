# Changelog


## [3.1.3] - 2022-07-18

### Changes

- Added editor-code that installs `Sturfee-VPS-SDK.unitypackage` as soon as the SDK package is installed
- Added more Japanese translations that covers all the text appearing on screen
- Protobuf libraries updated

## Fixes
- Fixed blank screen image getting sent to VPS service when using post processing on iOS

## [3.1.2] - 2022-06-18

## [3.1.1] - 2022-06-18

### Changes

- Package hosted on OpenUPM

## [3.0.7] - 2022-06-16

### Fixes
- Re-fixed the flicker issue that got introduced due to change in GLTF library in 3.0.3

## [3.0.6] - 2022-06-15

## [3.0.5] - 2022-06-03

### Fixes

- Fixes GPS Error issue when localization scan is performed 2nd time onwards in a session

## [3.0.4] - 2022-05-26

### Changes 

- Websocket connection for localization is now opened and closed only when VPS is enabled and disabled. Earlier it was open thoroughout the session

## [3.0.3] - 2022-05-13

### Changes

- Removed Sturfee's Custom GLTF library and replaced that with a forked UnityGLTF repository hosted here https://github.com/prefrontalcortex/UnityGLTF.git?path=/UnityGLTF/Assets/UnityGLTF#release/pfc/1.6.1-pre.3


## [3.0.2] - 2022-04-08

### Changes

- Removed Id validation on multiple Localization requests within a same session

## [3.0.1] - 2022-03-18

### Changes

- Replaces Sturfee's Transparent shader for occlusion with ARFoundation's Spatial Mapping Occlusion Shader

### Fixes

- Fixed flicker issue 




