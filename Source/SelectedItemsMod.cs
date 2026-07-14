using HarmonyLib;
using UnityEngine;
using Verse;

namespace SelectedItems
{
    public class SelectedItemsMod : Mod
    {
        public static SelectedItemsSettings Settings;

        private string selectedLimitBuffer;
        private string scrollStartsAtBuffer;
        private string uiGapTopBuffer;
        private string uiGapBottomBuffer;

        public SelectedItemsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SelectedItemsSettings>();
            selectedLimitBuffer = Settings.SelectedLimit.ToString();
            scrollStartsAtBuffer = Settings.ScrollStartsAt.ToString();
            uiGapTopBuffer = Settings.UiGapTop.ToString();
            uiGapBottomBuffer = Settings.UiGapBottom.ToString();
            new Harmony("yuyaryshev.selecteditems").PatchAll();
        }

        public override string SettingsCategory()
        {
            return "Selected Items";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            bool disableModRuntime = Settings.DisableModRuntime;
            listing.CheckboxLabeled("Disable Mod Runtime", ref disableModRuntime, "Disable all Selected Items runtime UI changes without unloading the mod.");
            Settings.DisableModRuntime = disableModRuntime;

            listing.GapLine();

            bool openStorageTabOnSelect = Settings.OpenStorageTabOnSelect;
            listing.CheckboxLabeled("Open Storage tab when selecting storage", ref openStorageTabOnSelect, "When selecting a stockpile, shelf, or storage container, open its Storage tab once. Manual tab changes are not forced afterward.");
            Settings.OpenStorageTabOnSelect = openStorageTabOnSelect;

            bool addStockpileSwitchArrows = Settings.AddStockpileSwitchArrows;
            listing.CheckboxLabeled("Add arrows to switch nearest stockpiles", ref addStockpileSwitchArrows, "Show edge arrows in the Selected Items panel when another matching stockpile or shelf is within 3 cells.");
            Settings.AddStockpileSwitchArrows = addStockpileSwitchArrows;

            bool drawSelectedItemsBelowStorageFilters = Settings.DrawSelectedItemsBelowStorageFilters;
            listing.CheckboxLabeled("Draw Selected Items below storage filters", ref drawSelectedItemsBelowStorageFilters, "Move the Selected Items panel below the vanilla storage filter tree. Disable this to draw it above the filters.");
            Settings.DrawSelectedItemsBelowStorageFilters = drawSelectedItemsBelowStorageFilters;

            bool integratePrecisionStockpileControlRendering = Settings.IntegratePrecisionStockpileControlRendering;
            listing.CheckboxLabeled("Integrate Precise Stockpile Control rendering", ref integratePrecisionStockpileControlRendering, "When Precision Stockpile Control is loaded, show its limit markers on Selected Items rows and allow its right-click edit menu.");
            Settings.IntegratePrecisionStockpileControlRendering = integratePrecisionStockpileControlRendering;

            bool showHighLevelTreeItems = Settings.ShowHighLevelTreeItems;
            listing.CheckboxLabeled("Show high level tree items", ref showHighLevelTreeItems, "Collapse fully selected storage categories into one category row instead of listing every item in that category.");
            Settings.ShowHighLevelTreeItems = showHighLevelTreeItems;

            bool showSearchResults = Settings.ShowSearchResults;
            listing.CheckboxLabeled("Show search results", ref showSearchResults, "When the Storage search box has matches, show up to three matching items below the Selected Items list.");
            Settings.ShowSearchResults = showSearchResults;

            listing.GapLine();

            int uiGapTop = Settings.UiGapTop;
            listing.TextFieldNumericLabeled("UI Gap top", ref uiGapTop, ref uiGapTopBuffer, 0f, 500f);
            Settings.UiGapTop = Mathf.Clamp(uiGapTop, 0, 500);

            int uiGapBottom = Settings.UiGapBottom;
            listing.TextFieldNumericLabeled("UI Gap bottom", ref uiGapBottom, ref uiGapBottomBuffer, 0f, 500f);
            Settings.UiGapBottom = Mathf.Clamp(uiGapBottom, 0, 500);

            int selectedLimit = Settings.SelectedLimit;
            listing.TextFieldNumericLabeled("Show selected list when selected item count is at most", ref selectedLimit, ref selectedLimitBuffer, 1f, 1000f);
            Settings.SelectedLimit = Mathf.Clamp(selectedLimit, 1, 1000);

            int scrollStartsAt = Settings.ScrollStartsAt;
            listing.TextFieldNumericLabeled("Start selected list scrollbar when item count is greater than", ref scrollStartsAt, ref scrollStartsAtBuffer, 1f, 2000f);
            Settings.ScrollStartsAt = Mathf.Clamp(scrollStartsAt, 1, 2000);

            listing.GapLine();
            listing.Label("Changes apply immediately. Existing open Storage tabs refresh when reopened or after a short idle gap.");

            listing.End();
            Settings.Write();
        }

        public static bool RuntimeEnabled()
        {
            return Settings == null || !Settings.DisableModRuntime;
        }
    }
}
