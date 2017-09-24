# DewLocalizationMiddleware
If you need a way to set your ASP NET CORE website/api service with custom localizations, this middleware can help you.

# NOTE
You can use the version 1.0 for .net core (asp net core 1.x) or the version 2.0 for .net core 2.0 (asp net core 2)

# How to install

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

__CustomName__ -  _WORKS ONLY IN V. 2.1.0_

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
__NOTE :__ If no file present, ever the default file, the middleware throw an exception
Remember to add the:
```c#
using DewCore.AspNetCore.Middlewares;
```
to use the UseDewLocalizationMiddleware method
## Use Translator like a service

If you don't want use the localization middleware as middleware in the pipeline, you can use it like a service.
This is how you can do it.

```c#
public void ConfigureServices(IServiceCollection services)
{
    //if you want use it with custom options
    services.AddScoped<IDewTranslator, DewTranslatorService>((sp) =>
    {
        var service = sp.GetRequiredService<IHostingEnvironment>();
        return new DewTranslatorService(new DewLocalizationMiddlewareOptions() { Language = "it-it" }, service);
    });
    //if you want use it with default options
    //->services.AddScoped<IDewTranslator,DewTranslatorService>();


    services.AddMvc();
}
```

In this case, you should use the dependency injection to get the translator:
```c#
public class HomeController : Controller
{
    IDewTranslator _translator = null;
    public HomeController(IDewTranslator translator)
    {
        _translator = translator;
    }
}
```
you can see in the next example how to work with it in both ways.


__NOTE:__ if you set the translator as a service you cannot read the dictionary from HttpContext, and same is the inverse (if you use it like middleware it will not work with D-Injection)

__NOTE:__ You can set both way (but why?)


## Work with Translator

Middleware expose an Helper class, the DewTranslator.

If you want use it, you need to remember to add this into _using_ section:

```c#
using DewCore.AspNetCore.Middlewares;
```

After that, in the controller.

```c#
//a method into a controller
public async Task<IActionResult> About()
{
    //if no item is setted with Default name or custom name, this method returns null
    var translator = HttpContext.GetDewLocalizationTranslator();
    // if you use the DewTranslator as a Service you should do
    await _translator.GetDictionaryFromFiles(HttpContext);
    ViewData["Message"] = translator.GetString("Your application description page."); //return the key as text if not found value
    //or _translator.GetString(...)_
    ViewData["Message"] = translator.GetString("Your application description page.", "Text not found!"); //return the second argument if key value not found
    //or
    ViewData["Message"] = translator["Your application description page."]; //however square notation throw an exception if key not exists
    return View();
}
```

## Differences between service and middleware

The main difference is that in the middleware way the dictionary will be read automatically in a point of request pipeline, and you after this can use it just calling _HttpContext.GetDewLocalizationTranslator()_.

In the service way, you just inject the object with its options to controller, but before you can use it, you need to read the dictionary by calling method _GetDictionaryFromFiles(HttpContext)_

After that, the use is the same.

## Note 
## NuGet
You can find it on nuget with the name [DewLocalizationMiddleware](https://www.nuget.org/packages/DewLocalizationMiddleware/)

## About
[Andrea Vincenzo Abbondanza](http://www.andrewdev.eu)

## Donate
[Help me to grow up, if you want](https://payPal.me/andreabbondanza)
