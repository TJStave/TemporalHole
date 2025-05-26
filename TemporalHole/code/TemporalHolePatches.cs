using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace TemporalHole
{
    [HarmonyPatch(typeof(InventoryBase))]
    [HarmonyPatchCategory(TemporalHoleModSystem.TemporalHoleInventoryPatchCategory)]
    public class TemporalHolePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(InventoryBase.ActivateSlot)), HarmonyPriority(Priority.HigherThanNormal)]
        public static bool TemporalHoleActivateSlotPrefix(InventoryBase __instance, object __result, int slotId, ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            // if not holding control, skip prefix and run base method
            if (!op.CtrlDown) return true;
            // don't suck up items from invalid inventories
            if (__instance is InventoryPlayerCreative || __instance is DummyInventory) return true;

            // only run this code if exactly one of the two slots has a temporal hole
            // don't want any recursive pocket dimension shenanigans
            if (!__instance[slotId].Empty && !sourceSlot.Empty
                && (__instance[slotId].Itemstack?.Collectible?.FirstCodePart() == "temporalhole" ^ sourceSlot?.Itemstack?.Collectible?.FirstCodePart() == "temporalhole"))
            {
                // stop the game from sending a fake packet
                __result = null;

                ItemSlot holeSlot;
                ItemSlot sunkSlot;
                // figure out which slot has the hole in it
                if (__instance[slotId].Itemstack?.Collectible?.FirstCodePart() == "temporalhole")
                {
                    holeSlot = __instance[slotId];
                    sunkSlot = sourceSlot;
                }
                else
                {
                    holeSlot = sourceSlot;
                    sunkSlot = __instance[slotId];
                }

                // tell the server to chuck the item through the hole
                TemporalHoleModSystem.clientChannel.SendPacket(new TemporalHolePacket()
                {
                    holeInventory = holeSlot.Inventory.InventoryID,
                    holeSlotId = holeSlot.Inventory.GetSlotId(holeSlot),
                    sunkInventory = sunkSlot.Inventory.InventoryID,
                    sunkSlotId = sunkSlot.Inventory.GetSlotId(sunkSlot)
                });

                // this is to prevent stuttering while the client catches up with the server
                ItemTemporalHole.AddToHole(holeSlot, sunkSlot);

                TemporalHoleModSystem.api.Logger.Event("finished trigger");
                return false;
            }
            return true;
        }
    }
}
