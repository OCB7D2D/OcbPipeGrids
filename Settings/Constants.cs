using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeManager
{

    public enum TYPES : ushort
    {
        PipeConnection = 1,
        PipePump = 2,
        PipeIrrigation = 3,
        PipeSouce = 4,
        PipeWaterBoiler = 5,
        PipeFluidConverter = 6,

        PipeReservoir = 7,
        PipeFluidInjector = 13,
        PlantationSprinkler = 14,
        PipeWell = 9,
        PlantationFarmLand = 8,
        PlantationFarmPlot = 10,
        PlantationGrowing = 11,

        PlantationComposter = 12,
    }

}
