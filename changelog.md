---
title: Changelog
description: Tracks changes and bug fixes between different versions of Bonobo Git Server for Windows
tags: [Changelog, Changes, Bug Fixes, Features]
---

## Version 3.1

**27 March 2014**

### Features

* zh-TW Traditional Chinese Translation added - [doggy8088](https://github.com/doggy8088)

### Bug Fixes

* Fix German Localization - [AliveDevil](https://github.com/AliveDevil)
* Fixed dissapearing menu
* Fixed CSS Virtual Path (#99 and #100) - [kfarnung](https://github.com/kfarnung)


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
    * For fixing this issue please use [sqlite administrator](http://sqliteadmin.orbmu2k.de/), open the database file located in App_Data and change your record in the table User and set the field Password to *21232F297A57A5A743894A0E4A801FC3* which means *admin*.
    * You can run this sql statement `UPDATE User SET Password = '21232F297A57A5A743894A0E4A801FC3' WHERE Username = 'YOUR USERNAME'`
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
