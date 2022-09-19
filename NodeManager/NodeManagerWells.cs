using KdTree3;

namespace NodeManager
{
    public partial class NodeManager
        : GlobalTicker, IPersistable
    {

        public readonly KdTree<MetricChebyshev>.Vector3i<PipeWell> Wells =
            new KdTree<MetricChebyshev>.Vector3i<PipeWell>(AddDuplicateBehavior.Update);

        public PipeWell AddWell(PipeWell well)
        {
            Log.Warning("========= ADDWELL");
            Wells.Add(well.WorldPos, well);
            // Search for output to fill up the well
            var results = Irrigators.RadialSearch(
                well.WorldPos, 20 /*, NbIrrigatorCache*/);
            for (int i = 0; i < results.Length; i += 1)
            {
                well.AddIrrigation(results[i].Item2);
            }
            //foreach (var output in Find<PipeGridOutput>(
            //    well.WorldPos, OutputArea, OutputHeight))
            //{
            //    // Add cross-references
            //    output.AddWell(well);
            //    well.AddOutput(output);
            //}
            //
            //// PlantManager.OnWellAdded(well);
            //
            //// Search for plants that could use us
            //foreach (var plant in Find<PlantGrowing>(
            //    well.WorldPos, well.SearchArea, well.SearchHeight))
            //{
            //    // Add reference
            //    plant.AddWell(well);
            //}
            //
            //// Register positions in our dictionary
            //if (false && well.GetBlock() is Block block && block.isMultiBlock)
            //{
            //    int rotation = well.Rotation;
            //    int length = block.multiBlockPos.Length;
            //    for (int _idx = 0; _idx < length; ++_idx)
            //    {
            //        var pos = block.multiBlockPos.Get(
            //            _idx, block.blockID, rotation);
            //        Wells[pos + well.WorldPos] = well;
            //    }
            //}
            //else
            //{
            //    // Block has only one position
            //    Wells[well.WorldPos] = well;
            //}
            //
            return well;
        }

        public bool RemoveWell(Vector3i position)
        {
            if (Wells.TryFindValueAt(position,
                out PipeWell well))
            {
                // Search for output to fill up the well
                var results = Irrigators.RadialSearch(
                    position, 20 /*, NbIrrigatorCache */);
                //for (int i = 0; i < results.Length; i += 1)
                //{
                //    well.RemoveIrrigation(results[i].Value);
                //}
                Wells.RemoveAt(position);
                return true;
            }

            //if (Wells.TryGetValue(position,
            //    out PipeGridWell well))
            //{
            //    foreach (var output in Find<PipeGridOutput>(
            //        position, OutputArea, OutputHeight))
            //    {
            //        output.RemoveWell(well);
            //        well.RemoveOutput(output);
            //    }
            //    // Search for plants that could use us
            //    foreach (var plant in Find<PlantGrowing>(
            //        well.WorldPos, OutputArea, OutputHeight))
            //    {
            //        // Add reference
            //        plant.RemoveWell(well);
            //    }
            //    Wells.Remove(position);
            //    return true;
            //}
            return false;
        }

    }
}
