using UnityEngine;
using RimWorld;
using Verse;

namespace SelectedItems
{
    public static class SelectedItemsPanel
    {
        private const float RowHeight = 24f;
        private const float HeaderHeight = 24f;
        private const float Padding = 3f;
        private const float ArrowButtonSize = 22f;

        public static float HeightFor(SelectedItemSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 0f;
            }

            float minHeight = HeaderHeight + Padding * 2f;
            if (snapshot.Expanded &&
                SelectedItemsMod.Settings?.AddStockpileSwitchArrows == true &&
                (StorageNavigation.TryFindNeighbor(StorageNavigation.Direction.Up, out _) ||
                 StorageNavigation.TryFindNeighbor(StorageNavigation.Direction.Down, out _)))
            {
                minHeight = ArrowButtonSize * 3f + Padding * 4f;
            }

            if (!snapshot.Expanded || snapshot.Items.Count == 0)
            {
                return minHeight;
            }

            int scrollStartsAt = SelectedItemsMod.Settings?.ScrollStartsAt ?? 5;
            scrollStartsAt = Mathf.Clamp(scrollStartsAt, 1, 2000);
            int visibleRows = snapshot.Rows.Count > scrollStartsAt ? scrollStartsAt : snapshot.Rows.Count;
            float searchHeight = SearchHeightFor(snapshot);
            return Mathf.Max(minHeight, HeaderHeight + Padding + visibleRows * RowHeight + searchHeight + Padding);
        }

        public static void Draw(Rect rect, ThingFilter filter, ThingFilter parentFilter, System.Collections.Generic.IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, System.Collections.Generic.List<ThingDef> storedDefs, SelectedItemSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(Padding);
            if (snapshot.Expanded && SelectedItemsMod.Settings?.AddStockpileSwitchArrows == true)
            {
                DrawSwitchArrows(rect, ref inner);
            }

            Text.Font = GameFont.Tiny;
            Rect headerRect = new Rect(inner.x, inner.y, inner.width, HeaderHeight);
            Rect toggleRect = new Rect(headerRect.xMax - 24f, headerRect.y, 24f, 24f);
            Rect refreshRect = new Rect(toggleRect.x - 26f, headerRect.y + 1f, 22f, 22f);
            Rect storedRect = new Rect(refreshRect.x - 26f, headerRect.y + 1f, 22f, 22f);
            Rect labelRect = new Rect(headerRect.x, headerRect.y + 2f, storedRect.x - headerRect.x - 4f, 20f);

            string selectedLabel = "Selected items: " + snapshot.TotalSelectedCount;
            if (snapshot.SelectedCountTruncated)
            {
                selectedLabel += "+ click refresh";
            }
            Widgets.Label(labelRect, selectedLabel);
            Texture2D storedTex = snapshot.ShowStoredItems ? SelectedItemsTextures.Box : SelectedItemsTextures.BoxOff;
            if (Widgets.ButtonImage(storedRect, storedTex))
            {
                snapshot.ShowStoredItems = !snapshot.ShowStoredItems;
                if (snapshot.ShowStoredItems && snapshot.TotalStoredCount > snapshot.Limit)
                {
                    snapshot.ForceFullList = true;
                    snapshot.Expanded = true;
                }
                StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, null);
            }
            TooltipHandler.TipRegion(storedRect, snapshot.ShowStoredItems ? "Hide items already stored here" : "Show items already stored here");

