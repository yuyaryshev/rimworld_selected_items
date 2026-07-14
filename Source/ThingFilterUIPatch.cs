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
        public struct DrawState
        {
            public bool DrawInPostfix;
            public Rect Rect;
            public ThingFilter Filter;
            public ThingFilter ParentFilter;
            public IEnumerable<ThingDef> ForceHiddenDefs;
            public TreeNode_ThingCategory DisplayRoot;
            public List<ThingDef> StoredDefs;
            public SelectedItemSnapshot Snapshot;
        }

        public static bool Prefix(
            ref Rect rect,
            out DrawState __state,
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
            __state = default;
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
            SelectedItemSnapshot snapshot = StorageFilterSelection.SnapshotFor(filter, parentFilter, forceHiddenDefs, node, storedDefs, state?.quickSearch?.filter);
            float selectedPanelHeight = SelectedItemsPanel.HeightFor(snapshot);
            if (selectedPanelHeight > 0f)
            {
                float panelWidth = rect.width - 16f - 6f;
                float gapTop = EffectiveUiGapTop();
                float gapBottom = Mathf.Clamp(SelectedItemsMod.Settings?.UiGapBottom ?? 0, 0, 500);
                float reservedHeight = gapTop + selectedPanelHeight + gapBottom;
                if (SelectedItemsMod.Settings?.DrawSelectedItemsBelowStorageFilters == true && rect.height > reservedHeight + 80f)
                {
                    Rect reservedRect = new Rect(rect.x + 3f, rect.yMax - reservedHeight - 3f, panelWidth, reservedHeight);
                    Rect selectedRect = new Rect(reservedRect.x, reservedRect.y + gapTop, reservedRect.width, selectedPanelHeight);
                    rect.yMax = reservedRect.y - 3f;
                    __state = new DrawState
                    {
                        DrawInPostfix = true,
                        Rect = selectedRect,
                        Filter = filter,
                        ParentFilter = parentFilter,
                        ForceHiddenDefs = forceHiddenDefs,
                        DisplayRoot = node,
                        StoredDefs = storedDefs,
                        Snapshot = snapshot
                    };
                }
                else
                {
                    Rect reservedRect = new Rect(rect.x + 3f, rect.y + 3f, panelWidth, reservedHeight);
                    Rect selectedRect = new Rect(reservedRect.x, reservedRect.y + gapTop, reservedRect.width, selectedPanelHeight);
                    SelectedItemsPanel.Draw(selectedRect, filter, parentFilter, forceHiddenDefs, node, storedDefs, snapshot);
                    rect.yMin = reservedRect.yMax + 3f;
                }
            }

            return true;
        }

        private static float EffectiveUiGapTop()
        {
            int configured = SelectedItemsMod.Settings?.UiGapTop ?? 0;
            if (configured <= 0 && PrecisionStockpileControlBridge.Loaded)
            {
                return 32f;
            }
            return Mathf.Clamp(configured, 0, 500);
        }

        public static void Postfix(DrawState __state)
        {
            if (__state.DrawInPostfix)
            {
                SelectedItemsPanel.Draw(__state.Rect, __state.Filter, __state.ParentFilter, __state.ForceHiddenDefs, __state.DisplayRoot, __state.StoredDefs, __state.Snapshot);
            }
        }
    }
}
