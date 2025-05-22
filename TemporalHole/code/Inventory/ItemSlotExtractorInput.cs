using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vintagestory.API.Common;

namespace TemporalHole
{
    class ItemSlotExtractorInput : ItemSlot
    {
        public ItemSlotExtractorInput(InventoryBase inventory) : base(inventory)
        {
            this.inventory = inventory;
        }

        public override bool CanHold(ItemSlot sourceSlot)
        {
            return sourceSlot.IsTimeHole() && base.CanHold(sourceSlot);
        }
    }

    public static class SlotExtension
    {
        public static bool IsTimeHole(this ItemSlot slot) => slot?.Itemstack?.Collectible?.FirstCodePart() == "temporalhole";
    }
}
