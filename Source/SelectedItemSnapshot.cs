using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SelectedItems
{
    public class SelectedItemSnapshot
    {
        public readonly List<ThingDef> Items = new List<ThingDef>();
        public readonly HashSet<ThingDef> StoredDefs = new HashSet<ThingDef>();
        public Vector2 ScrollPosition;
        public bool Expanded;
        public bool ForceFullList;
        public bool NeedsRefreshOnFirstExpand;
        public bool ShowStoredItems;
        public int Limit;
        public int TotalSelectedCount;
        public int TotalStoredCount;
        public int LastFrame;
    }
}
