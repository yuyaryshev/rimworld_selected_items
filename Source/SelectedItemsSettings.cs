using Verse;

namespace SelectedItems
{
    public class SelectedItemsSettings : ModSettings
    {
        public int SelectedLimit = 5;
        public int ScrollStartsAt = 5;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SelectedLimit, "selectedLimit", 5);
            Scribe_Values.Look(ref ScrollStartsAt, "scrollStartsAt", 5);
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
