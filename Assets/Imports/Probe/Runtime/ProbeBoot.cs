namespace Probe
{
    /// <summary>
    /// Decides whether Probe auto-creates its console and panel at startup.
    ///
    /// By default Probe runs ONLY in the Editor and Development builds, so it
    /// never ships in a release build by accident. Override with scripting
    /// define symbols (Project Settings ▸ Player ▸ Scripting Define Symbols):
    ///
    ///   PROBE_ENABLE   — force Probe on, even in non-development release builds
    ///   PROBE_DISABLE  — force Probe off everywhere (takes priority)
    /// </summary>
    static class ProbeBoot
    {
        public static bool AutoStartEnabled
        {
            get
            {
#if PROBE_DISABLE
                return false;
#elif UNITY_EDITOR || DEVELOPMENT_BUILD || PROBE_ENABLE
                return true;
#else
                return false;
#endif
            }
        }
    }
}
