---
title: Changelog
description: Tracks changes and bug fixes between different versions of Bonobo Git Server for Windows
tags: [Changelog, Changes, Bug Fixes, Features]
---

## Version 5.2

**17 March 2016**

### Security

This is an important security release adding a CSRF protection to POST actions in the app. Also, it fixes a token validation on password reset function and adds the CSRF protection there as well.

* add form antiforgery protection - Will Dean


## Version 5.1.1

**12 January 2016**

### Bug Fixes

* add Sqlite.Interop.dll to the project so it is part of the release

## Version 5.1

**11 January 2016**

### Features

* display general and personal repository URL as links - padremortius
* add Danish translation - larshg
* add Italian translation - Andrea Capigiri
* improve Japanese translation - mattn
* improve Chinese translation - StarryLibra
* improve French translation - latop2604
* Active Directory updates - Matt Bodily
	* use nested groups for permissions
	* allow logging in without specifying the AD domain (should use the default for all AD look-ups if one is not specified)
	* update so username at login is not case sensitive when retrieving roles
* External links functionality - kabongsteve
* increase repository logo quality by using PNG - mischalandwehr

### Bug Fixes

* exporting correct user environment variable for AD - BIPrc
* removing confidence requirement for file type - larshg
* fixed subfolder application redirection to root - Alex Moran
* fix error when changing URL - Alex Moran
* prevent repository buttons breaking - mischalandwehr
* fix multiple tags support - kabongsteve

### Code improvements

* install SQLite from nuget - padremortius
* start using MediaTypeMap - padremortius
* improve .gitignore - n.kochnev

## Version 5.0.1

**5 November 2015**

### Features

* add go to repository after creation - erdemyavuzyildiz

## Bug Fixes

* ADRepository username handling - larshg
* cookie authentication issue fix - bogusz
* don't strip domain in AD membership service - larshg
* fix teams and AD - larshg
* fix team deletation - BurhanEyimaya


## Version 5.0.0

**22 October 2015**

This is a major release as Ollienator simplified and consolidated authorization and also added new providers, but your current web.config could be out of date and might need an update. Check out the new docs and update your web.config accordingly.

### Features

* major rework of authentication and authorization - ollienator
* simplification of Active Directory integration (no need to run 2 servers) - ollienator
* authentication through OWIN and ADFS - ollienator
* updated nuget packages and libgit2sharp - amonomen
* client based culture and brazil translation - darioajr
* msysgit update 1.9.5 - larshg
* remove origin branch after cloning - latop2604
* allow relative repository path - lhko
* better error handling - matt-17

### Bug fixes

* improved detection of windows-1252 encoding - larshg
* fix typo errors - isaksson
* using UI date time format - crowar
* fix compile error:x64/SQLite.Interop.dll not found - myh


## Version 4.0.0

**11 Jun 2015**

### Features

* can run on Azure Website
* email check supports new long tld - restartz
* authenticated user name available on push - kholme2
* add logo to repository - sansys
* remove default port from repository view - ivanstus
* add file info (line count, size) - lkho

### Bug fixes

* fix incorrect encoding in blob preview - colinniu
* improved project infrastructure - robbforce
* fix bug with edit/view non-domain users with enabled domain integration - padremortius
* fix problem with not possible edit/delete account with domain authorization - padremortius
* fix errors in highlight.js - padremortius
* fix git clone depth 1 - silvanperego

## Version 3.6.0

**2 Apr 2015**

### Features

* new commit message format - alexkuznetsov
* french translation - glacasa
* show personalised URL - sansys
* added support for grouping repositories - lennardf1989
* minimize group - sansys
* link to commited changes - spoiledtechie

### Bug fixes

* fixed history and blame page - igoryk-zp
* fixed back link - igoryk-zp
* fixed russian translation - sansys

## Version 3.5.0

**19 Feb 2015**

### Features

