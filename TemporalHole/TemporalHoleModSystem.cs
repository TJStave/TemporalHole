using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;

namespace TemporalHole;

public class TemporalHoleModSystem : ModSystem
{

    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        api.RegisterItemClass("TemporalHole.ItemTemporalHole", typeof(ItemTemporalHole));
        api.RegisterBlockClass("TemporalHole.BlockTemporalExtractor", typeof(BlockTemporalExtractor));
        api.RegisterBlockEntityClass("TemporalHole.BlockEntityTemporalExtractor", typeof(BlockEntityTemporalExtractor));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
    }

}
