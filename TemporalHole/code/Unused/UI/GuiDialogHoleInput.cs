using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

//namespace TemporalHole
//{
//    class GuiDialogHoleInput : GuiDialog
//    {
//        public override string ToggleKeyCombinationCode => "temporalholegui";

//        public GuiDialogHoleInput(string dialogTitle, InventoryBase inventory, ICoreClientAPI capi) : base(capi)
//        {
//            SetupDialog(dialogTitle, inventory);
//        }

//        private void SetupDialog(string dialogTitle, InventoryBase inventory)
//        {
//            double elemToDlgPad = GuiStyle.ElementToDialogPadding;
//            double pad = GuiElementItemSlotGrid.unscaledSlotPadding;

//            ElementBounds slotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, pad, pad, 1, 1);
//            ElementBounds insetBounds = slotBounds.ForkBoundingParent(6, 6, 6, 6);

//            ElementBounds dialogBounds = insetBounds
//                .ForkBoundingParent(elemToDlgPad, elemToDlgPad + 20, elemToDlgPad, elemToDlgPad)
//                .WithAlignment(EnumDialogArea.RightMiddle);

//            SingleComposer = capi.Gui
//                .CreateCompo("temporalholeinput", dialogBounds)
//                .AddShadedDialogBG(ElementBounds.Fill)
//                .AddDialogTitleBar(dialogTitle, CloseIconPressed)
//                .AddInset(insetBounds)
//                .AddItemSlotGrid(inventory, null, 1, slotBounds)
//                .Compose();

//            SingleComposer.UnfocusOwnElements();
//        }

//        protected void CloseIconPressed()
//        {
//            TryClose();
//        }
//    }
//}
