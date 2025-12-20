<image src="app-icon.png" align="right" width="200px">

# Image HotSpots 3 App for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

A awesome image hotspots app to use with 2sxc

| Aspect              | Status | Comments or Version
| ------------------- | :----: | -------------------
| 2sxc                | ✅    | requires 2sxc v17.07.00
| Dnn                 | ✅    | For v9.6.1+
| Oqtane              | ✅    | Requires v05.00+
| No jQuery           | ✅    | Since v3
| Live Demo           | ➖    |
| Install Checklist   | ✅    | See [Installation](https://azing.org/2sxc/r/ccfIKnqk) on [azing.org](https://azing.org/2sxc)
| Source & License    | ✅    | included, ISC/MIT
| App Catalog         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/image-hotspots-v3-hybrid-for-dnn-and-oqtane)
| Screenshots         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/image-hotspots-v3-hybrid-for-dnn-and-oqtane)
| Best Practices      | ✅    | Uses v13.10 conventions
| Bootstrap 4         | ✅    | v4 optimized
| Bootstrap 5         | ✔️    | Tested & works but not optimized

## Customize the App

You can configure a lot, and since it's open-source you can also change anything you want.

If you want to customize the CSS, you will usually follow the ["Create Custom Styles in a Standard 2sxc App" checklist](https://azing.org/2sxc/r/gg_aB9FD)

## History

* v.03.00.00 2021-09
  * Update to 2sxc 12.05 standard
  * Hybrid for Dnn & Oqtane
  * dropped jQuery
  * cleaner code
* v.03.01.00 2022-04
  * Changed all access to services to ToSic.Sxc.Services
  * Changed all images to use IImageService
  * Activated image configuration
* v.03.02.00 2022-06
  * Changed all base classes to their 2sxc 14 equivalents
  * Replaced all GetService<> with the new ServiceKit14
  * Updated webpack
  * Changed all toolbar configurations to use the IToolbarService
* v.03.02.01 2022-07
  * Changed the typescript typings to the new Typescript API
* v.03.03.00 2023-05
  * Removed _ from Filenames
  * Enhanced Kit.Image with `imgAltFallback`
  * Replaced turnOn Tag with `Kit.Page.TurnOn`
* v03.04.00 2023-07
  * 2sxc 16.02 coding conventions
  * everything typed
* v03.04.01 2023-11
  * Bugfixes
* v03.17.00 2024-04
  * Strong Typed
  * Auto Generated Class
  * Typed MyItem
