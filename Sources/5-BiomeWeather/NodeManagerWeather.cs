using HarmonyLib;

namespace NodeFacilitator
{

    public struct BiomeWeather
    {
        public int BiomeID;
        public float ParticleRain;
        public float ParticleSnow;
        public float SurfaceWet;
        public float SurfaceSnow;
    }

    // Hook into server to client synchronization
    // Also pass the info down into the worker thread
    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch("SendPackages")]
    public static class WeatherManagerHook
    {
        static void Postfix(WeatherPackage[] ___weatherPackages)
        {
            // That only must happen on server
            var action = new ActionUpdateWeather();
            action.Setup(___weatherPackages);
            NodeManagerInterface.Instance.ToWorker.Add(action);
        }
    }

}
