using Verse;

namespace SelectedItems
{
    public class SelectedItemRow
    {
        public ThingDef ThingDef;
        public ThingCategoryDef CategoryDef;
        public bool StoredHere;
        public bool SearchResult;
        public bool Allowed;
        public bool HasPrecisionControls;

        public bool IsCategory => CategoryDef != null;

        public string Label
        {
            get
            {
                if (CategoryDef != null)
                {
                    return CategoryDef.LabelCap + " (All)";
                }
                return ThingDef?.LabelCap ?? string.Empty;
            }
        }

        public string Description
        {
            get
            {
                if (CategoryDef != null)
                {
                    return CategoryDef.description;
                }
                return ThingDef?.DescriptionDetailed ?? string.Empty;
            }
        }
    }
}
