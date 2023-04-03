### THIS PLUGIN IS NOT UNDER DEVELOPMENT AND NOT SUPPORTED

## ContactsPlugin for Xamarin and Windows

Simple cross platform plugin to get Contacts from the device.

### Migrate to: [Xamarin.Essentials](https://docs.microsoft.com/xamarin/essentials/index?WT.mc_id=friends-0000-jamont) or [.NET MAUI](https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions?WT.mc_id=friends-0000-jamont)

I have been working on Plugins for Xamarin for a long time now. Through the years I have always wanted to create a single, optimized, and official package from the Xamarin team at Microsoft that could easily be consumed by any application. The time is now with [Xamarin.Essentials](https://docs.microsoft.com/xamarin/essentials/index?WT.mc_id=friends-0000-jamont), which offers over 50 cross-platform native APIs in a single optimized package. I worked on this new library with an amazing team of developers and I highly highly highly recommend you check it out. 

Additionally, Xamarin.Essentials is now included in & [.NET MAUI](https://learn.microsoft.com/dotnet/maui/platform-integration/appmodel/permissions?WT.mc_id=friends-0000-jamont).

Due to the functionality being included "in the box" I have decided to officially archive this repo.

### Setup
* Currently in Alpha (turn on pre-release packages)
* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.Contacts [![NuGet](https://img.shields.io/nuget/v/Xam.Plugin.Contacts.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugin.Contacts/)
* Install into your PCL project and Client projects.

**Platform Support**

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS|Yes|iOS 7+|
|Xamarin.iOS Unified|Yes|iOS 7+|
|Xamarin.Android|Yes|API 14+|
|Windows Phone Silverlight|Yes|8.0+|
|Windows Phone RT|No|8.1+|
|Windows Store RT|No|8.1+|
|Windows 10 UWP|Yes|10+|
|Xamarin.Mac|No||

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android
* Windows Phone 8 (Silverlight)
* Windows Phone 8.1 RT (Blank Implementation)
* Windows Store 8.1 (Blank Implementation)
* Windows 10 UWP

### API Usage Example
```csharp
if(await CrossContacts.Current.RequestPermission())
      {
     
        List<Contact> contacts = null;
        CrossContacts.Current.PreferContactAggregation = false;//recommended
//run in background
        await Task.Run(() =>
        {
          if(CrossContacts.Current.Contacts == null)
            return;

          contacts = CrossContacts.Current.Contacts
            .Where(c => !string.IsNullOrWhiteSpace(c.LastName) && c.Phones.Count > 0)         
            .ToList();

          contacts = contacts.OrderBy(c => c.LastName).ToList();
        });
      }
```



### Important

* **Android**
 
 The android.permissions.READ_CONTACTS permission is required, but the library will automatically add this for you. Additionally, if your users are running Marshmallow the Plugin will automatically prompt them for runtime permissions when RequestPermissions() is called.
 
 You must add the Permission Plugin code into your Main or Base Activities:

```csharp
public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
{
    PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
}
```

You MUST set your Target version to API 23+ and Compile against API 23+:
![image](https://cloud.githubusercontent.com/assets/1676321/17110560/7279341c-5252-11e6-89be-8c10b38c0ea6.png)

* **iOS**
 
 When compiling against iOS 10 SDK Your info.plist needs to specify key NSContactsUsageDescription and a value explaining why the application wants to access the contacts. Permissions will automatically be requested when RequestPermissions() is called.

* **Windows Phone**
 
 You must add ID_CAP_CONTACTS permission

* **UWP**
 
 You must mark Contacts in Capabilities tab at app manifest.

#### License
This is a derivative to [Xamarin.Mobile's Contacts](http://github.com/xamarin/xamarin.mobile) with a cross platform API and other enhancements.

```
//
//  Copyright 2011-2013, Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
```
