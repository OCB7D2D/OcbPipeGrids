using HarmonyLib;
using NodeFacilitator;
using UnityEngine;

class BlockShapeBillboardPlantGrowth
{

    // Poor man's fix to save IL headache
    static Vector2 CurrentScale = Vector2.one;

    [HarmonyPatch(typeof(BlockShapeGrass))]
    [HarmonyPatch("renderFull")]
    public class BlockShapeGrass_renderFull
    {
        static void Prefix(Vector3i _worldPos,
            BlockValue _blockValue, Vector3 _drawPos)
        {
            CurrentScale.x = CurrentScale.y = 1f;
            if (_blockValue.Block is IPlantGrowingBlock pcfg)
            {
                CurrentScale = pcfg.PlantScaleMin;
                // Don't really like how this looks, but it works
                // Not sure why we are not passed original position
                Vector3i pos = _worldPos + new Vector3i(_drawPos);
                var client = NodeManagerInterface.Instance.Client;
                if (client.PlantStates.TryGetValue(pos,
                    out PlantProgress state))
                {
                    float progress = Mathf.Max(0f, state.Progress);
                    CurrentScale += progress / 100f * pcfg.PlantScaleFactor;
                    // CurrentScale = progress * pcfg.PlantScaleFactor;
                    // CurrentScale += pcfg.PlantScaleMin;
                }
                else
                {
                    Log.Error("1 Did not find PlantState for IPlantBlock");
                }

            }
        }
    }

    [HarmonyPatch(typeof(BlockShapeBillboardPlant))]
    [HarmonyPatch("renderFull")]
    public class BlockShapeBillboardPlant_renderFull
    {
        static void Prefix(Vector3i _worldPos,
            BlockValue _blockValue, Vector3 _drawPos)
        {
            // Log.Out("Setup scale for spin mesh");
            CurrentScale.x = CurrentScale.y = 1f;
            if (_blockValue.Block is IPlantGrowingBlock pcfg)
            {
                CurrentScale = pcfg.PlantScaleMin;
                // Don't really like how this looks, but it works
                // Not sure why we are not passed original position
                Vector3i pos = _worldPos + new Vector3i(_drawPos);
                var client = NodeManagerInterface.Instance.Client;
                if (client.PlantStates.TryGetValue(pos,
                    out PlantProgress state))
                {
                    float progress = state.Progress / 100f;
                    CurrentScale += progress * pcfg.PlantScaleFactor;
                    // Log.Out(" set scale to {1} => {0}", CurrentScale, progress);
                }
                else
                {
                    Log.Error("2 Did not find PlantState for IPlantBlock");
                }

            }
        }
    }

    [HarmonyPatch(typeof(BlockShapeBillboardPlant))]
    [HarmonyPatch("RenderSpinMesh")]
    public class BlockShapeBillboardPlant_RenderSpinMesh
    {
        static void Prefix(ref BlockShapeBillboardPlant.RenderData _data)
        {
            if (CurrentScale.x != 1 || CurrentScale.y != 1)
            {
                // Log.Out("Render spin mesh {0}", CurrentScale.y);
                _data.scale *= CurrentScale.x;
                _data.height *= CurrentScale.y;
            }
        }
    }

}

