using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[HarmonyPatch()]
class VoxelMeshTerrain_GetColorForTextureId
{
	static MethodBase TargetMethod()
	{
		// Pretty unreliable, but works as long as source doesn't change
		return AccessTools.GetDeclaredMethods(typeof(VoxelMeshTerrain))
			.FindAll(x => x.Name == "GetColorForTextureId")?[1];
	}

	public static float c1 = 0.8f; // terrDirt
	public static float c2 = 0.3f; // terrGravel
	public static float c3 = 0.2f; // terrOreCoal
	public static float c4 = 1f; // blend mode/depth?
	public static float uv1A = 0f; // terrAsphalt
	public static float uv1B = 0f; // terrOreIron
	public static float uv2A = 0f; // terrOrePotassiumNitrate
	public static float uv2B = 0f; // terrStone
	public static float uv3A = 0.3f; // Unused?
	public static float uv3B = 0f; // terrOreOilDeposit
	public static float uv4A = 0f; // terrOreLead
	public static float uv4B = 0f; // terrDestroyedStone

	static bool Prefix(int _texId,
		bool _bTopSoil,
		ref Color _color,
		ref Vector2 _uv,
		ref Vector2 _uv2,
		ref Vector2 _uv3,
		ref Vector2 _uv4)
	{
		_uv = _uv2 = _uv3 = _uv4 = Vector2.zero;
		/*if (Random.Range(0.01f, 1.0f) > 0.95f)
		{
			_uv = new Vector2(0.0f, 0.0f);
			_uv2 = new Vector2(0.0f, 0.0f);
			_uv3 = new Vector2(0.0f, 0.0f);
			_uv4 = new Vector2(0.0f, 0.0f);
			_color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		}
		else*/ if (_bTopSoil)
		{
			_uv = new Vector2(0.0f, 0.0f);
			_uv2 = new Vector2(0.0f, 0.0f);
			_uv3 = new Vector2(0.0f, 0.0f);
			_uv4 = new Vector2(0.0f, 0.0f);
			_color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		}
		else
		{
			_color = new Color(0.0f, 0.0f, 0.0f, 1f);
			switch (_texId)
			{
				case 1: // terrStone
					_uv2 = new Vector2(0.0f, 1f);
					break;
				case 2: // terrDirt
					_color = new Color(1f, 0.0f, 0.0f, 1f);
					break;
				case 10: // terrAsphalt
					_uv = new Vector2(1f, 0.0f);
					break;
				case 11: // terrGravel
					_color = new Color(0.0f, 1f, 0.0f, 1f);
					break;
				case 12: // terrGravel
					Log.Out("Terr Gravel 2 seen");
					_uv = new Vector2(uv1A, uv1B); // nitrate
					_uv2 = new Vector2(uv2A, uv2B); // nitrate
					_uv3 = new Vector2(uv3A, uv3B); // unused
					_uv4 = new Vector2(uv4A, uv4B); // unused
					_color = new Color(c1, c2, c3, c4);
					break;
				case 33: // terrOreIron
					_uv = new Vector2(0.0f, 1f);
					break;
				case 34: // terrOreCoal
					_color = new Color(0.0f, 0.0f, 1f, 1f);
					break;
				case 184: // Unused?
					_uv3 = new Vector2(1f, 0.0f);
					break;
				case 300: // terrOrePotassiumNitrate
					_uv2 = new Vector2(1f, 0.0f);
					break;
				case 316: // terrOreLead
					_uv4 = new Vector2(1f, 0.0f);
					break;
				case 438: // terrDestroyedStone
					_uv4 = new Vector2(0.0f, 1f);
					break;
				case 440: // terrOreOilDeposit
					_uv3 = new Vector2(0.0f, 1f);
					break;
				default:
					_color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
					break;
			}
		}
		return false;
	}
}

public class TerrainTestCmd : ConsoleCmdAbstract
{

    public override bool IsExecuteOnClient => true;

    public override bool AllowedInMainMenu => true;

    public override int DefaultPermissionLevel => 1000;

    public override string[] GetCommands() => new string[1] { "tst" };

    public override string GetDescription() => "Test commands";

    public override string GetHelp() => "Test commands:";

    public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
    {
		if (args.Count == 0)
        {
			Log.Out("UV1 {0}/{1}, UV2 {2}/{3}, UV3 {4}/{5}, UV4 {6}/{7}, COL {8}/{9}/{10}/{11}",
				VoxelMeshTerrain_GetColorForTextureId.uv1A,
				VoxelMeshTerrain_GetColorForTextureId.uv1B,
				VoxelMeshTerrain_GetColorForTextureId.uv2A,
				VoxelMeshTerrain_GetColorForTextureId.uv2B,
				VoxelMeshTerrain_GetColorForTextureId.uv3A,
				VoxelMeshTerrain_GetColorForTextureId.uv3B,
				VoxelMeshTerrain_GetColorForTextureId.uv4A,
				VoxelMeshTerrain_GetColorForTextureId.uv4B,
				VoxelMeshTerrain_GetColorForTextureId.c1,
				VoxelMeshTerrain_GetColorForTextureId.c2,
				VoxelMeshTerrain_GetColorForTextureId.c3,
				VoxelMeshTerrain_GetColorForTextureId.c4);

		}
		else if (args.Count == 2)
        {
			switch (args[0])
            {
				case "1A": VoxelMeshTerrain_GetColorForTextureId.uv1A = float.Parse(args[1]); break;
				case "1B": VoxelMeshTerrain_GetColorForTextureId.uv1B = float.Parse(args[1]); break;
				case "2A": VoxelMeshTerrain_GetColorForTextureId.uv2A = float.Parse(args[1]); break;
				case "2B": VoxelMeshTerrain_GetColorForTextureId.uv2B = float.Parse(args[1]); break;
				case "3A": VoxelMeshTerrain_GetColorForTextureId.uv3A = float.Parse(args[1]); break;
				case "3B": VoxelMeshTerrain_GetColorForTextureId.uv3B = float.Parse(args[1]); break;
				case "4A": VoxelMeshTerrain_GetColorForTextureId.uv4A = float.Parse(args[1]); break;
				case "4B": VoxelMeshTerrain_GetColorForTextureId.uv4B = float.Parse(args[1]); break;
				case "C1": VoxelMeshTerrain_GetColorForTextureId.c1 = float.Parse(args[1]); break;
				case "C2": VoxelMeshTerrain_GetColorForTextureId.c2 = float.Parse(args[1]); break;
				case "C3": VoxelMeshTerrain_GetColorForTextureId.c3 = float.Parse(args[1]); break;
				case "C4": VoxelMeshTerrain_GetColorForTextureId.c4 = float.Parse(args[1]); break;
				default: Log.Warning("Invalid command {0}", args[0]); break;
			}
		}
		else
        {
			Log.Warning("Invalid tst cmd");
        }
    }

}
