namespace NodeFacilitator
{
    public class ActionUpdateLightLevels : IActionWorker
    {

        public int SenderEntityId { get => -1; set => throw new
            System.Exception("Can send stop signal from clients!"); }

        public Vector3i Position;
        public byte SunLight;
        public byte Fertility;
        // public float Temp;
        // public float Clouds;

        public void Setup(WorldBase world, int clrIdx, Vector3i pos)
        {
            Position = pos;
            Fertility = (byte)world
                .GetBlock(pos + Vector3i.down)
                .Block.blockMaterial.FertileLevel;
            SunLight = (byte)world.GetBlockLightValue(clrIdx, pos);
            // WeatherManager weather = WeatherManager.Instance;
            // Temp = weather.GetCurrentTemperatureValue();
            // Clouds = weather.GetCurrentCloudThicknessPercent();
        }

        public void ProcessOnWorker(NodeManagerWorker worker)
            => worker.UpdatePlantStats(this);

    }
}
