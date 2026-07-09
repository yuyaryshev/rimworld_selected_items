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

        public SelectedItemsMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SelectedItemsSettings>();
            selectedLimitBuffer = Settings.SelectedLimit.ToString();
            scrollStartsAtBuffer = Settings.ScrollStartsAt.ToString();
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
