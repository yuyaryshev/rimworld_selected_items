using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SelectedItems
{
    public class SelectedItemSnapshot
    {
        public readonly List<ThingDef> Items = new List<ThingDef>();
        public readonly List<SelectedItemRow> Rows = new List<SelectedItemRow>();
        public readonly List<SelectedItemRow> SearchRows = new List<SelectedItemRow>();
        public readonly HashSet<ThingDef> StoredDefs = new HashSet<ThingDef>();
        public RimWorld.QuickSearchFilter SearchFilter;
        public string SearchText;
        public object ParentObject;
        public Vector2 ScrollPosition;
        public bool Expanded;
        public bool ForceFullList;
        public bool NeedsRefreshOnFirstExpand;
        public bool ShowStoredItems;
        public bool SearchResultsTruncated;
        public bool SelectedCountTruncated;
        public int Limit;
        public int TotalSelectedCount;
        public int TotalStoredCount;
        public int TotalSearchCount;
        public int LastFrame;
    }
}
