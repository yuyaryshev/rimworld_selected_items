using HarmonyLib;
using RimWorld;

namespace SelectedItems
{
    [HarmonyPatch(typeof(ITab_Storage), "FillTab")]
    public static class StorageTabContext
    {
        private static readonly System.Reflection.MethodInfo SelStoreSettingsParentGetter =
            AccessTools.PropertyGetter(typeof(ITab_Storage), "SelStoreSettingsParent");

        public static bool Active;
        public static IStoreSettingsParent CurrentParent;

        public static void Prefix(ITab_Storage __instance)
        {
            if (!SelectedItemsMod.RuntimeEnabled())
            {
                Active = false;
                CurrentParent = null;
                return;
            }

            Active = true;
            CurrentParent = SelStoreSettingsParentGetter?.Invoke(__instance, null) as IStoreSettingsParent;
        }

        public static void Finalizer()
        {
            Active = false;
            CurrentParent = null;
        }
    }
}
