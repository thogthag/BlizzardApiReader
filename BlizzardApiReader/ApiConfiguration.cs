using BlizzardApiReader.Core.Enums;
using BlizzardApiReader.Core.Extensions;
using System;

namespace BlizzardApiReader
{
    public class ApiConfiguration
    {
        public Region ApiRegion;
        public Locale ResultLocale;
        public string ApiKey;

        /// <summary>
        /// Initialize ApiConfiguration with default configurations and no api key
        /// </summary>
        public ApiConfiguration() : this(null)
        {

        }

        /// <summary>
        /// Initialize ApiConfiguration with default configurations and api key
        /// </summary>
        /// <param name="apiKey"></param>
        public ApiConfiguration(string apiKey) : this(Region.US, Locale.en_US, apiKey)
        {

        }

        /// <summary>
        /// Initialize ApiConfiguration
        /// </summary>
        /// <param name="region"></param>
        /// <param name="locale"></param>
        /// <param name="apiKey"></param>
        public ApiConfiguration(Region region, Locale locale, string apiKey)
        {
            ApiRegion = region;
            ResultLocale = locale;
            ApiKey = apiKey;
        }

        public static ApiConfiguration CreateEmpty()
        {
            return new ApiConfiguration();
        }

        public ApiConfiguration SetApiKey(string key)
        {
            ApiKey = key;
            return this;
        }

        /// <summary>
        /// Set the region of the ApiConfiguration
        /// </summary>
        /// <param name="region">The region to set</param>
        /// <returns>This instance of ApiConfiguration</returns>
        public ApiConfiguration SetRegion(Region region)
        {
            return SetRegion(region, false);
        }

        /// <summary>
        /// Set the region of the ApiConfiguration with locale set to default locale of region if bool is set to true
        /// </summary>
        /// <param name="region">The region to set</param>
        /// <param name="useDefaultLocale">Determines whether locale should be set based on default locale of region</param>
        /// <returns>This instance of ApiConfiguration</returns>
        public ApiConfiguration SetRegion(Region region, bool useDefaultLocale)
        {
            ApiRegion = region;
            if (useDefaultLocale)
            {
                ResultLocale = region.GetDefaultLocale();
            }
            return this;
        }

        public ApiConfiguration SetLocale(Locale locale)
        {
            ResultLocale = locale;
            return this;
        }

        /// <summary>
        /// Will use the default locale for the configuration region, must be called only after setting the Region
        /// </summary>
        /// <returns></returns>
        public ApiConfiguration UseDefaultLocale()
        {
            ResultLocale = ApiRegion.GetDefaultLocale();
            return this;
        }


        /// <summary>
        /// Declare this Configuration as the global default configuration, it will be used when no configuration is provided to the api reader.
        /// </summary>  
        public ApiConfiguration DeclareAsDefault()
        {
            ApiReader.SetDefaultConfiguration(this);
            return this;
        }

        public string GetLocaleString()
        {
            return ResultLocale.GetEnumName();
        }
        public string GetRegionString()
        {
            return ApiRegion.GetEnumName();
        }
    }

    public static class RegionExtensions
    {
        public static Locale GetDefaultLocale(this Region region)
        {
            switch (region)
            {
                case Region.EU:
                case Region.Europe:
                    return Locale.BritishEnglish;
                case Region.KR:
                case Region.Korea:
                    return Locale.Korean;
                case Region.TW:
                case Region.Taiwan:
                    return Locale.TraditionalChinese;
                case Region.SEA:
                case Region.SoutheastAsia:
                case Region.US:
                case Region.UnitedStates:
                    return Locale.AmericanEnglish;
                default:
                    throw new NotImplementedException($"The {nameof(Region)} [{region.ToString()}] does not have an associated {nameof(Locale)}");
            }
        }
    }
}
