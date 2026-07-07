using UnityEngine;
using Verse;

namespace SelectedItems
{
    public static class SelectedItemsPanel
    {
        private const float RowHeight = 24f;
        private const float HeaderHeight = 24f;
        private const float Padding = 3f;

        public static float HeightFor(SelectedItemSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 0f;
            }
            if (!snapshot.Expanded || snapshot.Items.Count == 0)
            {
                return HeaderHeight + Padding * 2f;
            }

            int scrollStartsAt = SelectedItemsMod.Settings?.ScrollStartsAt ?? 5;
            scrollStartsAt = Mathf.Clamp(scrollStartsAt, 1, 2000);
            int visibleRows = snapshot.Items.Count > scrollStartsAt ? scrollStartsAt : snapshot.Items.Count;
            return HeaderHeight + Padding + visibleRows * RowHeight + Padding;
        }

        public static void Draw(Rect rect, ThingFilter filter, ThingFilter parentFilter, System.Collections.Generic.IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, SelectedItemSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            Widgets.DrawMenuSection(rect);
            Rect inner = rect.ContractedBy(Padding);
            Text.Font = GameFont.Tiny;
            Rect headerRect = new Rect(inner.x, inner.y, inner.width, HeaderHeight);
            Rect toggleRect = new Rect(headerRect.xMax - 24f, headerRect.y, 24f, 24f);
            Rect refreshRect = new Rect(toggleRect.x - 26f, headerRect.y + 1f, 22f, 22f);
            Rect labelRect = new Rect(headerRect.x, headerRect.y + 2f, refreshRect.x - headerRect.x - 4f, 20f);

            Widgets.Label(labelRect, "Selected items: " + snapshot.TotalSelectedCount);
            if (Widgets.ButtonImage(refreshRect, TexButton.Reload))
            {
                StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot);
            }
            TooltipHandler.TipRegion(refreshRect, "Refresh selected item list");

            Texture2D toggleTex = snapshot.Expanded ? TexButton.ReorderUp : TexButton.ReorderDown;
            if (Widgets.ButtonImage(toggleRect, toggleTex))
            {
                snapshot.Expanded = !snapshot.Expanded;
                if (snapshot.Expanded && snapshot.TotalSelectedCount > snapshot.Limit)
                {
                    snapshot.ForceFullList = true;
                    StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot);
                }
                if (!snapshot.Expanded)
                {
                    snapshot.ForceFullList = false;
                    StorageFilterSelection.Refresh(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot);
                }
            }
            TooltipHandler.TipRegion(toggleRect, snapshot.Expanded ? "Hide selected item list" : "Show selected item list");

            if (!snapshot.Expanded || snapshot.Items.Count == 0)
            {
                return;
            }

            Rect rowsRect = new Rect(inner.x, inner.y + HeaderHeight, inner.width, inner.height - HeaderHeight);
            int scrollStartsAt = SelectedItemsMod.Settings?.ScrollStartsAt ?? 5;
            scrollStartsAt = Mathf.Clamp(scrollStartsAt, 1, 2000);

            if (snapshot.Items.Count > scrollStartsAt)
            {
                Rect viewRect = new Rect(0f, 0f, rowsRect.width - 16f, snapshot.Items.Count * RowHeight);
                Widgets.BeginScrollView(rowsRect, ref snapshot.ScrollPosition, viewRect);
                DrawRows(new Rect(0f, 0f, viewRect.width, viewRect.height), filter, snapshot);
                Widgets.EndScrollView();
            }
            else
            {
                DrawRows(rowsRect, filter, snapshot);
            }
        }

        private static void DrawRows(Rect rect, ThingFilter filter, SelectedItemSnapshot snapshot)
        {
            Text.Font = GameFont.Small;
            for (int i = 0; i < snapshot.Items.Count; i++)
            {
                ThingDef def = snapshot.Items[i];
                float y = rect.y + i * RowHeight;
                Rect rowRect = new Rect(rect.x, y, rect.width, RowHeight);
                Rect iconRect = new Rect(rowRect.x + 2f, rowRect.y + 2f, 20f, 20f);
                Rect labelRect = new Rect(iconRect.xMax + 4f, rowRect.y + 2f, rowRect.width - 54f, 20f);
                Rect checkRect = new Rect(rowRect.xMax - 24f, rowRect.y, 24f, 24f);

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

                bool allowed = filter.Allows(def);
                bool changed = allowed;
                Widgets.Checkbox(checkRect.position, ref changed, 24f, disabled: false, paintable: true);
                if (changed != allowed)
                {
                    filter.SetAllow(def, changed);
                }
            }
        }
    }
}
