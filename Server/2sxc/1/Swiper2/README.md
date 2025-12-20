<image src="app-icon.png" align="right" width="200px">

# Swiper 2 - Slider/Carousel App for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

CMS Slider App using the popular [Swiper JS](https://swiperjs.com/). It does not use jQuery 😉.


| Aspect              | Status | Comments or Version
| ------------------- | :----: | -------------------
| 2sxc                | ✅    | Requires v17.07.00
| Dnn                 | ✅    | Requires v09.06.01
| Oqtane              | ✅    | Requires v05.00+
| No jQuery           | ✅    | Since v 2.01
| Live Demo           | ✅    | See [blog with live demo](https://2sxc.org/en/blog/post/awesome-swiper-js-slider)
| Install Checklist   | ✅    | See [Installation](https://azing.org/2sxc/r/9yfw-D1D) on [azing.org](https://azing.org/2sxc)
| Source & License    | ✅    | included, MIT
| App Catalog         | ✅    | See [app catalog](https://2sxc.org/en/apps/app/swiper-image-slider-pro-v2-with-parallax-3d-cubes-coverflow-and-more-hybrid-for-dnn-and-oqtane)
| Best Practices      | ✅    | Uses v13.10 conventions
| Bootstrap 4         | ✅    | v4 optimized
| Bootstrap 5         | ✅    | v5 optimized

## Customize the App

If you want to customize the CSS, you will usually follow the ["Create Custom Styles in a Standard 2sxc App" checklist](https://azing.org/2sxc/r/gg_aB9FD)


## Thanks to

[SwiperJS](https://swiperjs.com/) for their awesome open-source library

## History

* v.02.00.00 2021-05
  * updated to 2sxc 12.0 standards
  * made hybrid / compatible with Oqtane
  * Removed jQuery
* v.02.01.00 2021-09
  * updated to 2sxc 12.05 standards
  * updated SwiperJs to V7
  * renamed app-folder to Swiper2
  * css classes now using a no-conflict model allowing side-by-side install with older swiper
  * App has a new GUID for identification allowing side-by-side install with older swiper
  * Upgrade to SwiperJS v7
* v.02.03.00 2022-02
  * Enabled data-optimizations
  * Fixed a bug that caused single slides to appear as duplicate
* v.02.04.00 2022-04 Version
  * Changed access to Services to ToSic.Sxc.Services
  * Change all images to use latest Picture-tag technology
  * Removed the PictureTag helper
  * Enabled image configuration
  * Replaced data-enableoptimizations with pageSvc.AssetAttributes()
* v.02.05.00 2022-06
  * Changed all base classes to their 2sxc 14 equivalents
  * Replaced all GetService<> with the new ServiceKit14
  * Changed all the toolbar configurations to use the IToolbarService
  * Updated webpack
* v.02.06.00 2023-05
  * Removed _ from Filenames
  * Code in one file the bs5, less duplicated code
  * Enhanced Kit.Image with `imgAltFallback`
  * Replaced turnOn Tag with `Kit.Page.TurnOn`
* v02.07.00 2023-07
  * 2sxc 16.02 coding conventions
  * everything typed
* v02.17.00 2024-04
  * strong typed
  * Auto Generated Class
  * Typed MyItem
* v02.17.01 2024-07
  * Update app.sln and app.csproj