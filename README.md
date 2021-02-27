# Trak Library App

<!-- Project shields -->
[![Build Status][appcenter-android-badge]][appcenter-url]
[![Build Status][appcenter-ios-badge]][appcenter-url]
[![License][license-badge]][license-url]
[![LinkedIn][linkedin-badge]][linkedin-url]

## Getting started

### Prerequisites

- Xamarin.Forms - [Install Xamarin through Visual Studio](https://docs.microsoft.com/en-us/xamarin/get-started/installation/windows).
- NET.Core 3.1 - [Download & Install NET.Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) and ensure it's accessible via an environment variable.
- Trak Library API - [Download & run](https://github.com/sparky-studios/trak-api) the Trak Library API by following the provided constructions. 

### Development setup

#### Setting up a project
Create a **secrets.json** file within the SparkyStudios.TrakLibrary project and add the following values:

```json
{
  "AppCenterAndroidSecret": "****",
  "AppCenterIOSSecret": "****",
  "EnvironmentUrl": "****"
}
```

- The **AppCenterAndroidSecret** will need to be set to the app center ID of the Android project, which can be seen [here](https://appcenter.ms/orgs/sparky-studios/apps/trak-app) after an Administrator has given you the valid permissions.
- The **AppCenterIOSSecret** will need to be set to the app center ID of the iOS project, which can be seen [here](https://appcenter.ms/orgs/sparky-studios/apps/trak-app-1)  after an Administrator has given you the valid permissions.
- The base address of your locally running **Trak Library API**. For local development it should be set to **http://{your local IP}:8080/**.

<!-- Badges -->
[appcenter-android-badge]: https://build.appcenter.ms/v0.1/apps/4f185b2b-a340-46cf-b6ea-3becc6548bb5/branches/develop/badge
[appcenter-ios-badge]: https://build.appcenter.ms/v0.1/apps/84bcad4c-e7ad-457c-b6dc-4c7ce276b3c7/branches/develop/badge
[appcenter-url]: https://appcenter.ms
[license-badge]: https://img.shields.io/badge/License-Apache%202.0-blue.svg
[license-url]: https://opensource.org/licenses/Apache-2.0
[linkedin-badge]: https://img.shields.io/badge/-LinkedIn-black.svg?style=flat-square&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/benjamin-carter-04a8a3114