namespace Helpfulcore.Localization
{
    public interface ILocalizationService
    {
        string Localize(string key, string defaultValue = null, bool editable = false, string language = null, bool autoCreate = true);
    }
}