            if (Widgets.ButtonImage(refreshRect, SelectedItemsTextures.Refresh))
            {
                snapshot.ForceFullList = true;
                StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, null);
            }
            TooltipHandler.TipRegion(refreshRect, "Refresh selected item list");

            Texture2D toggleTex = snapshot.Expanded ? SelectedItemsTextures.ChevronUp : SelectedItemsTextures.ChevronDown;
            if (Widgets.ButtonImage(toggleRect, toggleTex))
            {
                snapshot.Expanded = !snapshot.Expanded;
                if (snapshot.Expanded && (snapshot.TotalSelectedCount > snapshot.Limit || snapshot.NeedsRefreshOnFirstExpand))
                {
                    snapshot.NeedsRefreshOnFirstExpand = false;
                    StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, null);
                }
                if (!snapshot.Expanded)
                {
                    snapshot.ForceFullList = false;
                    StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, null);
                }
            }
            TooltipHandler.TipRegion(toggleRect, snapshot.Expanded ? "Hide selected item list" : "Show selected item list");

            if (!snapshot.Expanded || snapshot.Rows.Count == 0)
            {
                return;
            }

            Rect rowsRect = new Rect(inner.x, inner.y + HeaderHeight, inner.width, inner.height - HeaderHeight);
            int scrollStartsAt = SelectedItemsMod.Settings?.ScrollStartsAt ?? 5;
            scrollStartsAt = Mathf.Clamp(scrollStartsAt, 1, 2000);
            float searchHeight = SearchHeightFor(snapshot);
            if (searchHeight > 0f)
            {
                rowsRect.height -= searchHeight;
            }

            if (snapshot.Rows.Count > scrollStartsAt)
            {
                Rect viewRect = new Rect(0f, 0f, rowsRect.width - 16f, snapshot.Rows.Count * RowHeight);
                Widgets.BeginScrollView(rowsRect, ref snapshot.ScrollPosition, viewRect);
                DrawRows(new Rect(0f, 0f, viewRect.width, viewRect.height), filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, snapshot);
                Widgets.EndScrollView();
            }
            else
            {
                DrawRows(rowsRect, filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, snapshot);
            }

            if (searchHeight > 0f)
            {
                DrawSearchRows(new Rect(rowsRect.x, rowsRect.yMax, rowsRect.width, searchHeight), filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, snapshot);
            }
        }

        private static float SearchHeightFor(SelectedItemSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.Expanded || snapshot.SearchRows.Count == 0)
            {
                return 0f;
            }
            return Padding + snapshot.SearchRows.Count * RowHeight + (snapshot.SearchResultsTruncated ? RowHeight : 0f);
        }

        private static void DrawSwitchArrows(Rect rect, ref Rect inner)
        {
            bool left = StorageNavigation.TryFindNeighbor(StorageNavigation.Direction.Left, out _);
            bool right = StorageNavigation.TryFindNeighbor(StorageNavigation.Direction.Right, out _);
            bool up = StorageNavigation.TryFindNeighbor(StorageNavigation.Direction.Up, out _);
            bool down = StorageNavigation.TryFindNeighbor(StorageNavigation.Direction.Down, out _);
            bool useLeftRail = left || (!right && (up || down));
            bool useRightRail = right || (!left && !useLeftRail && (up || down));

            if (useLeftRail)
            {
                inner.xMin += ArrowButtonSize + Padding;
            }
            if (useRightRail)
            {
                inner.xMax -= ArrowButtonSize + Padding;
            }

            float leftRailX = rect.x + Padding;
            float rightRailX = rect.xMax - Padding - ArrowButtonSize;
            if (left)
            {
                DrawArrowButton(new Rect(leftRailX, rect.center.y - ArrowButtonSize / 2f, ArrowButtonSize, ArrowButtonSize), "<", StorageNavigation.Direction.Left);
            }
            if (right)
            {
                DrawArrowButton(new Rect(rightRailX, rect.center.y - ArrowButtonSize / 2f, ArrowButtonSize, ArrowButtonSize), ">", StorageNavigation.Direction.Right);
            }
            if (up)
            {
                if (useLeftRail)
                {
                    DrawArrowButton(new Rect(leftRailX, rect.y + Padding, ArrowButtonSize, ArrowButtonSize), "^", StorageNavigation.Direction.Up);
                }
                if (useRightRail)
                {
                    DrawArrowButton(new Rect(rightRailX, rect.y + Padding, ArrowButtonSize, ArrowButtonSize), "^", StorageNavigation.Direction.Up);
                }
            }
            if (down)
            {
                if (useLeftRail)
                {
                    DrawArrowButton(new Rect(leftRailX, rect.yMax - Padding - ArrowButtonSize, ArrowButtonSize, ArrowButtonSize), "v", StorageNavigation.Direction.Down);
                }
                if (useRightRail)
                {
                    DrawArrowButton(new Rect(rightRailX, rect.yMax - Padding - ArrowButtonSize, ArrowButtonSize, ArrowButtonSize), "v", StorageNavigation.Direction.Down);
                }
            }
        }

        private static void DrawArrowButton(Rect rect, string label, StorageNavigation.Direction direction)
        {
            if (Widgets.ButtonText(rect, label))
            {
                StorageNavigation.SelectNeighbor(direction);
            }
            TooltipHandler.TipRegion(rect, "Select nearest matching storage");
        }

        private static void DrawRows(Rect rect, ThingFilter filter, ThingFilter parentFilter, System.Collections.Generic.IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, System.Collections.Generic.List<ThingDef> storedDefs, SelectedItemSnapshot snapshot)
        {
            Text.Font = GameFont.Small;
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                SelectedItemRow row = snapshot.Rows[i];
                float y = rect.y + i * RowHeight;
                DrawRow(new Rect(rect.x, y, rect.width, RowHeight), filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, snapshot, row);
            }
        }

        private static void DrawSearchRows(Rect rect, ThingFilter filter, ThingFilter parentFilter, System.Collections.Generic.IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, System.Collections.Generic.List<ThingDef> storedDefs, SelectedItemSnapshot snapshot)
        {
            Text.Font = GameFont.Tiny;
            float y = rect.y + Padding;
            for (int i = 0; i < snapshot.SearchRows.Count; i++)
            {
                DrawRow(new Rect(rect.x, y, rect.width, RowHeight), filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, snapshot, snapshot.SearchRows[i]);
                y += RowHeight;
            }
            if (snapshot.SearchResultsTruncated)
            {
                Widgets.Label(new Rect(rect.x + 26f, y + 2f, rect.width - 26f, 20f), "And there are more...");
            }
        }

        private static void DrawRow(Rect rowRect, ThingFilter filter, ThingFilter parentFilter, System.Collections.Generic.IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, System.Collections.Generic.List<ThingDef> storedDefs, SelectedItemSnapshot snapshot, SelectedItemRow row)
        {
            Text.Font = GameFont.Small;
            if (row == null)
            {
                return;
            }

            if (row.IsCategory)
            {
                DrawCategoryRow(rowRect, filter, parentFilter, forceHiddenDefs, displayRoot, storedDefs, snapshot, row);
                return;
            }

            ThingDef def = row.ThingDef;
            if (def == null)
            {
                return;
            }

            Rect iconRect = new Rect(rowRect.x + 2f, rowRect.y + 2f, 20f, 20f);
            Rect checkRect = new Rect(rowRect.xMax - 24f, rowRect.y, 24f, 24f);
            bool precisionControls = row.HasPrecisionControls;
            Rect labelRect = new Rect(iconRect.xMax + 4f, rowRect.y + 2f, rowRect.width - (precisionControls ? 142f : 54f), 20f);

            if (row.StoredHere)
                {
                    Widgets.DrawBoxSolid(rowRect, new Color(0.45f, 0.45f, 0.45f, 0.22f));
                }
            if (row.SearchResult)
            {
                Widgets.DrawBoxSolid(rowRect, new Color(0.25f, 0.35f, 0.45f, 0.16f));
            }
                if (Mouse.IsOver(rowRect))
                {
                    Widgets.DrawHighlight(rowRect);
                }
                if (def.uiIcon != null && def.uiIcon != BaseContent.BadTex)
                {
                    Widgets.DefIcon(iconRect, def, null, 1f, null, drawPlaceholder: true);
                }

            Widgets.Label(labelRect, def.LabelCap);
            TooltipHandler.TipRegion(rowRect, def.DescriptionDetailed);

            if (!precisionControls)
            {
                bool allowed = row.Allowed;
                bool changed = allowed;
                Widgets.Checkbox(checkRect.position, ref changed, 24f, disabled: false, paintable: true);
                if (changed != allowed)
                {
                    filter.SetAllow(def, changed);
                    row.Allowed = changed;
                }
            }
            else
            {
                PrecisionStockpileControlBridge.DrawThingDefControls(rowRect, checkRect, def);
            }
            PrecisionStockpileControlBridge.OpenThingDefContextMenu(rowRect, def);
        }

        private static void DrawCategoryRow(Rect rowRect, ThingFilter filter, ThingFilter parentFilter, System.Collections.Generic.IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, System.Collections.Generic.List<ThingDef> storedDefs, SelectedItemSnapshot snapshot, SelectedItemRow row)
        {
            Rect labelRect = new Rect(rowRect.x + 4f, rowRect.y + 2f, rowRect.width - 34f, 20f);
            Rect checkRect = new Rect(rowRect.xMax - 24f, rowRect.y, 24f, 24f);
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }

            Widgets.Label(labelRect, row.Label);
            TooltipHandler.TipRegion(rowRect, row.Description);
            bool precisionControls = row.HasPrecisionControls;
            if (!precisionControls)
            {
                MultiCheckboxState state = row.Allowed ? MultiCheckboxState.On : MultiCheckboxState.Off;
                MultiCheckboxState changed = Widgets.CheckboxMulti(checkRect, state, paintable: true);
                if (changed != state)
                {
                    filter.SetAllow(row.CategoryDef, changed == MultiCheckboxState.On);
                    row.Allowed = changed == MultiCheckboxState.On;
                }
            }
            else
            {
                PrecisionStockpileControlBridge.DrawCategoryControls(rowRect, checkRect, row.CategoryDef);
            }
            PrecisionStockpileControlBridge.OpenCategoryContextMenu(rowRect, row.CategoryDef);
        }
    }
}
