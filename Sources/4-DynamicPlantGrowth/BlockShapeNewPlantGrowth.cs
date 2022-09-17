using HarmonyLib;
using NodeFacilitator;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

class BlockShapeNewPlantGrowth
{

    // Poor man's fix to save IL headache
    static Vector2 CurrentScale = Vector2.one;
    
    [HarmonyPatch(typeof(BlockShapeNew))]
    [HarmonyPatch("renderFace")]
    public static class BlockShapeNewRenderFace
    {

        public static Vector3 ScaleVector3by2(Vector3 vector, Vector2 scale)
        {
            vector.x *= scale.x;
            vector.y *= scale.y;
            vector.z *= scale.x;
            return vector;
        }

        static void ApplyScaleOffset(Vector3i cpos, BlockValue bv, ref Vector3 pos)
        {
            CurrentScale = Vector2.one;
            if (bv.Block is IPlantGrowingBlock block && bv.Block.shape is BlockShapeNew)
            {
                var client = NodeManagerInterface.Instance.Client;
                Vector3i bpos = cpos + new Vector3i(pos.x, pos.y, pos.z);
                var scale = block.PlantScaleMin;
                if (client.PlantStates.TryGetValue(bpos,
                    out PlantProgress state))
                {
                    float progress = Mathf.Max(0f, state.Progress);
                    scale += progress / 100f * block.PlantScaleFactor;
                }
                pos.y += 0.5f * scale.y - 0.5f;
                CurrentScale = scale;
            }
        }

        static readonly MethodInfo MethodScaleVector3by2 = AccessTools.Method(
            typeof(BlockShapeNewRenderFace), "ScaleVector3by2");

        static readonly MethodInfo MethodApplyScaleOffset = AccessTools.Method(
            typeof(BlockShapeNewRenderFace), "ApplyScaleOffset");

        static readonly FieldInfo FieldCurrentScale = AccessTools.Field(
            typeof(BlockShapeNewPlantGrowth), "CurrentScale");

        static IEnumerable<CodeInstruction> Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // Apply patch to apply offset and store scale
            // Scale will be used later in the second patch
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Callvirt) continue;
                if (!(codes[i].operand is MethodInfo virt)) continue;
                if (virt.Name != "GetRotationOffset") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!(codes[i].operand is MethodInfo call)) continue;
                if (call.Name != "op_Addition") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Stloc_S) continue;
                if (!(codes[i].operand is LocalBuilder var)) continue;
                // Since we insert always at this position, you need to read the code from bottom to top ;)
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, MethodApplyScaleOffset));
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloca_S, var.LocalIndex));
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2)); // put blockValue
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1)); // put chunkPos
            }
            // Patch scale calculation to add static scale from 1st step
            // We use a static variable since we know it is synchronous
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldloc_S) continue;
                if (!(codes[i].operand is LocalVariableInfo vdf)) continue;
                if (vdf.LocalIndex != 29) continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!(codes[i].operand is MethodInfo met1)) continue;
                if (met1.Name != "op_Addition") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Call) continue;
                if (!(codes[i].operand is MethodInfo met2)) continue;
                if (met2.Name != "op_Multiply") continue;
                if (++i >= codes.Count) break;
                if (codes[i].opcode != OpCodes.Ldloc_S) continue;
                if (!(codes[i].operand is LocalVariableInfo rdf)) continue;
                if (rdf.LocalIndex != 30) continue;
                // Since we insert always at this position, you need to read the code from bottom to top ;)
                codes.Insert(i - 1, new CodeInstruction(OpCodes.Call, MethodScaleVector3by2)); // call function
                codes.Insert(i - 1, new CodeInstruction(OpCodes.Ldsfld, FieldCurrentScale)); // put scale
            }
            return codes;
        }
    }

}

