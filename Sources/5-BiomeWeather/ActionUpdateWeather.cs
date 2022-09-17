namespace NodeFacilitator
{
    public class ActionUpdateWeather : IActionWorker
    {

        WeatherPackage[] Weather { get; set; }

        public int SenderEntityId { get => -1; set => throw new System
            .Exception("Can't send weather data from remote clients!"); }

        public void Setup(WeatherPackage[] biomes)
        {
            if (biomes == null) return;
            // Create new instance to pass to thread
            // Once sent it is "owned" by worker thread
            Weather = new WeatherPackage[255];
            for (int i = 0; i < biomes.Length; i++)
            {
                byte id = biomes[i].biomeID;
                Weather[id] = new WeatherPackage();
                Weather[id].CopyFrom(biomes[i]);
            }
        }

        public void ProcessOnWorker(NodeManagerWorker worker)
        {
            var Manager = NodeManagerInterface.Instance;
            Manager.BiomeWeathers = Weather;
            Weather = null; // Consumed
        }

    }
}
