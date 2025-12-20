<image src="app-icon.png" align="right" width="200px">

# QR Code 2 App for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

| Aspect              | Status | Comments or Version
| ------------------- | :----: | -------------------
| 2sxc                | ✅    | requires 2sxc v17.07.00
| Dnn                 | ✅    | For v9.6.1+
| Oqtane              | ✅    | Requires v05.00+
| No jQuery           | ✅    |
| Live Demo           | ➖    |
| Install Checklist   | ✅    | See [Installation](https://azing.org/2sxc/r/TCmnZ17h) on [azing.org](https://azing.org/2sxc)
| Source & License    | ✅    | included, ISC/MIT
| App Catalog         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/qr-code-v2-hybrid-for-dnn-and-oqtane)
| Screenshots         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/qr-code-v2-hybrid-for-dnn-and-oqtane)
| Best Practices      | ✅    | Uses v13.10 conventions
| Bootstrap 3         | ✅    | No optimizations needed
| Bootstrap 4         | ✅    | No optimizations needed
| Bootstrap 5         | ✅    | No optimizations needed

A 2sxc / DNN App to create custom QR Codes.

## Customize the App

The QR Code App has neither app settings nor app resources

The App uses inline-styles and style tags, which you can directly adjust in the templates.

## Special Thanks to

<https://papnkukn.github.io/qrcode-svg/> for an awesome JavaScript QR-Code Generator

## History

* v.01.00 ca. 2015
* v.02.00 2021-10
  * Changed from tokens to Razor
  * Tested on Oqtane
* v.02.00.01 2022-03
  * Updated License
  * Changed default view
* v.02.00.02 2022-03
  * Bugfix style file
* v.02.01.00 2022-04
  * Changed all access to Services to ToSic.Sxc.Services
  * Replaced data-enableoptimization with pageSvc.AssetAttributes()
* v.02.02.00 2022-06
  * Replaced all the base classes with their 2sxc 14 equivalents
  * Changed all GetService<> to the new ServiceKit14
  * Updated webpack
* v.02.03 2023-02
  * Replaced turnOn Tag with `Kit.Page.TurnOn`
  * Removed _ from Filenames
* v02.04.00 2023-07
  * 2sxc 16.02 coding conventions
  * everything typed
* v02.17.00 2024-04
  * strong typed
  * Auto Generated Class
* v02.17.01 2024-07
  * Update app.sln and app.csproj
