# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.3] - 12/10/2023
### Fixed
- Added support for sendgrid links to the URL Expander. This includes adding wildcard support to the URL Expander.
- Fixed URL Expanders window to open centred to its parent.

## [1.5.2] - 21/09/2023
### Fixed
- Fixed exception in filtering for browsers without a logged in user.
- Fixed scrolling for long lists of profiles
- Fixed help button position

## [1.5.1] - 21/09/2023
### Added
- Added the ability to skip rules when opening by holding the Alt key within the first 500ms.
- Added support for loading the url from the clipboard if no argument is passed
- Compact mode features:
  - Added a text filter instead of shortcut keys to better handle long lists of browsers.
  - Changed the "Always use" option to simply use the full domain name.
  - Added auto selection of the browser last used for the domain of the url. This allows simply pressing Enter to load in the last used browser.
### Fixed
- Fixed detection of chrome and edge profiles to only detect root level profiles so that snapshots (which appear as duplicates) are ignored.
- Fixed exception when choosing Always option in compact view
- Fixed handling of urls with ports


## [1.5.0] - 17/08/2023
### Added
- New Feature: Compact vertical mode
  - Includes display of username of chrome and edge profiles to help distinguish them
    - Note: requires "Refresh" of browsers to detect usernames
  - "Always" is accessible by right click context menu
### Changed
- Changed Browser detection to just use "Edge" and "Chrome" instead of "Microsoft Edge" and "Google Chrome" when naming profiles.
- Changed Settings, About, and Help windows to open on the same screen as the main window.
### Fixed
- Resolved issue with apply button getting enabled while the checkboxes and comboboxes are initialising.


## [1.4.4] - 17/10/2021
### Fixed
- Resolved issues with apply button in settings.


## [1.4.3] - 13/09/2021
### Fixed
- Updated help screens to match new filters
- Tidied up update checker
- Set default browser for newer versions of Windows
- Squashed bug selecting a filter row


## [1.4.2] - 09/09/2021
### Added
- Add support for O365 safelinks (expand these always)
- expand shortened urls e.g. adf.ly or goo.gl
  - UI displayed while loading
  - Only follow redirects for known URL shortners (list user configurable)
  - Added a timeout
  - Follow a maximum of 20 redirects
  - User "cancelable"
- go straight to settings if no URL parameter is received
- upgraded .net - this resolves some issues (e.g. https errors)
- updated browser filters rules to be more flexible:
  - Added ability to choose comparitor (Ends with, contains, regex, etc)
  - Added ability to compare domain, HTTP path, or full URL
  - Changed rules to be stored in Json format
  - Added auto import and conversion from old rule format
- other code clean up/fixes


## [1.4.1] - 24/08/2019
### Fixed
- Fixed couldn't hide chrome profiles separately (#52)
- Improved startup speed by caching browsers (#40)
(special thanks to [kthejoker](https://github.com/kthejoker) for his pull request)


## [1.4.0] - 12/06/2018
### Fixed
- Fixed Opera (post-blink) private mode (#35)
### Added
- Chrome profiles are now listed as separate options (#29)
(special thanks to [kueswol](https://github.com/kueswol) for his pull request)


## [1.3.9] - 06/04/2018
### Fixed
- Fixed Edge private mode (#34)
- Added Alt as an alternative to shift for open in private/incognito mode (#33)


## [1.3.8] - 20/10/2017
### Fixed
- Fixed pattern generator for single part domains (e.g. localhost) (issue #27)
- Fixed unintended unescaping of URL's (issue #28)


## [1.3.7] - 16/08/2017
### Fixed
- Fixed issues with clipping on high dpi screens (#24)


## [1.3.6] - 11/06/2017
### Changed
- BrowserSelect's window now shows up in the monitor with the mouse cursor instead of the default one (#22)


## [1.3.5] - 16/12/2016
### Fixed
- fixed crash on startup caused by incompatible/incomplete registry keys (issues #17,#20,#21)


## [1.3.4] - 02/09/2016
### Fixed
- fixed Always button adding rules with the wrong pattern for second-level domains (e.g. *.com.au for news.com.au)
### Added
- Shift Clicking on browsers now opens the URL in incognito/private browsing
- added an update checker (adds a yellow "New" icon to the main window to indicate a new version is available)[disabled by default]


## [1.3.3] - 03/08/2016
### Fixed
- fixed a crash on malformed (without protocol) URL's
### Added
- added donate button in about page


## [1.3.2] - 28/07/2016
### Fixed
- bugfix to bring IE to the foreground if it is already open


## [1.3.1] - 14/07/2016
### Fixed
- bugfix for Auto rule creation of domains with subdomains


## [1.3.0] - 11/07/2016
### Changed
- Added an "Always" button under browser icons that adds a rule for *.domain.tld
- Added a help button in the main form
- Added a help form for the settings page
- Added browser select to the list of options when adding rules
- made about form closable by Esc key
- changed how filters are executed to allow simpler use of a match-all pattern
- added an apply button to the settings page for Rules
- polished the rule adding interface
- some code Formating/Indenting/Restructuring


## [1.2.1] - 14/06/2016
### Fixed
- bugfix for InternetExplorer to open links in a new tab instead of a new window


## [1.2.0] - 08/06/2016
### Added
- you can now add URL patterns to select the Browser based on URL automatically.


## [1.1.0] - 18/05/2016
### Added
- added option to select browsers that are displayed on the list (and remove/hide some)


## [1.0.2] - 15/01/2016
### Added
- added option to set browser select as the default browser in settings


## [1.0.1] - 27/10/2015
### Added
- added edge browser for windows 10 (it wouldn't show up due edge being a Universal App)
