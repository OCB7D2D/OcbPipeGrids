using HarmonyLib;
using NodeFacilitator;
using UnityEngine;

class BlockShapeExt3dModelPlantGrowth
{

    // Ext3dModel
    [HarmonyPatch(typeof(BlockShapeExt3dModel))]
    [HarmonyPatch("renderFull")]
    public class BlockShapeExt3dModel_renderFull
    {

        static void Prefix(
            BlockValue _blockValue,
            VoxelMesh[] _meshes,
            ref int __state)
        {
            int idx = _blockValue.Block.MeshIndex;
            __state = _meshes[idx].Vertices.Count;
        }

        static void Postfix(
            Vector3i _worldPos,
            BlockValue _blockValue,
            Vector3 _drawPos,
            VoxelMesh[] _meshes,
            int __state)
        {
            if (_blockValue.Block is IPlantGrowingBlock pcfg)
            {
                int idx = _blockValue.Block.MeshIndex;
                var vertices = _meshes[idx].Vertices;
                var items = vertices.Items;
                var pos = new Vector3i(_worldPos + _drawPos);
                var client = NodeManagerInterface.Instance.Client;
                Vector2 CurrentScale = pcfg.PlantScaleMin;
                if (client.PlantStates.TryGetValue(pos,
                    out PlantProgress state))
                {
                    float progress = Mathf.Max(0f, state.Progress);
                    CurrentScale += progress / 100f * pcfg.PlantScaleFactor;
                    var offset = _drawPos + new Vector3(0.5f, 0, 0.5f);
                    for (int i = __state; i < vertices.Count; i++)
                    {
                        items[i] -= offset;
                        items[i].x *= CurrentScale.x;
                        items[i].y *= CurrentScale.y;
                        items[i].z *= CurrentScale.x;
                        items[i] += offset;
                    }
                }
            }
        }
    }

}

