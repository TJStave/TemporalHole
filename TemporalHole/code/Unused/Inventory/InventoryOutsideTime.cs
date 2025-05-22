using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

//namespace TemporalHole
//{
//    class InventoryOutsideTime : InventoryGeneric
//    {
//        ItemSlot theHole;

//        public InventoryOutsideTime(ICoreAPI api) : base(api)
//        {

//        }

//        public InventoryOutsideTime(int quantitySlots, string className, string instanceId, ItemSlot theHole, ICoreAPI api, NewSlotDelegate onNewSlot = null) : base(quantitySlots, className, instanceId, api, onNewSlot)
//        {
//            this.theHole = theHole;
//            this.SlotModified += OnSlotModified;
//        }

//        public InventoryOutsideTime(int quantitySlots, string invId, ItemSlot theHole, ICoreAPI api, NewSlotDelegate onNewSlot = null) : base(quantitySlots, invId, api, onNewSlot)
//        {
//            this.theHole = theHole;
//            this.SlotModified += OnSlotModified;
//        }

//        public void OnSlotModified(int slotid)
//        {
//            ItemSlot slot = this[slotid];
//            Api.Logger.Notification("Item consumed");
//            base.OnItemSlotModified(slot);
//            if (slot.Itemstack == null)
//            {
//                return;
//            }
//            string genKey = Convert.ToBase64String(BitConverter.GetBytes(Api.World.Rand.NextInt64())).Substring(0, 8);
//            while (theHole.Itemstack.Attributes.GetTreeAttribute("itemkeys").HasAttribute(genKey))
//            {
//                genKey = Convert.ToBase64String(BitConverter.GetBytes(Api.World.Rand.NextInt64())).Substring(0, 8);
//            }
//            theHole.Itemstack.Attributes.GetTreeAttribute("itemkeys").SetString(genKey, genKey);
//            theHole.Itemstack.Attributes.GetTreeAttribute("itemstacks").SetItemstack(genKey, slot.Itemstack);
//            slot.Itemstack = null;
//            slot.MarkDirty();
//            theHole.MarkDirty();
//        }
//    }
//}
