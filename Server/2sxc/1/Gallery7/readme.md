<image src="app-icon.png" align="right" width="200px">

# Gallery App V7 using Fancybox4 for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

A Gallery App using the [fancybox4](https://fancyapps.com/) javascript library.

| Aspect            | Status | Comments or Version                                                                                       |
| ----------------- | :----: | --------------------------------------------------------------------------------------------------------- |
| 2sxc              |   ✅   | requires 2sxc v17.07.00                                                                                   |
| Dnn               |   ✅   | For v9.6.1+                                                                                               |
| Oqtane            |   ✅   | Requires v05.00+                                                                                          |
| No jQuery         |   ✅   |
| Live Demo         |   ➖   |
| Install Checklist |   ✅   | See [Installation](https://azing.org/2sxc/r/WLu6KUI4) on [azing.org](https://azing.org/2sxc)              |
| Source & License  |   ✅   | included, ISC/MIT                                                                                         |
| App Catalog       |   ✅   | See [app catalog](https://2sxc.org/en/apps/app/gallery-app-v7-using-fancybox-4-hybrid-for-dnn-and-oqtane) |
| Screenshots       |   ✅   | See [app catalog](https://2sxc.org/en/apps/app/gallery-app-v7-using-fancybox-4-hybrid-for-dnn-and-oqtane) |
| Best Practices    |   ✅   | Uses v13.10 conventions                                                                                   |
| Bootstrap 3       |   ➖   | Not tested, but [v6.1](https://github.com/2sic/app-gallery/tree/v6-fancybox3) is                          |
| Bootstrap 4       |   ✅   |
| Bootstrap 5       |   ✅   |

## Customize the App

Gallery App V7 with Fancybox4 uses the new, [standard Settings-System in 2sxc](http://r.2sxc.org/settings) to configure it. It also has app resources (i18n labels) you can customize.

If you want to customize the CSS, you will usually follow the ["Create Custom Styles in a Standard 2sxc App" checklist](https://azing.org/2sxc/r/Lu5SDBqU)

---

## History

### Version 6

Version 6 is found here: <https://github.com/2sic/app-gallery/tree/v6-fancybox3>

- Updated to conventions of 2sxc 12.0 as a Hybrid Dnn/Oqtane App based on [this checklist](https://azing.org/2sxc/r/m0iSLifK)

- v06.01.00
  - Updated to features and conventions of 2sxc 12.03 based on [this checklist](https://azing.org/2sxc/r/KwXMhp8h)
  - Now uses the new `Settings.WebResources` and `Settings.Images` for global configuration
  - Renamed folder to enable side-by-side installation with older versions
  - Reset the GUID so it has a new ID now - allowing side-by-side installation with older versions
  - Dropped BS3 support - if anybody needs this, we recommend V5
  - Removed duplicate code, cleaned up everything
  - Albums can now also show Masonry-style galleries inside
- v07.00.00
  - Switched to Fancybox4
  - Dropped jQuery
  - Added turnOn to activate JS cleanly
  - Now uses dependencies from Settings.WebResources
  - Complete clean up / simplification of code files
  - Ensure that multiple galleries don't mix images in lightbox mode
  - Lots of documentation / comments everywhere
- v07.02.00
  - Moved all access of Services to ToSic.Sxc.Services
  - Activated image configuration
  - Replaced data-enableoptimization with IPageService.AssetAttributes()
  - Changed fancybox images to use the IImageService
- v07.03.00
  - Replaced all base classes with their 2sxc 14 equivalents
  - Changed from GetService<> to the new ServiceKit14
  - Changed the toolbars to use the IToolbarService
  - Updated webpack
- v07.04.00
  - Removed \_ from Filenames
  - Replaced turnOn Tag with `Kit.Page.TurnOn`
  - Code in one file the bs5, less duplicated code
- v07.05.00 2023-08
  - 2sxc 16.02 coding conventions
  - everything typed
  - changed image metadata to use new standard image metadata incl. crop features
  - add ability to set crop on all images
- v07.17.00 2024-04
  - strong typed
  - Auto Generated Class
- v07.17.01 2024-07
  - Update app.sln and app.csproj
- v07.17.02 2025-06
  - \*.webp extension/images now uploadable
