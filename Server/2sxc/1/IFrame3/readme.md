<image src="app-icon.png" align="right" width="200px">

# IFrame 3 App for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

An IFrame App to use with 2sxc

| Aspect              | Status | Comments or Version
| ------------------- | :----: | -------------------
| 2sxc                | ✅    | IFrame v3 requires 2sxc v17.07.00
| Dnn                 | ✅    | For v9.6.1+
| Oqtane              | ✅    | Requires v05.00+
| No jQuery           | ✅    |
| Live Demo           | ➖    |
| Install Checklist   | ✅    | See [Installation](https://azing.org/2sxc/r/-7DXsLjq) on [azing.org](https://azing.org/2sxc)
| Source & License    | ✅    | included, ISC/MIT
| App Catalog         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/iframe3-hybrid-for-dnn-and-oqtane)
| Screenshots         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/iframe3-hybrid-for-dnn-and-oqtane)
| Best Practices      | ✅    | Uses v13.10 conventions
| Bootstrap 3, 4, 5   | ✅    |


## Customize the App

Configuration (Settings, Resources)

The IFrame App has neither app settings nor app resources.

Since it's super simple and open-source, you can adjust as you need.

## History

* v.01.00.00 ca. 2014
* v.02.00.00 ca. 2020
* v.03.00.00 2021-09
  * Updated to 2sxc 12 Standard and made hybrid for use in Oqtane
* v.03.01.00 2022-04
  * Moved all access of Services to ToSic.Sxc.Services
  * Changed `<img>` tags to use IImageService
* v.03.02.00 2022-06
  * Replaced all base classes with their 2sxc 14 equivalents
  * Replaced GetService<> with the new ServiceKit14
* v.03.03.00 2023-02
  * Removed _ from Filenames
* v03.04.00 2023-05
  * 2sxc 16.02 coding conventions
  * everything typed
* v03.17.00 2024-04
  * strong typed
  * Auto Generated Class
* v03.17.01 2024-07
  * Update app.sln and app.csproj 