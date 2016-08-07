using System.Web;
using Sitecore.Mvc.Helpers;

namespace Helpfulcore.Localization.Extensions
{
    public static class LocalizationExtensions
    {
        public static IHtmlString Localize(
			this SitecoreHelper sitecoreHelper,
			string key, 
			string defaultValue = null,
			bool editable = false, 
			string language = null, 
			bool autoCreate = true)
        {
            return new HtmlString(LocalizationFactory.LocalizationService.Localize(key, defaultValue, editable, language, autoCreate));
        }
    }
}