* Remember me checkbox - whosa
* zh-HK, zh-CHT translation and improved encoding - lkho
* Improved commit layout - whosa
* Repository allows dot and underscore in the name - mbedded
* Convert tabs into spaces in blob and commit view - jafp
* Enable password reset - kengibous
* Add tags to commit view - heringeidaniel

### Bug fixes

* Fix #207 Remove the home variable from process info before adding it
* Rescuing from IdentityNotMap Exception for Windows Authentication - jshepler
* Fix to allow email addresses as users names, Issue #163 #158 - kengibous
* zh-TW improved - tooto


## Version 3.4.3

**14 Dec 2014**

### Features

* Display readme.md in repository browser - kengibous

### Bug Fixes

* Fix broken download link for files - latop2604


## Version 3.4.2

**11 Dec 2014**

### Features

* Support for large files and large repos - kfarnung
* Displaying markdown in repo browser - kengibous

### Bug Fixes

* Disabling post commit auditing as it causes problems with certain clients - stanshillis

## Version 3.4.1

**2 Dec 2014**

### Bug fixes

* Made commit details parsing more robust for ReceivePackHook - kfarnung

## Version 3.4

**30 Nov 2014**

### Features

* Post commit hook - stanshillis
* Commit auditing (username recording) - stashillis
* Keep selected branch on all pages - stanshillis
* Polish translation - Bartlomiej Kaminski
* History view for files - Igor Nakonechnyi
* Assembly version displayed in footer - Kyle Engibous
* Display avatar in commits page - Igor Nakonechnyi
* Blame for file - Igor Nakonechnyi

### Bug Fixes

* Disallow special characters for repository name - Matthias


## Version 3.3

**22 Aug 2014**

### Features

* Clone button for repositories in web management UI - latop2604
* Support for custom title, logo, additional footer message - OttoNull
* Add Active Directory group / Team synchronization - Louis-Charles Levasseur
* Add audit logging of login success or failure - dnadle

### Bug Fixes

* Add missing french translation keys - latop2604
* Add backwards compatible upgrade of method to store hashed passwords - embix
* Fix crash when repo contain GitLink node - latop2604
* Added generic message, if commit message is null or empty - SeitzDev
* Fix #133 JSON body displayed when going back in repository view
*

## Version 3.2

**19 May 2014**

### Features

* Repository browser performance improvement
* Asynchronnous load of commit messages in browser
* Better English localization
* Improved deployment process
* Nuget cleanup


### Bug Fixes

* Fixed #102 Create Team button is missing for Windows Authentication mode
* Fixed #104 Missing highlight.pack.js
* Fixed #117 Split commit messages


<hr />

## Version 3.1

**27 March 2014**

### Features

