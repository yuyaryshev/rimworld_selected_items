using HarmonyLib;
using RimWorld;
using Verse;

namespace SelectedItems
{
    [HarmonyPatch(typeof(Selector), nameof(Selector.Select))]
    public static class StorageAutoOpenPatch
    {
        public static void Postfix(object obj)
        {
            if (!SelectedItemsMod.RuntimeEnabled() || SelectedItemsMod.Settings?.OpenStorageTabOnSelect != true)
            {
                return;
            }
            if (obj == null || Find.Selector == null || Find.Selector.SelectedObjects.Count != 1 || !ReferenceEquals(Find.Selector.SingleSelectedObject, obj))
            {
                return;
            }
            if (StorageNavigation.HasStorageTab(obj))
            {
                InspectPaneUtility.OpenTab(typeof(ITab_Storage));
            }
        }
    }
}
