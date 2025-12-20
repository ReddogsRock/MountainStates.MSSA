<image src="app-icon.png" align="right" width="200px">

# CTA 3 (Call To Action) App for .net CMSs

> This is a [2sxc](https://2sxc.org) App for [DNN ☢️](https://www.dnnsoftware.com/) and [Oqtane 💧](https://www.oqtane.org/)

A Call-To-Action App to motivate customer interaction

| Aspect              | Status | Comments or Version
| ------------------- | :----: | -------------------
| 2sxc                | ✅    | requires 2sxc v17.07.00
| Dnn                 | ✅    | For v9.6.1+
| Oqtane              | ✅    | Requires v05.00+
| No jQuery           | ✅    |
| Live Demo           | ➖    |
| Install Checklist   | ✅    | See [Installation](https://azing.org/2sxc/r/wwLfP_ve) on [azing.org](https://azing.org/2sxc)
| Source & License    | ✅    | included, ISC/MIT
| App Catalog         | ➖    | See [app catalog](https://2sxc.org/en/apps/app/cta-v3-for-dnn-and-oqtane)
| Screenshots         | ➖    | See [app catalog](https://2sxc.org/en/apps/app/cta-v3-for-dnn-and-oqtane)
| Best Practices      | ✅    | Uses v16.02 conventions
| Bootstrap 3         | ✔️    |
| Bootstrap 4         | ✅    |
| Bootstrap 5         | ✅    |

## Customize the App

The Counter App doesn't use any app settings or app resources.

If you want to customize the CSS, you will usually follow the ["Create Custom Styles in a Standard 2sxc App" checklist](https://azing.org/2sxc/r/gg_aB9FD)

## History

* v03.00.00 2021-10
  * Updated to best-practices of 2sxc 12.05
  * Hybrid, so it works with Oqtane
  * Tested to look ok on Bootstrap5
* v03.01.00 2022-01
  * Enabled ajax-reload
  * Set default view
  * Updated License
* v03.02.00 2022-04
  * Changed Services to ToSic.Sxc.Services
  * Activated image configuration
* v03.03.00 2022-06
  * Replaced all base classes with their 2sxc 14 equivalent
  * Removed all GetService<> and used the new ServiceKit14
  * Changed all toolbars to use the new IToolbarService
* v03.04.00 2023-03
  * Replaced Tag.Custom with `Kit.HtmlTags`
  * Enhanced Kit.Image with `imgAltFallback`
  * Replace Edit.Toolbar with `@Kit.Toolbar.Default`
  * Removed _ from Filenames
  * Code in one file the bs5, less duplicated code
  * Add special toolbar for is-demo items
* v03.05.00 2023-06
  * Upgraded to conventions 16.02
  * Changed all code to be typed
* v03.17.00 2024-04
  * Strong Typed
  * Auto Generated Class
  * Typed MyItem
* v03.17.01 2024-07
  * Update app.sln and app.csproj