using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Common;

//namespace TemporalHole
//{
//    public static class CollectibleObject_TryMergeStacks_Patch
//    {
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.TryMergeStacks))]
//        public static bool HoleInputHandler(ItemStackMergeOperation op, ICoreAPI ___api)
//        {
//            if (op?.SourceSlot?.Itemstack?.Attributes == null) return true;

//            if (op.SinkSlot?.Itemstack?.Collectible?.FirstCodePart() == "temporalhole" && op.SourceSlot?.Itemstack?.Collectible?.FirstCodePart() != "temporalhole" &&
//                op.CurrentPriority == EnumMergePriority.DirectMerge && op.MouseButton == EnumMouseButton.Right)
//            {
//                string genKey = Convert.ToBase64String(BitConverter.GetBytes(___api.World.Rand.NextInt64()))[..8];
//                while (op.SinkSlot.Itemstack.Attributes.GetOrAddTreeAttribute("itemkeys").HasAttribute(genKey))
//                {
//                    genKey = Convert.ToBase64String(BitConverter.GetBytes(___api.World.Rand.NextInt64()))[..8];
//                }
//                op.SinkSlot.Itemstack.Attributes.GetOrAddTreeAttribute("itemkeys").SetString(genKey, genKey);
//                op.SinkSlot.Itemstack.Attributes.GetOrAddTreeAttribute("itemstacks").SetItemstack(genKey, op.SourceSlot.Itemstack);

//                op.SourceSlot.TakeOutWhole();
//                op.SinkSlot.MarkDirty();
//                return false;
//            }
//            return true;
//        }
//    }
//}
