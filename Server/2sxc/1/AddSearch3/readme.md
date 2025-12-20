<img src="app-icon.png" align="right" width="200px">

# AddSearch App for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

| Aspect              | Status | Comments or Version 
| ------------------- | :----: | -------------------
| 2sxc                | ✅    | requires v17.07.00
| Dnn                 | ✅    | For v9.6.1+
| Oqtane              | ✅    | Requires v05.00+
| No jQuery           | ✅    |
| Live Demo           | ➖    |
| Install Checklist   | ✅    | See [Installation](https://azing.org/2sxc/r/TBMhK1YB) on [azing.org](https://azing.org/2sxc)
| Source & License    | ✅    | included, ISC/MIT
| App Catalog         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/addsearch-app-v3-hybrid-for-dnn-and-oqtane)
| Screenshots         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/addsearch-app-v3-hybrid-for-dnn-and-oqtane)
| Best Practices      | ✅    | Uses v13.10 conventions
| Bootstrap 3         | ✅    |
| Bootstrap 4         | ✅    |
| Bootstrap 5         | ✅    |

## Customize the App

If you want to customize the CSS, you will usually follow the ["Create Custom Styles in a Standard 2sxc App" checklist](https://azing.org/2sxc/r/gg_aB9FD)

To configure the app settings of the AddSearch App you can use the ["Configuring AddSearch" checklist](https://azing.org/2sxc/r/LCKhUXm5).
In the app resources you can change the placeholder of the search fields.


## History

* v03.00.00 2022-03
  * Latest best-practices of 2sxc 12.05
  * clear/full separation of bs3/bs4/bs5
  * no more jQuery
* v03.01.00 2022-05
  * Replaced data-enableoptimizations with IPageService.AssetAttributes
* v03.02.00 2022-06 
  * Replaced all base classes with their new sxc14 equivalents
  * Updated webpack
  * Switched from GetService<> to Kit.Service
* v03.03.00 2023-05
  * Removed _ from Filenames
  * Code in one file the bs5, less duplicated code
  * Replaced turnOn Tag with `Kit.Page.TurnOn`
* v03.04.00 2023-07
  * 2sxc 16.02 coding conventions
  * everything typed
* v03.17.00
  * Strong Typed
  * Auto Generated Class
  * Typed MyItem
* v03.17.01 2024-07
  * Removed BS3
  * Update app.sln and app.csproj