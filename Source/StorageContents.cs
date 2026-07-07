using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SelectedItems
{
    public static class StorageContents
    {
        public static List<ThingDef> StoredDefs(IStoreSettingsParent parent)
        {
            IEnumerable<Thing> heldThings = null;

            if (parent is StorageGroup storageGroup)
            {
                heldThings = storageGroup.HeldThings;
            }
            else if (parent is ISlotGroupParent slotGroupParent)
            {
                heldThings = slotGroupParent.GetSlotGroup()?.HeldThings;
            }

            if (heldThings == null)
            {
                return new List<ThingDef>();
            }

            return heldThings
                .Select(thing => thing?.def)
                .Where(def => def != null && def.PlayerAcquirable && def.virtualDefParent == null)
                .Distinct()
                .OrderBy(def => def.label)
                .ToList();
        }
    }
}
