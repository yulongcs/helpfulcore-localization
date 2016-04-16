### Helpfulcore - helpful features for Sitecore
# Helpfulcore Localization
Abstracted localization service for Sitecore solutions which uses default dictionary items for translations. Automatic dictionary item creation feature support based on dictionary dot separated key format.
In order to use the module, please install this nuget package to your Sitecore website project:
```
Install-Package Helpfulcore.Localization.Web
```
Once you installed the package, you will have an extention method for the SitecoreHelper class with name "Localize" and next signature:
```cs
public static IHtmlString Localize(this SitecoreHelper sitecoreHelper, string key, string defaultValue = null, bool editable = false, bool autoCreate = true)
```

The package installs the dedicated Sitecore include config file with configuration for module initialization and the LocalizationService instance to /App_Config/Include/Helpfulcore/Helpfulcore.Localization.config.
```xml
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
  <sitecore>
    <helpfulcore>
      <localization>
        <localizationService type="Helpfulcore.Localization.LocalizationService, Helpfulcore.Localization" singleInstance="true">
          <AutoCreateDictionaryItems>true</AutoCreateDictionaryItems>
          <UseDotSeparatedKeyNotaion>true</UseDotSeparatedKeyNotaion>
        </localizationService>
      </localization>
    </helpfulcore>
    <pipelines>
      <initialize>
        <processor type="Helpfulcore.Localization.Pipelines.Initialize.InitializeLocalizationService, Helpfulcore.Localization" />
      </initialize>
    </pipelines>
  </sitecore>
</configuration>
```
This means that the extension method above just wraps the call to abstracted dedicated service 
```cs
namespace Helpfulcore.Localization
{
    public interface ILocalizationService
    {
        string Localize(string key, string defaultValue = null, bool editable = false, bool autoCreate = true);
    }
}
```
You can still resolve this service directly from Sitecore configuration factory
```cs
var localizationService = Factory.CreateObject("helpfulcore/localization/localizationService", true) as ILocalizationService;
```
or use LocalizationFactory which is being initialized in Sitecore "initialize" pipeline
```cs
var localizationService = LocalizationFactory.LocalizationService;
```
or register it in your IoC container like
```cs
container.Register(typeof(ILocalizationService), () => LocalizationFactory.LocalizationService, Lifestyle.Singleton);
```
The module expects you to use _dot.separated.key.format_ for your dictionary keys whish are going to be converted into relative Dictionary item paths in your configured Dictionary domain.
There is Auto-Create feature enabled by default as well so using this module, you, as a developer do not really care about creation dictionary translations during the development. They will be created automatically for you with (or without) default values.
Optional "editing" feature is also supported.
So in result you can simply write in your .cshtml rendering next statements:
```cs
@Html.Sitecore().Localize("header.socialNetworks.facebook", "Facebook")
@Html.Sitecore().Localize("header.socialNetworks.twitter", "Twitter", editable:true)
@Html.Sitecore().Localize("header.socialNetworks.linkedIn")
@Html.Sitecore().Localize("header.socialNetworks.instagramm", editable:true)
```
On first page load, relevant dictionary items will be created in your configured Dictionary Domain for the website (if no domain configured, the '/sitecore/system/Dictionary' will be used)
```
*[DictionaryDomainItem]/Header/SocialNetworks/Facebook - with default value "Facebook" in language the page were opened
*[DictionaryDomainItem]/Header/SocialNetworks/Twitter - with default value "Twitter" in language the page were opened. In editing mode this phrase will be editable.
*[DictionaryDomainItem]/Header/SocialNetworks/LinkedIn - with empty value in language the page were opened. 
*[DictionaryDomainItem]/Header/SocialNetworks/Instagramm - with empty value in language the page were opened. In editing mode this phrase will be editable.
```
All phrases will be automatically published to all available publishing targets.

## Requirements

* Sitecore CMS 6+ is supported
* on Sitecore site definitions _content_ database must be configured 
* use _dot.separated.key.format_ for dictionary keys with _camelNotation_
