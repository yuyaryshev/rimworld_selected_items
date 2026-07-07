using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SelectedItems
{
    public class SelectedItemSnapshot
    {
        public readonly List<ThingDef> Items = new List<ThingDef>();
        public Vector2 ScrollPosition;
        public int Limit;
        public int LastFrame;
    }
}
