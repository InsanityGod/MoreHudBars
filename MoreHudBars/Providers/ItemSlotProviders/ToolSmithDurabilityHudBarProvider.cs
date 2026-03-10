using Cairo;
using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using Toolsmith.ToolTinkering;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class ToolSmithDurabilityHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.DurabilityBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(!Config.ShowEvenIfBarFull && !itemStack.Collectible.ShouldDisplayItemDamage(itemStack)) return false;

        float maxDurability = TinkeringUtility.FindLowestMaxDurabilityForBar(itemStack);
        if(maxDurability <= 0) return false;

        float currentDurability = TinkeringUtility.FindLowestCurrentDurabilityForBar(itemStack);

        percentage =  currentDurability / maxDurability;
        return true;
    }

    public Color? GetColorOVerride(ItemSlot slot, float percentage)
    {
        var color = ColorUtil.ToRGBAFloats(TinkeringUtility.ToolsmithGetItemDamageColor(slot.Itemstack));
        return new(color[0], color[1], color[2]);
    }
}
