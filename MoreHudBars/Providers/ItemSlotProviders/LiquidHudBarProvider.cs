using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class LiquidHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.LiquidBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(itemStack.Collectible.GetCollectibleInterface<ILiquidInterface>() is not { } liquidInterface) return false;

        var currentLiters = liquidInterface.GetCurrentLitres(itemStack);
        if(currentLiters <= 0) return false;

        percentage =  currentLiters / liquidInterface.CapacityLitres;

        return true;
    }
}
