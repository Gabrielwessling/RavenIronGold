public static class DebugSettingsHelper
{
    public static int FontSize
    {
        get
        {
            return SettingsManager.Instance.Settings.debug.debugScale switch
            {
                0 => 14,
                1 => 18,
                2 => 24,
                3 => 32,
                _ => 18
            };
        }
    }
}