namespace NodeFacilitator
{
    // Extend NodeManager (worker thread) to hold biome weather
    // Also adding a few helper methods to query weather savely
    public partial class NodeManagerInterface
    {
        //########################################################
        //########################################################

        public BiomeMapping BiomeMap { get; set; }

        // Will be create when weather is synced
        public WeatherPackage[] BiomeWeathers = null;

        // Various helper to query weather at worker
        // Weather is only local to biomes in 7D2D
        public float GetBiomeParticleRain(int biome)
            => BiomeWeathers == null ? 0 :
               BiomeWeathers.Length <= biome ? 0 :
               BiomeWeathers[biome].particleRain;
        public float GetBiomeParticleSnow(int biome)
            => BiomeWeathers == null ? 0 :
               BiomeWeathers.Length <= biome ? 0 :
               BiomeWeathers[biome].particleSnow;
        public float GetBiomeSurfaceSnow(int biome)
            => BiomeWeathers == null ? 0 :
               BiomeWeathers.Length <= biome ? 0 :
               BiomeWeathers[biome].surfaceSnow;
        public float GetBiomeSurfaceWet(int biome)
            => BiomeWeathers == null ? 0 :
               BiomeWeathers.Length <= biome ? 0 :
               BiomeWeathers[biome].surfaceWet;

    }

}
