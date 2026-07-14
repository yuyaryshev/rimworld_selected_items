using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace SelectedItems
{
    public static class StorageFilterSelection
    {
        private static readonly ConditionalWeakTable<ThingFilter, SelectedItemSnapshot> Snapshots = new ConditionalWeakTable<ThingFilter, SelectedItemSnapshot>();
        private static bool expandNextSnapshot;

        public static void ExpandNextSnapshot()
        {
            expandNextSnapshot = true;
        }

        public static SelectedItemSnapshot SnapshotFor(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, List<ThingDef> storedDefs, QuickSearchFilter searchFilter)
        {
            int limit = SelectedItemsMod.Settings?.SelectedLimit ?? 5;
            limit = Clamp(limit, 1, 1000);
            int maxItems = limit * 2;
            int frame = UnityEngine.Time.frameCount;
            bool forceExpanded = expandNextSnapshot;
            expandNextSnapshot = false;
            bool hasSnapshot = Snapshots.TryGetValue(filter, out SelectedItemSnapshot snapshot);
            object currentParent = StorageTabContext.CurrentParent;
            if (hasSnapshot && snapshot.ParentObject == currentParent && snapshot.Limit == limit)
            {
                snapshot.SearchFilter = searchFilter;
                RefreshSearchRowsIfNeeded(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, searchFilter);
                if (forceExpanded)
                {
                    snapshot.Expanded = true;
                }
                snapshot.LastFrame = frame;
                return snapshot;
            }

            int scanLimit = hasSnapshot && snapshot.ForceFullList ? maxItems : maxItems;
            List<ThingDef> allowedVisible = AllowedVisibleDefs(filter, parentFilter, forceHiddenDefs, displayRoot, scanLimit, out bool selectedTruncated);
            storedDefs = VisibleStoredDefs(storedDefs, parentFilter, forceHiddenDefs, displayRoot);
            List<SelectedItemRow> searchRows = SearchRows(filter, parentFilter, forceHiddenDefs, displayRoot, searchFilter, out int totalSearchCount, out bool searchTruncated);

            Snapshots.Remove(filter);
            snapshot = new SelectedItemSnapshot
            {
                Expanded = forceExpanded || allowedVisible.Count <= limit,
                NeedsRefreshOnFirstExpand = allowedVisible.Count > limit,
                ShowStoredItems = storedDefs.Count <= limit,
                Limit = limit,
                TotalSelectedCount = allowedVisible.Count,
                TotalStoredCount = storedDefs.Count,
                TotalSearchCount = totalSearchCount,
                SearchResultsTruncated = searchTruncated,
                SelectedCountTruncated = selectedTruncated,
                SearchFilter = searchFilter,
                SearchText = searchFilter?.Text ?? string.Empty,
                ParentObject = currentParent,
                LastFrame = frame
            };
            RefreshItems(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, allowedVisible, storedDefs, searchRows, maxItems);
            Snapshots.Add(filter, snapshot);
            return snapshot;
        }

        public static void Refresh(SelectedItemSnapshot snapshot, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, List<ThingDef> storedDefs, QuickSearchFilter searchFilter)
        {
            searchFilter = searchFilter ?? snapshot.SearchFilter;
            int limit = SelectedItemsMod.Settings?.SelectedLimit ?? 5;
            limit = Clamp(limit, 1, 1000);
            int maxItems = limit * 2;
            int scanLimit = maxItems;
            List<ThingDef> allowedVisible = AllowedVisibleDefs(filter, parentFilter, forceHiddenDefs, displayRoot, scanLimit, out bool selectedTruncated);
            storedDefs = VisibleStoredDefs(storedDefs, parentFilter, forceHiddenDefs, displayRoot);
            List<SelectedItemRow> searchRows = SearchRows(filter, parentFilter, forceHiddenDefs, displayRoot, searchFilter, out int totalSearchCount, out bool searchTruncated);
            snapshot.TotalSelectedCount = allowedVisible.Count;
            snapshot.TotalStoredCount = storedDefs.Count;
            snapshot.TotalSearchCount = totalSearchCount;
            snapshot.SearchResultsTruncated = searchTruncated;
            snapshot.SelectedCountTruncated = selectedTruncated;
            snapshot.Limit = limit;
            snapshot.ParentObject = StorageTabContext.CurrentParent;
            if (allowedVisible.Count <= limit)
            {
                snapshot.ForceFullList = false;
                snapshot.NeedsRefreshOnFirstExpand = false;
            }
            RefreshItems(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, allowedVisible, snapshot.ShowStoredItems ? storedDefs : null, searchRows, maxItems);
        }

        public static void RefreshAfterFilterChange(SelectedItemSnapshot snapshot, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, List<ThingDef> storedDefs)
        {
            if (snapshot == null)
            {
                return;
            }

            RefreshSearchRowsIfNeeded(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot, snapshot.SearchFilter);
        }

        private static void RefreshItems(SelectedItemSnapshot snapshot, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, List<ThingDef> allowedVisible, List<ThingDef> storedDefs, List<SelectedItemRow> searchRows, int maxItems)
        {
            snapshot.Items.Clear();
            snapshot.Rows.Clear();
            snapshot.SearchRows.Clear();
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
            RebuildRows(snapshot, filter, parentFilter, forceHiddenDefs, displayRoot);
            snapshot.SearchRows.AddRange(searchRows);
        }

        private static void RebuildRows(SelectedItemSnapshot snapshot, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            snapshot.Rows.Clear();
            HashSet<ThingDef> covered = new HashSet<ThingDef>();
            if (SelectedItemsMod.Settings?.ShowHighLevelTreeItems == true)
            {
                foreach (TreeNode_ThingCategory node in FullCategoryRows(filter, parentFilter, forceHiddenDefs, displayRoot))
                {
                    snapshot.Rows.Add(new SelectedItemRow
                    {
                        CategoryDef = node.catDef,
                        Allowed = true,
                        HasPrecisionControls = PrecisionStockpileControlBridge.HasCategoryControls(node.catDef)
                    });
                    foreach (ThingDef def in VisibleDescendants(node.catDef, parentFilter, forceHiddenDefs, displayRoot))
                    {
                        covered.Add(def);
                    }
                }
            }

            foreach (ThingDef def in snapshot.Items)
            {
                if (covered.Contains(def))
                {
                    continue;
                }
                snapshot.Rows.Add(new SelectedItemRow
                {
                    ThingDef = def,
                    StoredHere = snapshot.StoredDefs.Contains(def),
                    Allowed = filter.Allows(def),
                    HasPrecisionControls = PrecisionStockpileControlBridge.HasThingDefControls(def)
                });
            }
        }

        private static IEnumerable<TreeNode_ThingCategory> FullCategoryRows(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            if (displayRoot == null)
            {
                yield break;
            }
            foreach (TreeNode_ThingCategory child in displayRoot.ChildCategoryNodes)
            {
                foreach (TreeNode_ThingCategory full in FullCategoryRowsRecursive(child, filter, parentFilter, forceHiddenDefs, displayRoot))
                {
                    yield return full;
                }
            }
        }

        private static IEnumerable<TreeNode_ThingCategory> FullCategoryRowsRecursive(TreeNode_ThingCategory node, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            List<ThingDef> descendants = VisibleDescendants(node.catDef, parentFilter, forceHiddenDefs, displayRoot).ToList();
            if (descendants.Count > 1 && descendants.All(filter.Allows))
            {
                yield return node;
                yield break;
            }

            foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
            {
                foreach (TreeNode_ThingCategory full in FullCategoryRowsRecursive(child, filter, parentFilter, forceHiddenDefs, displayRoot))
                {
                    yield return full;
                }
            }
        }

        private static List<SelectedItemRow> SearchRows(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, QuickSearchFilter searchFilter, out int total, out bool truncated)
        {
            total = 0;
            truncated = false;
            List<SelectedItemRow> rows = new List<SelectedItemRow>();
            if (SelectedItemsMod.Settings?.ShowSearchResults != true || searchFilter == null || !searchFilter.Active)
            {
                return rows;
            }

            foreach (ThingDef def in VisibleDescendants(displayRoot?.catDef, parentFilter, forceHiddenDefs, displayRoot))
            {
                if (!searchFilter.Matches(def))
                {
                    continue;
                }
                total++;
                if (rows.Count < 3)
                {
                    rows.Add(new SelectedItemRow
                    {
                        ThingDef = def,
                        SearchResult = true,
                        Allowed = filter != null && filter.Allows(def),
                        HasPrecisionControls = PrecisionStockpileControlBridge.HasThingDefControls(def)
                    });
                }
                else
                {
                    truncated = true;
                    break;
                }
            }
            return rows;
        }

        private static void RefreshSearchRowsIfNeeded(SelectedItemSnapshot snapshot, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, QuickSearchFilter searchFilter)
        {
            searchFilter = searchFilter ?? snapshot.SearchFilter;
            string searchText = searchFilter?.Text ?? string.Empty;
            if (snapshot.SearchText == searchText)
            {
                return;
            }
            snapshot.SearchFilter = searchFilter;
            snapshot.SearchText = searchText;
            List<SelectedItemRow> searchRows = SearchRows(filter, parentFilter, forceHiddenDefs, displayRoot, searchFilter, out int totalSearchCount, out bool searchTruncated);
            snapshot.TotalSearchCount = totalSearchCount;
            snapshot.SearchResultsTruncated = searchTruncated;
            snapshot.SearchRows.Clear();
            snapshot.SearchRows.AddRange(searchRows);
        }

        private static List<ThingDef> AllowedVisibleDefs(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot, int maxItems, out bool truncated)
        {
            truncated = false;
            List<ThingDef> result = new List<ThingDef>();
            foreach (ThingDef def in filter.AllowedThingDefs)
            {
                if (!Visible(def, parentFilter, forceHiddenDefs, displayRoot))
                {
                    continue;
                }
                if (result.Count >= maxItems)
                {
                    truncated = true;
                    break;
                }
                result.Add(def);
            }
            return result;
        }

        private static List<ThingDef> VisibleStoredDefs(List<ThingDef> storedDefs, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            if (storedDefs == null)
            {
                return new List<ThingDef>();
            }
            List<ThingDef> result = new List<ThingDef>();
            int maxItems = (SelectedItemsMod.Settings?.SelectedLimit ?? 5) * 2;
            foreach (ThingDef def in storedDefs)
            {
                if (!Visible(def, parentFilter, forceHiddenDefs, displayRoot))
                {
                    continue;
                }
                result.Add(def);
                if (result.Count >= maxItems)
                {
                    break;
                }
            }
            return result;
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
            return result;
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

        private static IEnumerable<ThingDef> VisibleDescendants(ThingCategoryDef categoryDef, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, TreeNode_ThingCategory displayRoot)
        {
            if (categoryDef == null)
            {
                yield break;
            }
            foreach (ThingDef def in categoryDef.DescendantThingDefs)
            {
                if (Visible(def, parentFilter, forceHiddenDefs, displayRoot))
                {
                    yield return def;
                }
            }
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
