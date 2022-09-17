using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Xml;

namespace NodeFacilitator
{


    //########################################################
    //########################################################

    // Any Block that has this interface will automatically
    // invoke the implementation for `PostParse(XmlElement)`
    public interface ICustomBlockParser
    {
        void PostParse(XmlElement xml);
    }

    //########################################################
    //########################################################

    // Harmony hook to call `PostParse(XmlElements`
    // for each Block that is of `ICustomBlockParser`
    public class CustomBlockParser
    {

        [HarmonyPatch(typeof(BlocksFromXml))]
        [HarmonyPatch("ParseBlock")]
        public static class BlocksFromXmlParseBlock
        {

            static void PostInitBlock(Block block, XmlElement xml)
            {
                if (block is ICustomBlockParser blk) blk.PostParse(xml);
            }

            static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 1; i < codes.Count; i++)
                {
                    if (codes[i].opcode != OpCodes.Ldloc_S) continue;
                    if (!(codes[i].operand is LocalVariableInfo blk)) continue;
                    if (blk.LocalType.Name != "Block") continue;
                    if (++i >= codes.Count) break;
                    if (codes[i].opcode != OpCodes.Callvirt) continue;
                    if (!(codes[i].operand is MethodInfo virt)) continue;
                    if (virt.DeclaringType.Name != "Block") continue;
                    if (virt.Name != "Init") continue;
                    codes.Insert(i + 1, CodeInstruction.Call(typeof(BlocksFromXmlParseBlock), "PostInitBlock"));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1, null));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_S, blk.LocalIndex));
                }
                return codes;
            }
        }

    }
}
