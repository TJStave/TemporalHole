using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.Client.NoObf;

namespace TemporalHole;

public class TemporalHoleModSystem : ModSystem
{
    internal static ICoreAPI api;
    internal static Harmony harmony;
    internal static IServerNetworkChannel serverChannel;
    internal static IClientNetworkChannel clientChannel;

    public const string TemporalHoleInventoryPatchCategory = "temporalHoleInventory";
    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        TemporalHoleModSystem.api = api;

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
}
