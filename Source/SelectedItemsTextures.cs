using UnityEngine;
using Verse;

namespace SelectedItems
{
    [StaticConstructorOnStartup]
    public static class SelectedItemsTextures
    {
        public static readonly Texture2D Refresh = ContentFinder<Texture2D>.Get("SelectedItems/Refresh");
        public static readonly Texture2D ChevronUp = ContentFinder<Texture2D>.Get("SelectedItems/ChevronUp");
        public static readonly Texture2D ChevronDown = ContentFinder<Texture2D>.Get("SelectedItems/ChevronDown");
        public static readonly Texture2D Box = ContentFinder<Texture2D>.Get("SelectedItems/Box");
        public static readonly Texture2D BoxOff = ContentFinder<Texture2D>.Get("SelectedItems/BoxOff");
    }
}
