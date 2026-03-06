using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Info;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class ConditionHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.ConditionBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(itemStack.Collectible is not ItemWearable { }) return false;
        
        percentage = itemStack.Attributes.GetFloat("condition", -1);
        return percentage >= 0;
    }
}
