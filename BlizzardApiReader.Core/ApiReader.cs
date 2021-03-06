﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BlizzardApiReader.Core.Exceptions;

namespace BlizzardApiReader.Core
{
    public class ApiReader
    {
        private const string url = "https://REGION.api.blizzard.com";
        /// <summary>
        /// Default configuration will be used if api reader does not have a local instance of ApiConfiguration
        /// </summary>
        private static ApiConfiguration defaultConfig { get; set; }
        private static LimitersList limiters { get; } = new LimitersList();


        public ApiConfiguration Configuration;

        private IWebClient _webClient;
        private string _token;
        private DateTime _tokenExpiration;

        public ApiReader(ApiConfiguration apiConfiguration = null, IWebClient webClient = null)
        {
            Configuration = apiConfiguration;
            if (webClient == null)
                _webClient = new ApiWebClient();
            else
                _webClient = webClient;
        }

        public static void SetDefaultConfiguration(ApiConfiguration configuration)
        {
            defaultConfig = configuration;
        }

        public static void ClearDefaultConfiguration()
        {
            defaultConfig = null;
        }

        public async Task<T> GetAsync<T>(string query)
        {
            throwIfInvalidRequest();

            if (tokenExpired())
            {
                await SendTokenRequest();
            }

            string urlRequest = parseUrl(query);
            IApiResponse response = await _webClient.MakeHttpRequestAsync(urlRequest);
            limiters.NotifyAll(this, response);

            if (response.IsSuccessful())
            {
                string json = await response.ReadContentAsync();
                return JsonConvert.DeserializeObject<T>(json);
            }
            else
            {
                throw new BadResponseException("Response is not successful", response);
            }

        }

        /// <summary>
        /// Sends token request and sets it as the current token.
        /// </summary>
        /// <returns></returns>
        public async Task<string> SendTokenRequest()
        {
            var response = await _webClient.RequestAccessTokenAsync(getConfiguration());
            if (response.IsSuccessful())
            {
                string json = await response.ReadContentAsync();
                JObject jObject = JObject.Parse(json);
                _token = (string)jObject["access_token"];
                int expiresInSeconds = (int)jObject["expires_in"];
                _tokenExpiration = DateTime.Now.AddSeconds(expiresInSeconds);
                return _token;
            }
            //TODO: Add better error handling
            throw new HttpRequestException("response code was not successful");
        }


        private void throwIfInvalidRequest()
        {

            verifyConfigurationIsValid();

            if (limiters.AnyReachedLimit())
                throw new RateLimitReachedException("http request was blocked by RateLimiter");
        }
        private bool tokenExpired()
        {
            if (String.IsNullOrEmpty(_token)
                || DateTime.Now > _tokenExpiration)
            {
                return true;
            }

            return false;
        }


        private void verifyConfigurationIsValid()
        {
            if (getConfiguration() == null)
                throw new NullReferenceException("ApiConfiguration is not set, either declare one as global configuration or set a local instance configuration object.");
        }


        private string parseUrl(string query)
        {
            query = parseSpecialCharacters(query);

            string region = getConfiguration().GetRegionString();
            string newUrl = url.Replace("REGION", region.ToLower());
            newUrl += query + "?locale=" + getConfiguration().GetLocaleString() + "&access_token=" + _token;
            return newUrl;
        }



        private string parseSpecialCharacters(string s)
        {
            s = s.Replace("#", "%23");
            return s;
        }
        private ApiConfiguration getConfiguration()
        {
            if (Configuration == null)
                return defaultConfig;
            else
                return Configuration;
        }
    }
}
