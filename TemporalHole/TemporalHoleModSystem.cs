using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.Client.NoObf;
using System;

namespace TemporalHole;

public class TemporalHoleModSystem : ModSystem
{
    internal static ICoreAPI api;
    internal static Harmony harmony;
    internal static IServerNetworkChannel serverChannel;
    internal static IClientNetworkChannel clientChannel;
    public static TemporalHoleConfig config;

    public const string TemporalHoleInventoryPatchCategory = "temporalHoleInventory";
    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        TemporalHoleModSystem.api = api;

        TryToLoadConfig(api);

        api.Network.RegisterChannel("temporalhole").RegisterMessageType(typeof(TemporalHolePacket));

        api.RegisterItemClass("TemporalHole.ItemTemporalHole", typeof(ItemTemporalHole));
        api.RegisterBlockClass("TemporalHole.BlockTemporalExtractor", typeof(BlockTemporalExtractor));
        api.RegisterBlockEntityClass("TemporalHole.BlockEntityTemporalExtractor", typeof(BlockEntityTemporalExtractor));

        if (harmony == null)
        {
            harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchCategory(TemporalHoleInventoryPatchCategory);
            Mod.Logger.VerboseDebug("Temporal Hole Patched");
        }
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverChannel = api.Network.GetChannel("temporalhole")
            .SetMessageHandler<TemporalHolePacket>(TemporalHoleNetworking.HandleHoleSink);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        clientChannel = api.Network.GetChannel("temporalhole");
    }

    public override void Dispose()
    {
        harmony?.UnpatchAll(Mod.Info.ModID);
        harmony = null;
        base.Dispose();
    }

    private void TryToLoadConfig(ICoreAPI api)
    {
        //It is important to surround the LoadModConfig function in a try-catch. 
        //If loading the file goes wrong, then the 'catch' block is run.
        try
        {
            config = api.LoadModConfig<TemporalHoleConfig>("temporalholeconfig.json");
            if (config == null) //if the 'temporalholeconfig.json' file isn't found...
            {
                config = new TemporalHoleConfig();
            }
            //Save a copy of the mod config.
            api.StoreModConfig<TemporalHoleConfig>(config, "temporalholeconfig.json");
        }
        catch (Exception e)
        {
            //Couldn't load the mod config... Create a new one with default settings, but don't save it.
            Mod.Logger.Error("Could not load config! Loading default settings instead.");
            Mod.Logger.Error(e);
            config = new TemporalHoleConfig();
        }
    }
}
