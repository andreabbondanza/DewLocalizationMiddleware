# DewLocalizationMiddleware
If you need a way to set your ASP NET CORE website/api service with custom localizations, this middleware can help you.

# NOTE
You can use the version 1.0 for .net core (asp net core 1.x) or the version 2.0 for .net core 2.0 (asp net core 2)

# How it install

Simply go into NuGet package manager and search for DewLocalizationMiddleware or use:
```Console
Install-Package DewLocalizationMiddleware -Version 1.0.0
```
or
```Console
Install-Package DewLocalizationMiddleware -Version 2.1.1
```
from package manager console.

# How it works

Now I'll show you how middleware works

## Defaults

The middleware has the __DewLocalizationMiddlewareOptions__ type. If you don't want customize anything you can just call the middleware with his Usexxx method.
However you can customize a bit the middleware with the options.
Here the type with default values
```c#
/// <summary>
/// DewLocalization options class
/// </summary>
public class DewLocalizationMiddlewareOptions
{
    /// <summary>
    /// Default language
    /// </summary>
    public string Language = "en-us";
    /// <summary>
    /// Default localization files path (not should start with "/" and should end with "/")
    /// </summary>
    public string Path = "Localization";
    /// <summary>
    /// Default cookie language name
    /// </summary>
    public string Cookie = "lang";
    /// <summary>
    /// Httpcontext item name
    /// </summary>
    public string CustomName = "DewLocalization";
}
```
#### Options

__Language__ 

By default the middleware assume the language "en-us". Every language file must by a "\<lang>.json" where \<lang> is a choosed string from the cookie or the default language.

In default case the file is called "en-us.json", obviously you can change the default value. 

For example you can leave "en-us" like default value, and call the other files "fr-fr","it-it","it-ch", etc. and use this notation in the cookie.

__NOTE :__ default language works only when the call to the first file (for example "it-it") fails.

__Path__ 

By default the middleware assume that the localization folder is in the main project folder and it's called "Localization".

You can change it, however be carefull that you don't need to place the "/" at the end of the new _Path_ string.

For example if you want place it into a directory called "Language/Dictionaries/it-it.json" __Path__ must be "Language/Dictionaries".

__Cookie__

This is just the language Cookie name. Call it how you want. Default value is "lang".

__CustomName__ -  _WORKS ONLY IN V. 2.1.0__

If you need a custom name for HttpContext item you can set it here, just remember to pass to __GetDewLocalizationTranslator__ (HttpContext extension class method).

Default is "DewLocalization"

## How add middleware to pipeline

```c#
//default
app.UseDewLocalizationMiddleware();
//or with options
app.UseDewLocalizationMiddleware(new DewLocalizationMiddlewareOptions()
{
    Cookie = "LanguageCookie",
    Path = "MyNewLocalizationPath"
});
```

Remember to add the:
```c#
using DewCore.AspNetCore.Middlewares;
```
to use the UseDewLocalizationMiddleware method

## Work with Translator

Middleware expose an Helper class, the DewTranslator.

If you want use it, you need to remember to add this into _using_ section:

```c#
using DewCore.AspNetCore.Middlewares;
```

After that, in the controller.

```c#
//a method into a controller
public IActionResult About()
{
    //if no item is setted with Default name or custom name, this method returns null
    var translator = HttpContext.GetDewLocalizationTranslator();
    ViewData["Message"] = translator.GetString("Your application description page."); //return the key as text if not found value
    //or
    ViewData["Message"] = translator.GetString("Your application description page.", "Text not found!"); //return the second argument if key value not found
    //or
    ViewData["Message"] = translator["Your application description page."]; //however square notation throw an exception if key not exists
    return View();
}
```
## Note 
## NuGet
You can find it on nuget with the name [DewLocalizationMiddleware](https://www.nuget.org/packages/DewLocalizationMiddleware/)

## About
[Andrea Vincenzo Abbondanza](http://www.andrewdev.eu)

## Donate
[Help me to grow up, if you want](https://payPal.me/andreabbondanza)