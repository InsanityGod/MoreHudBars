using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using System;
using System.Linq;
using Vintagestory.API.Common;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class BagHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.BagBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(itemStack.Collectible.GetCollectibleInterface<IHeldBag>() is not { } heldbag) return false;

        float maxSlots = heldbag.GetQuantitySlots(itemStack);
        if(maxSlots <= 0) return false;

        float usedSlots = heldbag.GetContents(itemStack, world)?.Count(static stack => stack is not null) ?? 0;

        percentage = usedSlots / maxSlots;

        return true;
    }
}
