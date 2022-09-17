using HarmonyLib;

namespace NodeFacilitator
{
    static class HarmonyHook
    {

        // Whenever power state changes, we
        // pass that info down to the worker
        [HarmonyPatch(typeof(PowerConsumer))]
        [HarmonyPatch("IsPoweredChanged")]
        private class IsPoweredChanged
        {
            static void Prefix(
                PowerItem __instance,
                bool newPowered)
            {
                if (!NodeManagerInterface.HasInstance) return;
                if (!NodeManagerInterface.HasServer) return;
                var action = new ActionSetPower();
                action.Setup(__instance.Position, newPowered);
                NodeManagerInterface.PushToWorker(action);
            }
        }

    }
}
