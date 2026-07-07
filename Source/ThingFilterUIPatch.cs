using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SelectedItems
{
    [HarmonyPatch(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow))]
    public static class ThingFilterUIPatch
    {
        private static float viewHeight;

        public static bool Prefix(
            Rect rect,
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
            if (!StorageTabContext.Active)
            {
                return true;
            }

            DrawThingFilterConfigWindow(rect, state, filter, parentFilter, openMask, forceHiddenDefs, forceHiddenFilters, forceHideHitPointsConfig, forceHideQualityConfig, showMentalBreakChanceRange, suppressSmallVolumeTags, map);
            return false;
        }

        private static void DrawThingFilterConfigWindow(Rect rect, ThingFilterUI.UIState state, ThingFilter filter, ThingFilter parentFilter, int openMask, IEnumerable<ThingDef> forceHiddenDefs, IEnumerable<SpecialThingFilterDef> forceHiddenFilters, bool forceHideHitPointsConfig, bool forceHideQualityConfig, bool showMentalBreakChanceRange, List<ThingDef> suppressSmallVolumeTags, Map map)
        {
            Widgets.DrawMenuSection(rect);
            float width = rect.width - 2f;
            Rect clearRect = new Rect(rect.x + 3f, rect.y + 3f, width / 2f - 3f - 1.5f, 24f);
            if (Widgets.ButtonText(clearRect, "ClearAll".Translate()))
            {
                filter.SetDisallowAll(forceHiddenDefs, forceHiddenFilters);
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }
            if (Widgets.ButtonText(new Rect(clearRect.xMax + 3f, clearRect.y, clearRect.width, 24f), "AllowAll".Translate()))
            {
                filter.SetAllowAll(parentFilter);
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }

            rect.yMin = clearRect.yMax;
            Rect searchRect = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 16f - 6f, 24f);
            state.quickSearch.OnGUI(searchRect);
            rect.yMin = searchRect.yMax + 3f;

            TreeNode_ThingCategory node = filter.RootNode;
            bool hitPointsConfigurable = true;
            bool qualityConfigurable = true;
            if (parentFilter != null)
            {
                node = parentFilter.DisplayRootCategory;
                hitPointsConfigurable = parentFilter.allowedHitPointsConfigurable;
                qualityConfigurable = parentFilter.allowedQualitiesConfigurable;
            }

            SelectedItemSnapshot snapshot = StorageFilterSelection.SnapshotFor(filter, parentFilter, forceHiddenDefs, node);
            float selectedPanelHeight = SelectedItemsPanel.HeightFor(snapshot);
            if (selectedPanelHeight > 0f)
            {
                Rect selectedRect = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 16f - 6f, selectedPanelHeight);
                SelectedItemsPanel.Draw(selectedRect, filter, snapshot);
                rect.yMin = selectedRect.yMax + 3f;
            }

            rect.xMax -= 4f;
            rect.yMax -= 6f;
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);
            Rect visibleRect = new Rect(0f, 0f, rect.width, rect.height);
            visibleRect.position += state.scrollPosition;
            Widgets.BeginScrollView(rect, ref state.scrollPosition, viewRect);

            float y = 2f;
            if (hitPointsConfigurable && !forceHideHitPointsConfig)
            {
                DrawHitPointsFilterConfig(ref y, viewRect.width, filter);
            }
            if (qualityConfigurable && !forceHideQualityConfig)
            {
                DrawQualityFilterConfig(ref y, viewRect.width, filter);
            }
            if (ModsConfig.AnomalyActive && showMentalBreakChanceRange)
            {
                DrawMentalBreakFilterConfig(ref y, viewRect.width, filter);
            }

            float treeTop = y;
            Rect treeRect = new Rect(0f, y, viewRect.width, 9999f);
            visibleRect.position -= treeRect.position;
            Listing_TreeThingFilter listing = new Listing_TreeThingFilter(filter, parentFilter, forceHiddenDefs, forceHiddenFilters, suppressSmallVolumeTags, state.quickSearch.filter);
            listing.Begin(treeRect);
            listing.ListCategoryChildren(node, openMask, map, visibleRect);
            listing.End();
            state.quickSearch.noResultsMatched = listing.matchCount == 0;
            if (Event.current.type == EventType.Layout)
            {
                viewHeight = treeTop + listing.CurHeight + 90f;
            }

            Widgets.EndScrollView();
        }

        private static void DrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter)
        {
            Rect rect = new Rect(20f, y, width - 20f, 32f);
            FloatRange range = filter.AllowedHitPointsPercents;
            Widgets.FloatRange(rect, 1, ref range, 0f, 1f, "HitPoints", ToStringStyle.PercentZero, 0f, GameFont.Small, null, 0.01f);
            filter.AllowedHitPointsPercents = range;
            y += 37f;
            Text.Font = GameFont.Small;
        }

        private static void DrawQualityFilterConfig(ref float y, float width, ThingFilter filter)
        {
            Rect rect = new Rect(20f, y, width - 20f, 32f);
            QualityRange range = filter.AllowedQualityLevels;
            Widgets.QualityRange(rect, 876813230, ref range);
            filter.AllowedQualityLevels = range;
            y += 37f;
            Text.Font = GameFont.Small;
        }

        private static void DrawMentalBreakFilterConfig(ref float y, float width, ThingFilter filter)
        {
            Rect rect = new Rect(20f, y, width - 20f, 32f);
            FloatRange range = filter.AllowedMentalBreakChance;
            Widgets.FloatRange(rect, 968573221, ref range, 0f, 1f, "MaxMentalBreakChance", ToStringStyle.PercentZero);
            filter.AllowedMentalBreakChance = range;
            y += 37f;
            Text.Font = GameFont.Small;
        }
    }
}
