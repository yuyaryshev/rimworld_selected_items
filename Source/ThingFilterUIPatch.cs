using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SelectedItems
{
    [HarmonyPatch(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow))]
    [HarmonyPriority(Priority.First)]
    public static class ThingFilterUIPatch
    {
        public static bool Prefix(
            ref Rect rect,
            ThingFilterUI.UIState state,
            ThingFilter filter,
            ThingFilter parentFilter = null,
            int openMask = 1,
            IEnumerable<ThingDef> forceHiddenDefs = null,
            IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null,
            bool forceHideHitPointsConfig = false,
            bool forceHideQualityConfig = false,
            bool showMentalBreakChanceRange = false,
            List<ThingDef> suppressSmallVolumeTags = null,
            Map map = null)
        {
            if (!SelectedItemsMod.RuntimeEnabled() || !StorageTabContext.Active)
            {
                return true;
            }

            TreeNode_ThingCategory node = filter.RootNode;
            if (parentFilter != null)
            {
                node = parentFilter.DisplayRootCategory;
            }

            List<ThingDef> storedDefs = StorageContents.StoredDefs(StorageTabContext.CurrentParent);
            SelectedItemSnapshot snapshot = StorageFilterSelection.SnapshotFor(filter, parentFilter, forceHiddenDefs, node, storedDefs);
            float selectedPanelHeight = SelectedItemsPanel.HeightFor(snapshot);
            if (selectedPanelHeight > 0f)
            {
                Rect selectedRect = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 16f - 6f, selectedPanelHeight);
                SelectedItemsPanel.Draw(selectedRect, filter, parentFilter, forceHiddenDefs, node, storedDefs, snapshot);
                rect.yMin = selectedRect.yMax + 3f;
            }

            return true;
        }
    }
}
