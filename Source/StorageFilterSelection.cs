using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace SelectedItems
{
    public static class StorageFilterSelection
    {
        private const int ReopenFrameGap = 30;
        private static readonly ConditionalWeakTable<ThingFilter, SelectedItemSnapshot> Snapshots = new ConditionalWeakTable<ThingFilter, SelectedItemSnapshot>();

        public static SelectedItemSnapshot SnapshotFor(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, List<ThingDef> storedDefs)
        {
            int limit = SelectedItemsMod.Settings?.SelectedLimit ?? 5;
            limit = Clamp(limit, 1, 1000);
            int maxItems = limit * 2;
            int frame = UnityEngine.Time.frameCount;
            List<ThingDef> allowedVisible = AllowedVisibleDefs(filter, parentFilter, forceHiddenDefs, displayRoot);
            storedDefs = VisibleStoredDefs(storedDefs, parentFilter, forceHiddenDefs, displayRoot);

            if (!Snapshots.TryGetValue(filter, out SelectedItemSnapshot snapshot) || frame - snapshot.LastFrame > ReopenFrameGap || snapshot.Limit != limit)
            {
                Snapshots.Remove(filter);
                snapshot = new SelectedItemSnapshot
                {
                    Expanded = allowedVisible.Count <= limit,
                    ShowStoredItems = storedDefs.Count <= limit,
                    Limit = limit,
                    TotalSelectedCount = allowedVisible.Count,
                    TotalStoredCount = storedDefs.Count,
                    LastFrame = frame
                };
                RefreshItems(snapshot, allowedVisible, storedDefs, maxItems);
                Snapshots.Add(filter, snapshot);
                return snapshot;
            }

            snapshot.LastFrame = frame;
            snapshot.TotalSelectedCount = allowedVisible.Count;
            snapshot.TotalStoredCount = storedDefs.Count;
            snapshot.StoredDefs.Clear();
            foreach (ThingDef storedDef in storedDefs)
            {
                snapshot.StoredDefs.Add(storedDef);
            }
            snapshot.Items.RemoveAll(def => def == null || !Visible(def, parentFilter, forceHiddenDefs, displayRoot));
            foreach (ThingDef def in CombinedDefs(allowedVisible, snapshot.ShowStoredItems ? storedDefs : null))
            {
                if (!snapshot.ForceFullList && snapshot.Items.Count >= maxItems)
                {
                    break;
                }
                if (!snapshot.Items.Contains(def))
                {
                    snapshot.Items.Add(def);
                }
            }

            return snapshot;
        }

        public static void Refresh(SelectedItemSnapshot snapshot, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, List<ThingDef> storedDefs)
        {
            int limit = SelectedItemsMod.Settings?.SelectedLimit ?? 5;
            limit = Clamp(limit, 1, 1000);
            int maxItems = limit * 2;
            List<ThingDef> allowedVisible = AllowedVisibleDefs(filter, parentFilter, forceHiddenDefs, displayRoot);
            storedDefs = VisibleStoredDefs(storedDefs, parentFilter, forceHiddenDefs, displayRoot);
            snapshot.TotalSelectedCount = allowedVisible.Count;
            snapshot.TotalStoredCount = storedDefs.Count;
            snapshot.Limit = limit;
            RefreshItems(snapshot, allowedVisible, snapshot.ShowStoredItems ? storedDefs : null, maxItems);
        }

        private static void RefreshItems(SelectedItemSnapshot snapshot, List<ThingDef> allowedVisible, List<ThingDef> storedDefs, int maxItems)
        {
            snapshot.Items.Clear();
            snapshot.StoredDefs.Clear();
            if (storedDefs != null)
            {
                foreach (ThingDef storedDef in storedDefs)
                {
                    snapshot.StoredDefs.Add(storedDef);
                }
            }

            List<ThingDef> combined = CombinedDefs(allowedVisible, storedDefs);
            if (snapshot.ForceFullList)
            {
                snapshot.Items.AddRange(combined);
            }
            else
            {
                snapshot.Items.AddRange(combined.Take(maxItems));
            }
        }

        private static List<ThingDef> AllowedVisibleDefs(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            return filter.AllowedThingDefs
                .Where(def => Visible(def, parentFilter, forceHiddenDefs, displayRoot))
                .OrderBy(def => def.label)
                .ToList();
        }

        private static List<ThingDef> VisibleStoredDefs(List<ThingDef> storedDefs, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            if (storedDefs == null)
            {
                return new List<ThingDef>();
            }
            return storedDefs
                .Where(def => Visible(def, parentFilter, forceHiddenDefs, displayRoot))
                .OrderBy(def => def.label)
                .ToList();
        }

        private static List<ThingDef> CombinedDefs(List<ThingDef> allowedVisible, List<ThingDef> storedDefs)
        {
            List<ThingDef> result = new List<ThingDef>();
            foreach (ThingDef def in allowedVisible)
            {
                if (!result.Contains(def))
                {
                    result.Add(def);
                }
            }
            if (storedDefs != null)
            {
                foreach (ThingDef def in storedDefs)
                {
                    if (!result.Contains(def))
                    {
                        result.Add(def);
                    }
                }
            }
            return result.OrderBy(def => def.label).ToList();
        }

        private static bool Visible(ThingDef def, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            if (def == null || !def.PlayerAcquirable || def.virtualDefParent != null)
            {
                return false;
            }
            if (displayRoot != null && !displayRoot.catDef.DescendantThingDefs.Contains(def))
            {
                return false;
            }
            if (forceHiddenDefs != null && forceHiddenDefs.Contains(def))
            {
                return false;
            }
            if (parentFilter != null)
            {
                if (!parentFilter.Allows(def))
                {
                    return false;
                }
                if (parentFilter.IsAlwaysDisallowedDueToSpecialFilters(def))
                {
                    return false;
                }
            }
            return true;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }
}
