using HarmonyLib;

namespace NodeFacilitator
{
    public class BiomeMapping
    {

        private GridCompressedData<byte> BiomeMap = null;
        private int BiomeMapWidth = 0;
        private int BiomesScaleDiv = 0;
        private int BiomesMapWidthHalf = 0;
        private int BiomesMapHeightHalf = 0;

        static readonly HarmonyFieldProxy<GridCompressedData<byte>> FieldBiomeMap = new HarmonyFieldProxy
            <GridCompressedData<byte>>(typeof(WorldBiomeProviderFromImage), "m_BiomeMap");
        static readonly HarmonyFieldProxy<int> FieldBiomeMapWidth = new HarmonyFieldProxy
            <int>(typeof(WorldBiomeProviderFromImage), "biomeMapWidth");
        static readonly HarmonyFieldProxy<int> FieldBiomesScaleDiv = new HarmonyFieldProxy
            <int>(typeof(WorldBiomeProviderFromImage), "biomesScaleDiv");
        static readonly HarmonyFieldProxy<int> FieldBiomesMapWidthHalf = new HarmonyFieldProxy
            <int>(typeof(WorldBiomeProviderFromImage), "biomesMapWidthHalf");
        static readonly HarmonyFieldProxy<int> FieldBiomesMapHeightHalf = new HarmonyFieldProxy
            <int>(typeof(WorldBiomeProviderFromImage), "biomesMapHeightHalf");

        public BiomeMapping(IBiomeProvider provider)
        {
            if (provider is WorldBiomeProviderFromImage map)
            {
                BiomeMap = FieldBiomeMap.Get(map);
                BiomeMapWidth = FieldBiomeMapWidth.Get(map);
                BiomesScaleDiv = FieldBiomesScaleDiv.Get(map);
                BiomesMapWidthHalf = FieldBiomesMapWidthHalf.Get(map);
                BiomesMapHeightHalf = FieldBiomesMapHeightHalf.Get(map);
            }
            else
            {
                BiomeMap = null;
                BiomeMapWidth = 0;
                BiomesScaleDiv = 1;
                BiomesMapWidthHalf = 0;
                BiomesMapHeightHalf = 0;
                Log.Error("Unsupported Biome Provider");
            }
        }

        public byte GetBiomeID(Vector3i pos)
            => GetBiomeID(pos.x, pos.z);

        public byte GetBiomeID(int x, int z)
        {
            if (BiomeMap == null) return byte.MaxValue;
            int _x = x / BiomesScaleDiv + BiomesMapWidthHalf;
            if (_x < 0 || _x >= BiomeMap.width) return byte.MaxValue;
            int _y = z / BiomesScaleDiv + BiomesMapHeightHalf;
            if (_y < 0 || _y >= BiomeMap.height) return byte.MaxValue;
            return BiomeMap.GetValue(_x, _y);
        }

        [HarmonyPatch(typeof(World))]
        [HarmonyPatch("SetupSleeperVolumes")]
        public static class LoadWorld
        {
            static void Postfix(World __instance)
            {
                NodeManagerInterface.Instance.BiomeMap = new BiomeMapping(
                    __instance.ChunkCache.ChunkProvider.GetBiomeProvider());
            }
        }
    }
}