* zh-TW Traditional Chinese Translation added - [doggy8088](https://github.com/doggy8088)

### Bug Fixes

* Fix German Localization - [AliveDevil](https://github.com/AliveDevil)
* Fixed dissapearing menu
* Fixed CSS Virtual Path (#99 and #100) - [kfarnung](https://github.com/kfarnung)


<hr />

## Version 3.0

**18 March 2014**

### Features

* Major redesign
* Diff view for files
* Short SHA in commit view
* Changed lines added to commits

### Bug Fixes

* Fix #93 Browsing cshtml extensions 
* Fix #94 Wrong date display for different locale



<hr />

## Version 2.1

**3 March 2014**

### Features

* RAW file display
* Scanning for existing repositories
* Select a default language in settings section
* Efficient working with streams
* Swedish localization - [JLedel](https://github.com/JLedel)
* Russian localization - grigoryev
* Spanish localization - [AHTA](https://github.com/AHTA)
* Download repository as ZIP - [Rémy de Sérésin](https://github.com/latop2604)

### Bug Fixes

* Fixed a problem viewing files with '+' or '&' in the path
* Fixed a problem viewing branches and tags with '/' in the name
* Fixed missing label for team members when creating team
* Tweaked English strings


<hr />


## Version 2.0.1

**30 August 2013**

### Features

* Displaying current username in Windows Authentication mode

### Bug Fixes

* Fixed the problem with repository view for normal users in Windows Authentication mode


<hr />



## Version 2.0

**25 August 2013**

### Features

* Windows Authentication Support
* Spanish Translation

### Bug Fixes

* Usernames are normalized to invariant lowercase.
* Fix text in Chinese translation


### Compatibility Issues

* Converts all the usernames to lowercase. 
	* Keep that in mind while logging
	* Only ASCII chars are supported even for existing usernames, if you have other characters in your username it is recommended to create a new user.


<hr />


## Version 1.3.0 

**30 June 2013**

### Features

* Gitsharp removed
* Switched to libgit2
* Tag support added
* Improved repository browser - blog support and faster navigation

### Bug Fixes

* Enable repository browser view  of ASP.NET special folders
* Enable repository browser view of any extension
* Fix a crash issue if there is not master branch - [Yubo Xie](https://github.com/xieyubo)
* Fix a crash if user browses an empty repository - [Yubo Xie](https://github.com/xieyubo)
* Fixing incorrect hint place - [TheBlueSky](https://github.com/TheBlueSky)

<hr />


## Version 1.2.0

**30 May 2013**

### Features 

* Replaced multiselects with the checkbox lists - Mark N
* Turkish translation - [zafer06](https://github.com/zafer06)
* One URL for secure and anonymous access - [Aimeast](https://github.com/Aimeast)
* Default settings file is created automatically when not exists - [Aimeast](https://github.com/Aimeast)
* Default database is created automatically when not exists - [Aimeast](https://github.com/Aimeast)
* EF 5.0 code first introduction - [Aimeast](https://github.com/Aimeast)
* Switched to ASP.NET MVC 4 and .NET 4.5
* Nuget packages used for external dependencies
* Allow to pass username and password from URL
* Removed git.aspx from URL
* Settings must be set before the first use
* Git logo added

### Bug Fixes

* Changing password for normal user
* Display large binary files
* Hashing password with proper encoding - [Aimeast](https://github.com/Aimeast)
* Max allowed content length set to 4MB
* Page width set to 980px - [Aimeast](https://github.com/Aimeast)
* Fixed integration with TeamCity - [micchickenburger](https://github.com/micchickenburger)

### Compatibility Issues

* Password is not compatible with the previous version due to encoding change.
    * For fixing this issue please use [sqlite administrator](http://sqliteadmin.orbmu2k.de/), open the database file located in App_Data and change your record in the table User and set the field Password to *0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3* which means *admin*.
    * You can run this sql statement `UPDATE User SET Password = '0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3' WHERE Username = 'YOUR USERNAME'`
* Database name changed from Bonobo.Git.Server.Release.db to Bonobo.Git.Server.db
    * Go to App_Data folder and rename the file
* Windows Server 2003 is not supported because of the ASP.NET MVC 4.5 and .NET 4.5 versions
    * IIS 7+ and .NET 4.5 and ASP.NET MVC 4.5 is required to run Bonobo Git Server

<hr />


## Version 1.1.0

**9 October 2011**

### Features 

* Administrator can create new user accounts
* Settings
    * Disable anonymous user registration (by default user registration is disabled and only administrator can create new users)
	* Disable creation repository by users (by default only administrator can create new repositories)
* Confirmation for Team, User and Repository removals
    * Security hole with delete on GET removed
* Download link for text based files in repository browser
* Chinese translation added
* Japanese translation added
* Reasonable states returned to git client if authentication failed or if repository does not exists

### Bug Fixes

* Fixed issue with UserConfiguration (config.xml) was invalid after overwriting  with custom values
* MaxRequestLength extended to 100MB for large file upload
* RequestLimit for IIS 7 extended to 100MB for large file upload
* Fixed redirecting from Create actions
* Fixed URL redirecting after Create and Delete actions
* Favicon application error fixed
* Fixed FormsAuthentication redirect call after basic authentication returns 401(and fire up runtime exception)
* Fixed repository delete with read only files
