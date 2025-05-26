using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace TemporalHole
{
    [ProtoContract]
    public class TemporalHolePacket
    {
        [ProtoMember(1)] // the inventory the temporal hole is in
        public string holeInventory;

        [ProtoMember(2)] // the slot the temporal hole is in
        public int holeSlotId;

        [ProtoMember(3)] // the inventory the item to be stored is in
        public string sunkInventory;

        [ProtoMember(4)] // the slot the item to be stored is in
        public int sunkSlotId;
    }
    class TemporalHoleNetworking
    {
        // method to handle chucking the item through the hole
        public static void HandleHoleSink(IPlayer byPlayer, TemporalHolePacket holeSinkInfo)
        {
            // get the relevant inventories
            IInventory holeInv = byPlayer.InventoryManager.GetInventory(holeSinkInfo.holeInventory);
            IInventory sunkInv = byPlayer.InventoryManager.GetInventory(holeSinkInfo.sunkInventory);
            
            // error checking
            if(holeInv == null)
            {
                TemporalHoleModSystem.api.Logger.Error("Inventory containing hole does not exist");
                return;
            }
            if (sunkInv == null)
            {
                TemporalHoleModSystem.api.Logger.Error("Inventory containing sunk item does not exist");
                return;
            }

            // get the relevant item slots
            ItemSlot holeSlot = holeInv[holeSinkInfo.holeSlotId];
            ItemSlot sunkSlot = sunkInv[holeSinkInfo.sunkSlotId];

            // more error checking
            if (holeSlot == null)
            {
                TemporalHoleModSystem.api.Logger.Error("Slot containing hole does not exist");
                return;
            }
            if (sunkSlot == null)
            {
                TemporalHoleModSystem.api.Logger.Error("Slot containing sunk item does not exist");
                return;
            }

            // you guessed it, error checking
            if (holeSlot?.Itemstack?.Item is not ItemTemporalHole)
            {
                TemporalHoleModSystem.api.Logger.Error("Hole is not a Temporal Hole");
                return;
            }

            // call the method to chuck the item through the hole
            ItemTemporalHole.AddToHole(holeSlot, sunkSlot);
        }
    }
}
