using System.Collections.Generic;
using UnityEngine;

namespace NodeFacilitator
{

    public class BoundsHelper : MonoBehaviour
    {

        private static Transform GoRoot;
        private static Transform GoPool;

        private static Dictionary<Vector3i, HelperEntry> Bounds
            = new Dictionary<Vector3i, HelperEntry>();
        private static Dictionary<Vector3i, HelperEntry> Reachs
            = new Dictionary<Vector3i, HelperEntry>();

        private static Dictionary<Vector3i, HelperEntry>
            Helpers(bool alt) => alt ? Reachs : Bounds;

        private static void DestroyHelper(Vector3i pos,
            Dictionary<Vector3i, HelperEntry> Helpers)
        {
            if (Helpers.TryGetValue(pos, out HelperEntry helper))
            {
                if (helper.Helper == null) return;
                helper.Helper.parent = GoPool;
                helper.Helper.localPosition = Vector3.zero;
                helper.Helper.gameObject.SetActive(false);
                Helpers.Remove(pos);
            }
        }

        public static void RemoveBoundsHelper(Vector3i pos)
        {
            DestroyHelper(pos, Bounds);
            DestroyHelper(pos, Reachs);
        }

        private static Transform CreateFocusCube()
        {
            List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
            if (localPlayers == null || localPlayers.Count <= 0) return null;
            NGuiWdwInGameHUD component = LocalPlayerUI.GetUIForPlayer(localPlayers[0])
                .nguiWindowManager.GetWindow(EnumNGUIWindow.InGameHUD).GetComponent<NGuiWdwInGameHUD>();
            if (component.FocusCube == null) return null;
            return Instantiate(component.FocusCube);
        }

        public static Transform GetBoundsHelper(
            Vector3i pos, bool alt = false)
        {
            if (GoRoot == null) InitHelpers();
            if (!Helpers(alt).TryGetValue(pos,
                out HelperEntry helper))
            {
                Transform cube = GoPool.childCount > 0 ?
                    GoPool.GetChild(0) : CreateFocusCube();
                Helpers(alt).Add(pos, helper =
                    new HelperEntry(pos, cube));
                cube.parent = GoRoot;
            }
            return helper.Helper;
        }

        private static void InitHelpers()
        {
            if (GoRoot != null) return;
            GoRoot = new GameObject("BoundHelpers").transform;
            GoPool = new GameObject("Pool").transform;
            GoPool.localPosition = Vector3.zero;
            GoPool.localScale = Vector3.one;
            GoPool.parent = GoRoot;
            Origin.Add(GoRoot, 0);
        }

        public static void CleanupHelpers()
        {
            foreach (var bound in Bounds)
                Destroy(bound.Value.Helper);
            Bounds.Clear();
            foreach (var reach in Reachs)
                Destroy(reach.Value.Helper);
            Reachs.Clear();
        }

        public class HelperEntry : IEqualityComparer<HelperEntry>
        {

            public Vector3 Position;
            public Transform Helper;

            public HelperEntry(Vector3i position, Transform helper)
            {
                Position = position;
                Helper = helper;
            }

            public bool Equals(HelperEntry x, HelperEntry y)
                => x.Position.Equals(y.Position);

            public int GetHashCode(HelperEntry obj)
                => obj.Position.GetHashCode();
        }
    }

}
