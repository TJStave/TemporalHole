using Cairo.Freetype;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.GameContent.Mechanics;

namespace TemporalHole
{

    public class BlockEntityTemporalExtractor : BlockEntityOpenableContainer
    {
        static SimpleParticleProperties holeParticles;
        static SimpleParticleProperties holeTimeParticles;
        static SimpleParticleProperties extractorArmParticles;

        static BlockEntityTemporalExtractor()
        {
            holeParticles = new SimpleParticleProperties(1, 1, ColorUtil.ToRgba(40, 180, 180, 180), new Vec3d(), new Vec3d(), new Vec3f(), new Vec3f());
            holeParticles.MinVelocity.Set(-0.1f, -0.1f, -0.1f);
            holeParticles.AddVelocity.Set(0.2f, 0.2f, 0.2f);
            holeParticles.LifeLength = 1f;
            holeParticles.GravityEffect = 0f;
            holeParticles.ParticleModel = EnumParticleModel.Cube;
            holeParticles.WithTerrainCollision = false;
            holeParticles.MinSize = 0.5f;
            holeParticles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEAR, 0.2f);
            holeParticles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -16f);

            holeTimeParticles = new SimpleParticleProperties(1, 1, ColorUtil.ToRgba(60, 50, 235, 200), new Vec3d(), new Vec3d(), new Vec3f(), new Vec3f());
            holeTimeParticles.AddPos.Set(2f / 16f, 2f / 16f, 2f / 16f);
            holeTimeParticles.MinVelocity.Set(-0.01f, -0.01f, -0.01f);
            holeTimeParticles.AddVelocity.Set(0.02f, 0.02f, 0.02f);
            holeTimeParticles.LifeLength = 1f;
            holeTimeParticles.GravityEffect = 0f;
            holeTimeParticles.ParticleModel = EnumParticleModel.Quad;
            holeTimeParticles.WithTerrainCollision = false;
            holeTimeParticles.MinSize = 0.2f;
            holeTimeParticles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -16f);

            extractorArmParticles = new SimpleParticleProperties(1, 1, ColorUtil.ToRgba(60, 50, 235, 200), new Vec3d(), new Vec3d(), new Vec3f(), new Vec3f());
            extractorArmParticles.LifeLength = 0.25f;
            extractorArmParticles.GravityEffect = 0f;
            extractorArmParticles.ParticleModel = EnumParticleModel.Quad;
            extractorArmParticles.WithTerrainCollision = false;
            extractorArmParticles.MinSize = 0.1f;
            extractorArmParticles.MaxSize = 0.12f;
            holeTimeParticles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.QUADRATIC, -16f);
        }

        internal InventoryTemporalExtractor inventory;
        internal BlockTemporalExtractor ownBlock;

        // For how long the current ore has been grinding
        public float inputExtractTime;
        public float prevInputExtractTime;


        //GuiDialogBlockEntityQuern clientDialog;
        ExtractorArmsRenderer renderer;
        bool automated;
        BEBehaviorMPConsumer mpc;
        private float prevSpeed = float.NaN;

        #region Getters

        public float ExtractSpeed
        {
            get
            {
                // if powerless extraction is enabled
                if (TemporalHoleModSystem.config?.powerlessExtraction == true) return Math.Max(1, mpc.TrueSpeed);
                if (automated && mpc.Network != null) return mpc.TrueSpeed;

                return 0;
            }
        }

        MeshData ExtractorBaseMesh
        {
            get
            {
                object value;
                Api.ObjectCache.TryGetValue("temporalholemod-extractorbasemesh-" + ownBlock.LastCodePart(), out value);
                return (MeshData)value;
            }
            set
            { Api.ObjectCache["temporalholemod-extractorbasemesh-" + ownBlock.LastCodePart()] = value; }
        }

        MeshData ExtractorArmsMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("temporalholemod-extractorarmsmesh-" + ownBlock.LastCodePart(), out value);
                return (MeshData)value;
            }
            set
            { Api.ObjectCache["temporalholemod-extractorarmsmesh-" + ownBlock.LastCodePart()] = value; }
        }

        MeshData TemporalHoleMesh
        {
            get
            {
                object value;
                Api.ObjectCache.TryGetValue("temporalholemod-temporalholemesh-" + ownBlock.LastCodePart(), out value);
                return (MeshData)value;
            }
            set { Api.ObjectCache["temporalholemod-temporalholemesh-" + ownBlock.LastCodePart()] = value; }
        }

        #endregion

        #region Config

        public virtual float MaxExtractTime()
        {
            return 4;
        }

        public override string InventoryClassName
        {
            get { return "temporalExtractor"; }
        }

        public virtual string DialogTitle
        {
            get { return Lang.Get("temporalhole:dialog-temporalextractor"); }
        }

        public override InventoryBase Inventory
        {
            get { return inventory; }
        }

        #endregion

        public BlockEntityTemporalExtractor()
        {
            inventory = new InventoryTemporalExtractor(null, null);
            inventory.SlotModified += OnSlotModifid;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.LateInitialize("temporalextractor-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);

            ownBlock = Block as BlockTemporalExtractor;

            RegisterGameTickListener(Every100ms, 100);
            RegisterGameTickListener(Every500ms, 500);

            if (api.Side == EnumAppSide.Client)
            {
                renderer = new ExtractorArmsRenderer(api as ICoreClientAPI, Pos, GenMesh("arms"));
                renderer.mechPowerPart = this.mpc;
                if (automated)
                {
                    renderer.ShouldRender = true;
                    renderer.ShouldRotateAutomated = true;
                }

                (api as ICoreClientAPI).Event.RegisterRenderer(renderer, EnumRenderStage.Opaque, "extractor");

                ExtractorBaseMesh ??= GenMesh("base");
                ExtractorArmsMesh ??= GenMesh("arms");
                TemporalHoleMesh ??= GenHoleMeshData();
            }
        }

        public override void CreateBehaviors(Block block, IWorldAccessor worldForResolve)
        {
            base.CreateBehaviors(block, worldForResolve);

            mpc = GetBehavior<BEBehaviorMPConsumer>();
            if (mpc != null)
            {
                mpc.OnConnected = () => {
                    automated = true;
                    if (renderer != null)
                    {
                        renderer.ShouldRender = true;
                        renderer.ShouldRotateAutomated = true;
                    }
                };

                mpc.OnDisconnected = () => {
                    automated = false;
                    if (renderer != null)
                    {
                        renderer.ShouldRender = false;
                        renderer.ShouldRotateAutomated = false;
                    }
                };
            }
        }

        private void Every100ms(float dt)
        {
            float extractSpeed = ExtractSpeed;

            if (Api.Side == EnumAppSide.Client)
            {
                if (!InputSlot.Empty)
                {
                    holeParticles.MinPos.Set(Pos.X + 0.5, Pos.Y + 0.5, Pos.Z + 0.5);
                    holeTimeParticles.MinPos.Set(Pos.X + 0.5, Pos.Y + 0.4, Pos.Z + 0.5);
                    holeTimeParticles.AddPos.Set(0.2, 0.2, 0.2);

                    Api.World.SpawnParticles(holeParticles);
                    if (CanExtract())
                    {
                        if (Api.World.Rand.Next(4) == 0) 
                            Api.World.SpawnParticles(holeTimeParticles);

                        //if(Api.World.Rand.NextDouble() * 100 < 20 * armParticleChance)
                        if(extractSpeed > 0)
                        {
                            double particlex = 0.37 * Math.Sin(renderer.AngleRad + (0.5 * Math.PI));
                            double particlez = 0.37 * Math.Cos(renderer.AngleRad + (0.5 * Math.PI));
                            extractorArmParticles.MinPos.Set(Pos.X + 0.5 + particlex, Pos.Y + 0.6, Pos.Z + 0.5 + particlez);
                            Api.World.SpawnParticles(extractorArmParticles);
                            extractorArmParticles.MinPos.Set(Pos.X + 0.5 + particlex, Pos.Y + 0.4, Pos.Z + 0.5 + particlez);
                            Api.World.SpawnParticles(extractorArmParticles);
                            extractorArmParticles.MinPos.Set(Pos.X + 0.5 - particlex, Pos.Y + 0.6, Pos.Z + 0.5 - particlez);
                            Api.World.SpawnParticles(extractorArmParticles);
                            extractorArmParticles.MinPos.Set(Pos.X + 0.5 - particlex, Pos.Y + 0.4, Pos.Z + 0.5 - particlez);
                            Api.World.SpawnParticles(extractorArmParticles);
                        }

                    }
                }
                if (automated && mpc.TrueSpeed != prevSpeed)
                {
                    prevSpeed = mpc.TrueSpeed;
                }
                else prevSpeed = float.NaN;

                return;
            }


            // Only tick on the server and merely sync to client
            if (CanExtract() && extractSpeed > 0)
            {
                // modify by speed modifier (the ?? is there if something goes wrong and config is missing)
                inputExtractTime += dt * extractSpeed * TemporalHoleModSystem.config?.extractionSpeedModifier ?? 1;

                if (inputExtractTime >= MaxExtractTime())
                {
                    ExtractInput();
                    inputExtractTime = 0;
                }

                MarkDirty();
            }
        }

        private void ExtractInput()
        {
            // select a random key
            string extractedKey = InputHoleKeys.Values[Api.World.Rand.Next(InputHoleKeys.Count)].GetValue() as string;

            // get the code and ItemStack bytes
            string extractedCode = InputHoleCodes.GetString(extractedKey);
            byte[] extractedBytes = InputHoleStacks.GetBytes(extractedKey);

            if(extractedBytes == null)
            {
                Api.Logger.Error("Extractor failed to find bytes with key {0}", extractedKey);
                InputHoleKeys.RemoveAttribute(extractedKey);
                InputHoleCodes.RemoveAttribute(extractedKey);
                InputHoleStacks.RemoveAttribute(extractedKey);
                InputSlot.MarkDirty();
                return;
            }

            // create the stack from bytes
            ItemStack extractedStack = new ItemStack(extractedBytes);

            if(extractedCode != null)
            {
                // this whole section serves mostly for ensuring the stacks have the correct id if the temporal hole has been moved between worlds somehow
                if (extractedStack.Class == EnumItemClass.Block)
                {
                    Block block = Api.World.GetBlock(new AssetLocation(extractedCode));
                    if(block == null)
                    {
                        Api.Logger.Error("Extractor failed to find block with code {0}", extractedCode);
                        InputHoleKeys.RemoveAttribute(extractedKey);
                        InputHoleCodes.RemoveAttribute(extractedKey);
                        InputHoleStacks.RemoveAttribute(extractedKey);
                        InputSlot.MarkDirty();
                        return;
                    }
                    extractedStack.Id = block.Id;
                }
                else if(extractedStack.Class == EnumItemClass.Item)
                {
                    Item item = Api.World.GetItem(new AssetLocation(extractedCode));
                    if (item == null)
                    {
                        Api.Logger.Error("Extractor failed to find item with code {0}", extractedCode);
                        InputHoleKeys.RemoveAttribute(extractedKey);
                        InputHoleCodes.RemoveAttribute(extractedKey);
                        InputHoleStacks.RemoveAttribute(extractedKey);
                        InputSlot.MarkDirty();
                        return;
                    }
                    extractedStack.Id = item.Id;
                }
            }

            // now that the stack should have the right id, resolve it
            extractedStack.ResolveBlockOrItem(Api.World);

            // if there's a chute below, send the item to internal output slot to allow auto-pull
            Block below = Api.World.BlockAccessor.GetBlock(this.Pos.DownCopy());
            if (below?.Attributes != null && below.Attributes.KeyExists("item-flowrate"))
            {
                // these checks make the extractor only get the next item when there's room
                // this is intended to allow it to run automatically without accidentally letting items despawn on the ground
                if (OutputSlot.Itemstack == null)
                {
                    OutputSlot.Itemstack = extractedStack;
                }
                else
                {
                    int mergableQuantity = OutputSlot.Itemstack.Collectible.GetMergableQuantity(OutputSlot.Itemstack, extractedStack, EnumMergePriority.AutoMerge);

                    if (mergableQuantity > 0)
                    {
                        OutputSlot.Itemstack.StackSize += extractedStack.StackSize;
                    }
                    else return;
                }
            }
            // otherwise drop the stack as an item
            else Api.World.SpawnItemEntity(extractedStack, this.Pos.ToVec3d().Add(0.5, 0.2, 0.5), new Vec3d(0, -0.02f, 0));

            InputHoleKeys.RemoveAttribute(extractedKey);
            InputHoleCodes.RemoveAttribute(extractedKey);
            InputHoleStacks.RemoveAttribute(extractedKey);
            InputSlot.MarkDirty();
            OutputSlot.MarkDirty();
        }

        public void DumpOutput(IWorldAccessor world)
        {
            Block below = world.BlockAccessor.GetBlock(this.Pos.DownCopy());
            if (!OutputSlot.Empty && (below?.Attributes == null || !below.Attributes.KeyExists("item-flowrate")))
            {
                world.SpawnItemEntity(OutputSlot.TakeOutWhole(), this.Pos.ToVec3d().Add(0.5, 0.2, 0.5), new Vec3d(0, -0.02f, 0));
            }
        }


        // Sync to client every 500ms
        private void Every500ms(float dt)
        {
            if (Api.Side == EnumAppSide.Server && (ExtractSpeed > 0 || prevInputExtractTime != inputExtractTime) && inventory[0].Itemstack?.Collectible.FirstCodePart() == "temporalhole")  //don't spam update packets when empty
            {
                MarkDirty();
            }

            prevInputExtractTime = inputExtractTime;
        }

        bool beforeExtracting;
        void updateExtractingState()
        {
            if (Api?.World == null) return;

            bool nowExtracting = automated && mpc.TrueSpeed > 0f;

            if (nowExtracting != beforeExtracting)
            {
                if (renderer != null)
                {
                    renderer.ShouldRotateManual = false;
                }

                Api.World.BlockAccessor.MarkBlockDirty(Pos, OnRetesselated);

                if (Api.Side == EnumAppSide.Server)
                {
                    MarkDirty();
                }
            }

            beforeExtracting = nowExtracting;
        }

        private void OnSlotModifid(int slotid)
        {
            //if (Api is ICoreClientAPI)
            //{
            //    clientDialog.Update(inputExtractTime, MaxExtractTime());
            //}

            if (slotid == 0)
            {
                if (InputSlot.Empty)
                {
                    inputExtractTime = 0.0f; // reset the progress to 0 if the item is removed.
                }
                MarkDirty(true);

                //if (clientDialog != null && clientDialog.IsOpened())
                //{
                //    clientDialog.SingleComposer.ReCompose();
                //}
            }
        }


        private void OnRetesselated()
        {
            if (renderer == null) return; // Maybe already disposed

            renderer.ShouldRender = automated;
        }

        internal MeshData GenMesh(string type = "base")
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.BlockId == 0) return null;

            MeshData mesh;
            ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;

            mesher.TesselateShape(block, Shape.TryGet(Api, "temporalhole:shapes/block/temporalextractor-" + type + ".json"), out mesh);

            RotateMesh(mesh);

            return mesh;
        }

        internal MeshData GenHoleMeshData()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.BlockId == 0) return null;

            MeshData mesh;
            ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;

            mesher.TesselateShape(block, Shape.TryGet(Api, "temporalhole:shapes/item/temporalhole.json"), out mesh);

            mesh.Translate(new Vec3f(0f, -0.5f / 16f, -3f / 16f))
                .Rotate(new Vec3f(0f, 0f, 0.5f), 0.5f * (float)Math.PI, 0f, 0f)
                .Translate(new Vec3f(0f, 5f / 16f, 0f));

            RotateMesh(mesh);

            return mesh;
        }

        public bool CanExtract()
        {
            TreeAttribute holeContents = InputHoleKeys;
            if (holeContents == null || holeContents.Count == 0) return false;
            return true;
        }

        #region Events

        public override bool OnPlayerRightClick(IPlayer byPlayer, BlockSelection blockSel)
        {
            //if (Api.Side == EnumAppSide.Client)
            //{
            //    toggleInventoryDialogClient(byPlayer, () =>
            //    {
            //        clientDialog = new GuiDialogBlockEntityQuern(DialogTitle, Inventory, Pos, Api as ICoreClientAPI);
            //        clientDialog.Update(inputExtractTime, MaxExtractTime());
            //        return clientDialog;
            //    });
            //}

            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (slot.Empty)
            {
                if (InputSlot.TryPutInto(Api.World, slot) > 0)
                {
                    MarkDirty();
                    return true;
                }
            }
            else if (InputSlot.Empty && slot.IsTimeHole())
            {
                if (slot.TryPutInto(Api.World, InputSlot) > 0)
                {
                    MarkDirty();
                    return true;
                }
            }
            return false;
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            base.OnReceivedClientPacket(player, packetid, data);
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            base.OnReceivedServerPacket(packetid, data);

            if (packetid == (int)EnumBlockEntityPacketId.Close)
            {
                (Api.World as IClientWorldAccessor).Player.InventoryManager.CloseInventory(Inventory);
                invDialog?.TryClose();
                invDialog?.Dispose();
                invDialog = null;
            }
        }



        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            Inventory.FromTreeAttributes(tree.GetTreeAttribute("inventory"));

            if (Api != null)
            {
                Inventory.AfterBlocksLoaded(Api.World);
            }

            inputExtractTime = tree.GetFloat("inputExtractTime");

            //if (Api?.Side == EnumAppSide.Client && clientDialog != null)
            //{
            //    clientDialog.Update(inputExtractTime, MaxExtractTime());
            //}
        }



        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            ITreeAttribute invtree = new TreeAttribute();
            Inventory.ToTreeAttributes(invtree);
            tree["inventory"] = invtree;

            tree.SetFloat("inputExtractTime", inputExtractTime);
        }




        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            //clientDialog?.TryClose();

            renderer?.Dispose();
            renderer = null;
        }
        #endregion

        #region Helper getters


        public ItemSlot InputSlot
        {
            get { return inventory[0]; }
        }

        public ItemSlot OutputSlot
        {
            get { return inventory[1]; }
        }

        public ItemStack InputStack
        {
            get { return inventory[0].Itemstack; }
            set { inventory[0].Itemstack = value; inventory[0].MarkDirty(); }
        }

        public ItemStack OutputStack
        {
            get { return inventory[1].Itemstack; }
            set { inventory[1].Itemstack = value; inventory[1].MarkDirty(); }
        }


        public TreeAttribute InputHoleKeys
        {
            get
            {
                ItemSlot slot = inventory[0];
                return slot.Itemstack?.Attributes?.GetOrAddTreeAttribute("itemkeys") as TreeAttribute;
            }
        }

        public TreeAttribute InputHoleStacks
        {
            get
            {
                ItemSlot slot = inventory[0];
                return slot.Itemstack?.Attributes?.GetOrAddTreeAttribute("itemstacks") as TreeAttribute;
            }
        }

        public TreeAttribute InputHoleCodes
        {
            get
            {
                ItemSlot slot = inventory[0];
                return slot.Itemstack?.Attributes?.GetOrAddTreeAttribute("itemcodes") as TreeAttribute;
            }
        }

        #endregion


        public override void OnStoreCollectibleMappings(Dictionary<int, AssetLocation> blockIdMapping, Dictionary<int, AssetLocation> itemIdMapping)
        {
            foreach (var slot in Inventory)
            {
                if (slot.Itemstack == null) continue;

                if (slot.Itemstack.Class == EnumItemClass.Item)
                {
                    itemIdMapping[slot.Itemstack.Item.Id] = slot.Itemstack.Item.Code;
                }
                else
                {
                    blockIdMapping[slot.Itemstack.Block.BlockId] = slot.Itemstack.Block.Code;
                }
                slot.Itemstack?.Collectible.OnStoreCollectibleMappings(Api.World, slot, blockIdMapping, itemIdMapping);
            }
        }

        public override void OnLoadCollectibleMappings(IWorldAccessor worldForResolve, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, int schematicSeed, bool resolveImports)
        {
            foreach (var slot in Inventory)
            {
                if (slot.Itemstack == null) continue;
                if (!slot.Itemstack.FixMapping(oldBlockIdMapping, oldItemIdMapping, worldForResolve))
                {
                    slot.Itemstack = null;
                }
                slot.Itemstack?.Collectible.OnLoadCollectibleMappings(worldForResolve, slot, oldBlockIdMapping, oldItemIdMapping, resolveImports);
            }
        }



        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            if (Block == null) return false;

            mesher.AddMeshData(this.ExtractorBaseMesh);
            if (!automated)
            {
                mesher.AddMeshData(
                    this.ExtractorArmsMesh.Clone()
                    .Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, renderer.AngleRad, 0)
                );
            }

            if (!InputSlot.Empty)
            {
                mesher.AddMeshData(this.TemporalHoleMesh);
            }

            return true;
        }

        private MeshData RotateMesh(MeshData mesh)
        {
            switch (ownBlock.LastCodePart())
            {
                case "west":
                    mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, 0.5f * (float)Math.PI, 0);
                    break;
                case "south":
                    mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, 1f * (float)Math.PI, 0);
                    break;
                case "east":
                    mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, 1.5f * (float)Math.PI, 0);
                    break;
            }
            return mesh;
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            renderer?.Dispose();
        }

    }
}