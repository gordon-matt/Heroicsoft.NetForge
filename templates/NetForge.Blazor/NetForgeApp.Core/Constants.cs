namespace NetForgeApp;

public static class Constants
{
    /// <summary>Browser <c>localStorage</c> key for the light/dark theme.</summary>
    public const string ClientThemeLocalStorageKey = "netforgeapp.theme";

    public static class MaxLengths
    {
        public const int Title = 256;
        public const int UserId = 450;
    }

    public static class Schemas
    {
        public const string App = "app";
    }

    /// <summary>
    /// Shown on the Identity pages, which render before a Blazor circuit exists.
    /// </summary>
    public static class ServerSettingDefaults
    {
        public const string SiteName = "NetForgeApp";
    }
}
