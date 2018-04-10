using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace DewCore.AspNetCore.Middlewares
{
    /// <summary>
    /// Service interface
    /// </summary>
    public interface IDewTranslator
    {
        /// <summary>
        /// Return the key if string is not present
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetString(string key);
        /// <summary>
        /// return def if string is not present
        /// </summary>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        string GetString(string key, string def);
        /// <summary>
        /// return the raw dictionary
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetInternalDictionary();
        /// <summary>
        /// read dictionary
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task GetDictionaryFromFiles(HttpContext context);
    }
    /// <summary>
    /// Translator helper for middleware
    /// </summary>
    public class DewTranslatorService : IDewTranslator
    {
        private Dictionary<string, string> _dictionary;
        private DewLocalizationMiddlewareOptions _options = null;
        private IHostingEnvironment _env = null;
        /// <summary>
        /// Search a string into dictionary, if not found return the key (useful if you use the value as key in your default language)
        /// </summary>
        /// <param name="key">Key value</param>
        /// <returns></returns>
        public string GetString(string key)
        {
            var s = _dictionary.FirstOrDefault(x => x.Key == key);
            if (s.Equals(default(KeyValuePair<string, string>)))
            {
                return key;
            }
            return s.Value;
        }
        /// <summary>
        /// Search a string into dictionary, if not found return the default
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="def">Default value</param>
        /// <returns></returns>
        public string GetString(string key, string def)
        {
            var s = _dictionary.FirstOrDefault(x => x.Key == key);
            if (s.Equals(default(KeyValuePair<string, string>)))
            {
                return def;
            }
            return s.Value;
        }
        /// <summary>
        /// Return the key value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public string this[string key]
        {
            get { return _dictionary.First(x => x.Key == key).Value; }
        }
        /// <summary>
        /// Return the internal string dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetInternalDictionary()
        {
            return _dictionary;
        }
        /// <summary>
        /// Read the dictionary from the file
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task GetDictionaryFromFiles(HttpContext context)
        {
            var language = context.Request.Cookies.FirstOrDefault(x => x.Key == _options.Cookie);
            string currLanguage = _options.Language;
            if (!language.Equals(default(KeyValuePair<string, string>)))
                currLanguage = language.Value;
            string localizationJson = null;
            try
            {
                using (Stream file = _env.ContentRootFileProvider.GetFileInfo(_options.Path + "/" + currLanguage + ".json").CreateReadStream())
                {
                    using (StreamReader streamReader = new StreamReader(file))
                    {
                        localizationJson = await streamReader.ReadToEndAsync();
                    }
                }
            }
            catch
            {//if something goes wrong with the file we load the default
                using (Stream file = _env.ContentRootFileProvider.GetFileInfo(_options.Path + "/" + _options.Language + ".json").CreateReadStream())
                {
                    using (StreamReader streamReader = new StreamReader(file))
                    {
                        localizationJson = await streamReader.ReadToEndAsync();
                    }
                }
            }
            _dictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(localizationJson);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="env"></param>
        public DewTranslatorService(DewLocalizationMiddlewareOptions options = null, IHostingEnvironment env = null)
        {
            _options = options;
            _env = env;
        }
    }
    /// <summary>
    /// Translator helper for middleware
    /// </summary>
    public class DewTranslator : IDewTranslator
    {
        private Dictionary<string, string> _dictionary;
        /// <summary>
        /// Search a string into dictionary, if not found return the key (useful if you use the value as key in your default language)
        /// </summary>
        /// <param name="key">Key value</param>
        /// <returns></returns>
        public string GetString(string key)
        {
            var s = _dictionary.FirstOrDefault(x => x.Key == key);
            if (s.Equals(default(KeyValuePair<string, string>)))
            {
                return key;
            }
            return s.Value;
        }
        /// <summary>
        /// Search a string into dictionary, if not found return the default
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="def">Default value</param>
        /// <returns></returns>
        public string GetString(string key, string def)
        {
            var s = _dictionary.FirstOrDefault(x => x.Key == key);
            if (s.Equals(default(KeyValuePair<string, string>)))
            {
                return def;
            }
            return s.Value;
        }
        /// <summary>
        /// Return the key value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public string this[string key]
        {
            get => _dictionary.ContainsKey(key) ? _dictionary[key] : key;
        }
        /// <summary>
        /// Return the internal string dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetInternalDictionary()
        {
            return _dictionary;
        }
        /// <summary>
        /// Read dictionary again (disabled on DewTranslator
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task GetDictionaryFromFiles(HttpContext context)
        {
            return null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dictionary"></param>
        public DewTranslator(Dictionary<string, string> dictionary = null)
        {
            _dictionary = dictionary;
        }
    }
    /// <summary>
    /// Dew localization middleware class
    /// </summary>
    public class DewLocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DewLocalizationMiddlewareOptions _options;
        private Dictionary<string, string> _dictionary = null;
        private async Task GetDictionaryFromFiles(HttpContext context, IHostingEnvironment env)
        {
            var language = context.Request.Cookies.FirstOrDefault(x => x.Key == _options.Cookie);
            string currLanguage = _options.Language;
            if (!language.Equals(default(KeyValuePair<string, string>)))
                currLanguage = language.Value;
            string localizationJson = null;
            try
            {
                using (Stream file = env.ContentRootFileProvider.GetFileInfo(_options.Path + "/" + currLanguage + ".json").CreateReadStream())
                {
                    using (StreamReader streamReader = new StreamReader(file))
                    {
                        localizationJson = await streamReader.ReadToEndAsync();
                    }
                }
            }
            catch
            {//if something goes wrong with the file we load the default
                using (Stream file = env.ContentRootFileProvider.GetFileInfo(_options.Path + "/" + _options.Language + ".json").CreateReadStream())
                {
                    using (StreamReader streamReader = new StreamReader(file))
                    {
                        localizationJson = await streamReader.ReadToEndAsync();
                    }
                }
            }
            _dictionary = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(localizationJson);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Next middleware</param>
        /// <param name="options">Localization options</param>
        public DewLocalizationMiddleware(RequestDelegate next, DewLocalizationMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }
        /// <summary>
        /// Invoke method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IHostingEnvironment env)
        {
            GetDictionaryFromFiles(context, env).Wait();
            context.Items.Add(_options.CustomName, _dictionary);
            await _next(context);
        }
    }
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
    /// <summary>
    /// HTTPContext DewLocalization Extension class
    /// </summary>
    public static class DewLocalizationHttpContextExtension
    {
        /// <summary>
        /// Returns the DewTranslator object
        /// </summary>
        /// <param name="context"></param>
        /// <param name="customName"></param>
        /// <returns></returns>
        public static DewTranslator GetDewLocalizationTranslator(this HttpContext context, string customName = null)
        {
            var name = customName ?? "DewLocalization";
            var data = context.Items.FirstOrDefault(x => x.Key as string == name);
            return data.Equals(default(KeyValuePair<object, object>)) ? null : new DewTranslator(data.Value as Dictionary<string, string>);
        }
    }
    /// <summary>
    /// DewLocalization pipeline builder extension
    /// </summary>
    public static class DewLocalizationBuilderExtension
    {
        /// <summary>
        /// Builder method
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseDewLocalizationMiddleware(
            this IApplicationBuilder builder, DewLocalizationMiddlewareOptions options = null)
        {
            if (options == null)
                options = new DewLocalizationMiddlewareOptions();
            return builder.UseMiddleware<DewLocalizationMiddleware>(options);
        }
    }
}
