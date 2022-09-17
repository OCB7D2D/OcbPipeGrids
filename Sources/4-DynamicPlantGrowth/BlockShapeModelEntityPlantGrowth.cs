using HarmonyLib;
using NodeFacilitator;
using UnityEngine;
using static LightingAround;

class BlockShapeModelEntityPlantGrowth
{

    // Ext3dModel
    [HarmonyPatch(typeof(BlockShapeModelEntity))]
    [HarmonyPatch("OnBlockEntityTransformBeforeActivated")]
    public class BlockShapeModelEntityRender
    {
        static void Postfix(
            WorldBase _world,
            Vector3i _blockPos,
            int _cIdx,
            BlockValue _blockValue,
            BlockEntityData _ebcd)
        {
            if (_blockValue.Block is IPlantGrowingBlock pcfg)
            {
                if (_ebcd.bHasTransform)
                {
                    Vector2 CurrentScale = pcfg.PlantScaleMin;
                    var client = NodeManagerInterface.Instance.Client;
                    if (client.PlantStates.TryGetValue(_blockPos,
                        out PlantProgress state))
                    {
                        float progress = Mathf.Max(0f, state.Progress);
                        CurrentScale += progress / 100f * pcfg.PlantScaleFactor;
                        //var offset = _drawPos + new Vector3(0.5f, 0, 0.5f);
                        //for (int i = __state; i < vertices.Count; i++)
                        //{
                        //    items[i] -= offset;
                        //    items[i].x *= CurrentScale.x;
                        //    items[i].y *= CurrentScale.y;
                        //    items[i].z *= CurrentScale.x;
                        //    items[i] += offset;
                        //}
                    }
                    var scale = _ebcd.transform.localScale;
                    scale.x *= CurrentScale.x;
                    scale.y *= CurrentScale.y;
                    scale.z *= CurrentScale.x;
                    _ebcd.transform.localScale = scale;
                    _ebcd.Apply();

                }
            }
        }
        /*
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
                if (client.PlantStates.TryGetValue(pos,
                    out PlantProgress state))
                {
                    Vector2 CurrentScale = pcfg.PlantScaleMin +
                        state.Progress / 100f * pcfg.PlantScaleFactor;
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
        */
    }

}

