using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NodeFacilitator
{

    //################################################################
    //################################################################

    public static class CustomTerrain
    {

        //********************************************************
        // Struct to pass settings into a map
        //********************************************************

        public struct Blend
        {
            public int texID;
            public float Dirt;
            public float Gravel;
            public float OreCoal;
            public float Asphalt;
            public float OreIron;
            public float OreNitrate;
            public float OreOil;
            public float OreLead;
            public float StoneDesert;
            public float StoneRegular;
            public float StoneDestroyed;
            public float TerrainBlend;
        }

        //********************************************************
        //********************************************************

        // Create counter for virtual ID space
        public static int VirtualIDs = 1300;

        // Get Accessor for private field we need to access
        public static readonly FieldInfo FieldTextureForEachSide =
            AccessTools.Field(typeof(Block), "bTextureForEachSide");

        // Map for virtual ids to custom terrain blend settings
        public static readonly Dictionary<int, Blend>
            CustomBlends = new Dictionary<int, Blend>();

        //********************************************************
        //********************************************************

        // Cleanup static members on game exit
        public static void GameShutdown()
        {
            CustomBlends.Clear();
            VirtualIDs = 1300;
        }

        //********************************************************
        //********************************************************

        public static int Init(Block block, ref Blend blending)
        {
            // Make sure we only have a single texture ID
            // Important for following call to succeed correctly
            if ((bool)FieldTextureForEachSide.GetValue(block)) throw
                new Exception("Terrain Blend must have single texture ID!");
            // Easiest way to query the single texture id after above condition
            // The arguments passed into the function are void in that context
            blending.texID = block.GetSideTextureId(BlockValue.Air, BlockFace.Top);
            // Register us at a fantasy ID
            block.SetSideTextureId(++VirtualIDs);
            // This is the most important setting AFAICT
            // without you may not see any results at all
            block.Properties.ParseFloat("TerrainBlend", ref blending.TerrainBlend);
            // Parse the configuration from the properties
            block.Properties.ParseFloat("BlendDirt", ref blending.Dirt);
            block.Properties.ParseFloat("BlendGravel", ref blending.Gravel);
            block.Properties.ParseFloat("BlendOreCoal", ref blending.OreCoal);
            block.Properties.ParseFloat("BlendAsphalt", ref blending.Asphalt);
            block.Properties.ParseFloat("BlendOreIron", ref blending.OreIron);
            block.Properties.ParseFloat("BlendOreNitrate", ref blending.OreNitrate);
            block.Properties.ParseFloat("BlendOreOil", ref blending.OreOil);
            block.Properties.ParseFloat("BlendOreLoad", ref blending.OreLead);
            block.Properties.ParseFloat("BlendStoneDesert", ref blending.StoneDesert);
            block.Properties.ParseFloat("BlendStoneRegular", ref blending.StoneRegular);
            block.Properties.ParseFloat("BlendStoneDestroyed", ref blending.StoneDestroyed);
            // Remember the settings in a map from virtual IDs to config
            // When we need them, we only get passed the texture ID, which
            // will be the virtual one we registered. Then we can act upon
            // checking within the virtual map first, to see if there is
            // any specific and custom terrain blend config registered.
            CustomBlends.Add(VirtualIDs, blending);
            // Return new virtual id
            return VirtualIDs;

        }

        //********************************************************
        // Harmony Patch to support custom terrain blends
        // We basically create virtual texture IDs that we
        // query in the relevant function we are patching up.
        // When we see an existing virtual ID in the patched
        // function, we act accordingly to create a custom
        // blend setting for the MicroSplat shader.
        //********************************************************

        [HarmonyPatch()]
        static class VoxelMeshTerrain_GetColorForTextureId
        {

            // Use function to select method to patch
            static MethodBase TargetMethod()
            {
                // Pretty unreliable, but works as long as source doesn't change
                return AccessTools.GetDeclaredMethods(typeof(VoxelMeshTerrain))
                    .Find(x => x.Name == "GetColorForTextureId"
                        && x.GetParameters().Length == 8);
            }

            static bool Prefix(
                VoxelMeshTerrain __instance,
                int _subMeshIdx, ref int _fullTexId,
                bool _bTopSoil, ref Color _color,
                ref Vector2 _uv, ref Vector2 _uv2,
                ref Vector2 _uv3, ref Vector2 _uv4)
            {
                // This might be our fantasy ID, intercept and correct
                var texID = VoxelMeshTerrain.DecodeMainTexId(_fullTexId);
                // Log.Out("GetColor for {0}", texID);
                // Check if this texture ID is known to use as a virtual ID
                // If found we have custom terrain blend settings to apply
                if (CustomBlends.TryGetValue(texID, out Blend blend))
                {
                    // In some cases we need to fallback to the fallback
                    if (!World.IsSplatMapAvailable || __instance.IsPreviewVoxelMesh)
                    {
                        _uv = _uv2 = _uv3 = _uv4 = Vector2.zero;
                        // Pack the original texture id again and pass to original code
                        // Will e.g. render the preview with a single fallback texture
                        // Re-implemented from original `GetColorForTextureId`
                        _fullTexId = VoxelMeshTerrain.EncodeTexIds(blend.texID, 0);
                        _color = __instance.submeshes[_subMeshIdx].GetColorForTextureId(_fullTexId);
                        Log.Out("Encoded texture ID {0} to get fallback Color {1}", blend.texID, _color);
                    }
                    else
                    {
                        // Set custom MicroSplat blend settings
                        _color = new Color(blend.Dirt, blend.Gravel,
                            blend.OreCoal, blend.TerrainBlend);
                        _uv = new Vector2(blend.Asphalt, blend.OreIron);
                        _uv2 = new Vector2(blend.OreNitrate, blend.StoneRegular);
                        _uv3 = new Vector2(blend.StoneDesert, blend.OreOil);
                        _uv4 = new Vector2(blend.OreLead, blend.StoneDestroyed);
                        return false;
                    }
                }
                // Invoke regular code
                return true;
            }
        }

        //********************************************************
        //********************************************************

    }

    //################################################################
    //################################################################

}
