using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace TemporalHole
{
    class ItemTemporalHole : Item
    {
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            int? itemCount = inSlot?.Itemstack?.Attributes?.GetTreeAttribute("itemkeys")?.Count;
            if (itemCount > 0)
            {
                dsc.AppendLine("");
                dsc.Append(Lang.Get("temporalhole:holecontents", itemCount));
            }
        }

        // adds item in sourceSlot to Temporal Hole in holeSlot
        public static void AddToHole(ItemSlot holeSlot, ItemSlot sourceSlot)
        {
            if (!sourceSlot.CanTake())
            {
                TemporalHoleModSystem.api.Logger.Notification("Temporal hole attempted to take {0} from a locked slot", sourceSlot.Itemstack.ToString());
                return;
            }
            string genKey = GenerateKey(holeSlot.Itemstack.Attributes.GetOrAddTreeAttribute("itemkeys"));

            ItemStack sourceStack = sourceSlot.TakeOutWhole();
            AddItemToAttributes(genKey, sourceStack, holeSlot);
        }

        // generates a 8 character long key, and checks to make sure it's unique
        private static string GenerateKey(ITreeAttribute keys)
        {
            string genKey = Convert.ToBase64String(BitConverter.GetBytes(TemporalHoleModSystem.api.World.Rand.NextInt64()))[..8];
            while (keys.HasAttribute(genKey))
            {
                genKey = Convert.ToBase64String(BitConverter.GetBytes(TemporalHoleModSystem.api.World.Rand.NextInt64()))[..8];
            }

            return genKey;
        }

        // adds the item to the holes attributes
        private static void AddItemToAttributes(string genKey, ItemStack sourceStack, ItemSlot holeSlot)
        {
            ITreeAttribute holeAttrs = holeSlot.Itemstack.Attributes;
            holeAttrs.GetOrAddTreeAttribute("itemkeys").SetString(genKey, genKey);
            // item code is stored seperately from the stacks to ensure the correct item is extracted after export and importing schematics
            holeAttrs.GetOrAddTreeAttribute("itemcodes").SetString(genKey, sourceStack.Collectible?.Code.ToString());
            // Item stacks are stored as bytes because for some reason ItemStackAttributes seem to get erased out of nowhere (on v1.20.9)
            holeAttrs.GetOrAddTreeAttribute("itemstacks").SetBytes(genKey, sourceStack.ToBytes());
            holeSlot.MarkDirty();
        }

        /////////////////////////////////////////////////////////////////////////////////
        // redundant code from before using harmony patches for inventory interactions //
        /////////////////////////////////////////////////////////////////////////////////

        //// This enables dropping items onto the hole
        //public override int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack, EnumMergePriority priority)
        //{
        //    if (sourceStack?.Collectible?.FirstCodePart() != "temporalhole" && priority == EnumMergePriority.DirectMerge)
        //    {
        //        return sourceStack.StackSize;
        //    }
        //    return 0;
        //}

        //// takes the stack dropped onto the hole and adds it to attributes
        //// only works when holding ctrl
        //public override void TryMergeStacks(ItemStackMergeOperation op)
        //{
        //    if (op.SourceSlot?.Itemstack?.Collectible?.FirstCodePart() != "temporalhole" && op.CurrentPriority == EnumMergePriority.DirectMerge && op.CtrlDown)
        //    {
        //        op.MovedQuantity = op.SourceSlot.StackSize;
        //        AddToHole(op.SinkSlot, op.SourceSlot);
        //        return;
        //    }
        //}

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        // redundant code from when I attempted to handle storing items using an inventory with dialogue //
        ///////////////////////////////////////////////////////////////////////////////////////////////////

        //private GuiDialogHoleInput invDialog;
        //private long currentHole = -1;

        //public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        //{
        //    if (!firstEvent) return;
        //    if (api.Side == EnumAppSide.Server && !slot.Itemstack.Attributes.HasAttribute("holeid"))
        //    {
        //        long holeId = api.World.Rand.NextInt64();
        //        slot.Itemstack.Attributes.SetLong("holeid", holeId);
        //        slot.Itemstack.Attributes.GetOrAddTreeAttribute("itemkeys");
        //        slot.Itemstack.Attributes.GetOrAddTreeAttribute("itemstacks");
        //        slot.MarkDirty();
        //    }
        //    IPlayer byPlayer = (byEntity as EntityPlayer)?.Player;
        //    if (byPlayer == null) return;

        //    currentHole = slot.Itemstack.Attributes.GetLong("holeid");
        //    InventoryOutsideTime inv = new InventoryOutsideTime(1, "inventoryholeinput-0", slot, api, (_, invent) => new ItemSlotHoleInput(invent));
        //    api.Logger.Notification("Inventory created: {0}", inv);

        //    handling = EnumHandHandling.PreventDefault;
        //    return;
        //}

        //public override void OnModifiedInInventorySlot(IWorldAccessor world, ItemSlot slot, ItemStack extractedStack = null)
        //{
        //    if (slot?.Itemstack?.Attributes?.GetLong("holeid") == currentHole || extractedStack?.Attributes?.GetLong("holeid") == currentHole)
        //    {
        //        invDialog?.TryClose();
        //    }
        //    base.OnModifiedInInventorySlot(world, slot, extractedStack);
        //}

        //protected void ToggleInventoryDialogClient(IPlayer byPlayer, InventoryOutsideTime inv)
        //{
        //    if (invDialog == null)
        //    {
        //        ICoreClientAPI capi = api as ICoreClientAPI;
        //        invDialog = new GuiDialogHoleInput(Lang.Get("temporalhole:item-temporalhole"), inv, api as ICoreClientAPI);
        //        invDialog.OnClosed += () =>
        //        {
        //            invDialog = null;
        //            capi.Network.SendPacketClient(inv.Close(byPlayer));
        //            currentHole = -1;
        //        };

        //        invDialog.TryOpen();
        //        capi.Network.SendPacketClient(inv.Open(byPlayer));
        //    }
        //    else
        //    {
        //        invDialog.TryClose();
        //    }
        //}
    }
}
