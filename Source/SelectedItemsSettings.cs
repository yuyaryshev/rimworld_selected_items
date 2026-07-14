using Verse;

namespace SelectedItems
{
    public class SelectedItemsSettings : ModSettings
    {
        public bool DisableModRuntime;
        public bool OpenStorageTabOnSelect = true;
        public bool AddStockpileSwitchArrows = true;
        public bool DrawSelectedItemsBelowStorageFilters = true;
        public bool IntegratePrecisionStockpileControlRendering = true;
        public bool ShowHighLevelTreeItems = true;
        public bool ShowSearchResults = true;
        public int UiGapTop;
        public int UiGapBottom;
        public int SelectedLimit = 5;
        public int ScrollStartsAt = 5;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref DisableModRuntime, "disableModRuntime", false);
            Scribe_Values.Look(ref OpenStorageTabOnSelect, "openStorageTabOnSelect", true);
            Scribe_Values.Look(ref AddStockpileSwitchArrows, "addStockpileSwitchArrows", true);
            Scribe_Values.Look(ref DrawSelectedItemsBelowStorageFilters, "drawSelectedItemsBelowStorageFilters", true);
            Scribe_Values.Look(ref IntegratePrecisionStockpileControlRendering, "integratePrecisionStockpileControlRendering", true);
            Scribe_Values.Look(ref ShowHighLevelTreeItems, "showHighLevelTreeItems", true);
            Scribe_Values.Look(ref ShowSearchResults, "showSearchResults", true);
            Scribe_Values.Look(ref UiGapTop, "uiGapTop", 0);
            Scribe_Values.Look(ref UiGapBottom, "uiGapBottom", 0);
            Scribe_Values.Look(ref SelectedLimit, "selectedLimit", 5);
            Scribe_Values.Look(ref ScrollStartsAt, "scrollStartsAt", 5);
            UiGapTop = Clamp(UiGapTop, 0, 500);
            UiGapBottom = Clamp(UiGapBottom, 0, 500);
            SelectedLimit = Clamp(SelectedLimit, 1, 1000);
            ScrollStartsAt = Clamp(ScrollStartsAt, 1, 2000);
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
