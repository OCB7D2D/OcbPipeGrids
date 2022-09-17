using System.Collections.Generic;
using UnityEngine;

namespace NodeFacilitator
{

    // Instance available on every client
    // Server might be remote or local
    public partial class NodeManagerClient
    {

        public Dictionary<Vector3i, PlantProgress> PlantStates
            = new Dictionary<Vector3i, PlantProgress>();

        public void SetPlantState(Vector3i pos, IPlantProgress state)
        {
            if (PlantStates.TryGetValue(pos,
                out PlantProgress prev))
            {
                if (prev.BID != state.BID)
                {
                    Log.Error("Trying to set old state {0} vs {1}",
                        prev.BID, state.BID);
                    return;
                }
            }
            // Since value of dict is a struct, we must reassign
            // We can't seem to get a true reference from dict!?
            PlantStates[pos] = new PlantProgress(state);
            // Mark chunk for regeneration to re-draw growth
            World world = GameManager.Instance.World;

            if (world?.GetChunkFromWorldPos(pos) is Chunk chunk)
            {
                if (chunk.GetBlockEntity(pos) is BlockEntityData data)
                {
                    if (data.blockValue.Block is IPlantGrowingBlock pcfg)
                    {
                        if (data.blockValue.Block.shape is BlockShapeModelEntity shape)
                        {
                            float progress = Mathf.Max(0f, state.Progress);
                            Vector2 CurrentScale = pcfg.PlantScaleMin +
                                progress / 100f * pcfg.PlantScaleFactor;
                            // Log.Out("+++++++++++++++++++++++++++++++++++ {0}", CurrentScale);
                            // Log.Out("+++++++++++++++++++++++++++++++++++ {0}", data.transform.localScale);
                            // Log.Out("+++++++++++++++++++++++++++++++++PP {0}", data.transform.localPosition);
                            // Log.Out("+++++++++++++++++++++++++++++++++>> {0}", pos);
                            // Log.Out("+++++++++++++++++++++++++++++++++>> {0}/{1}/{2}", World.toBlockXZ(pos.x),
                            //     World.toBlockY(pos.y), World.toBlockXZ(pos.z));
                            // Log.Out("+++++++++++++++++++++++++++++++++++ {0}", CurrentScale);
                            var draw = data.transform.localPosition;
                            draw.y = World.toBlockY(pos.y) +
                                shape.modelOffset.y * CurrentScale.y;
                            data.transform.localPosition = draw;
                            var scale = data.transform.localScale;
                            scale.x = CurrentScale.x;
                            scale.y = CurrentScale.y;
                            scale.z = CurrentScale.x;
                            data.transform.localScale = scale;
                            data.Apply();
                        }
                    }
                    else
                    {
                        chunk.NeedsRegeneration = true;
                    }
                }
                else
                {
                    chunk.NeedsRegeneration = true;
                }
            }
        }

    }
}
