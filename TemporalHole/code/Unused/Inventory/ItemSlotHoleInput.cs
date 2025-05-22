using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

//namespace TemporalHole
//{
//    class ItemSlotHoleInput : ItemSlot
//    {
//        public ItemSlotHoleInput(InventoryBase inventory) : base(inventory)
//        {
//            this.inventory = inventory;
//        }

//        public override bool CanHold(ItemSlot sourceSlot)
//        {
//            return sourceSlot.RecursionCheck() && base.CanHold(sourceSlot);
//        }
//    }

//    public static class SlotExtension
//    {
//        public static bool RecursionCheck(this ItemSlot slot) => slot?.Itemstack?.Collectible?.FirstCodePart() != "temporalhole";
//    }
//}
