using HarmonyLib;
using RimWorld;

namespace SelectedItems
{
    [HarmonyPatch(typeof(ITab_Storage), "FillTab")]
    public static class StorageTabContext
    {
        public static bool Active;

        public static void Prefix()
        {
            Active = true;
        }

        public static void Finalizer()
        {
            Active = false;
        }
    }
}
