using System.Globalization;

namespace Chimp.Common;

public static class LanguageHelper
{
    public static void SetUiLanguage(string lang)
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentUICulture;
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentUICulture;
    }
}
