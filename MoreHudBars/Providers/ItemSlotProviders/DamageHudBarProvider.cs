using Cairo;
using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class DamageHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.DamageBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(!MoreHudBarsConfig.Instance!.DamageBar.ShowEvenIfBarFull && !itemStack.Collectible.ShouldDisplayItemDamage(itemStack)) return false;

        float maxDurability = itemStack.Collectible.GetMaxDurability(itemStack);
        if(maxDurability <= 0) return false;

        float currentDurability = itemStack.Collectible.GetRemainingDurability(itemStack);

        percentage =  currentDurability / maxDurability;
        return true;
    }

    public Color? GetColorOVerride(ItemSlot slot)
    {
        var color = ColorUtil.ToRGBAFloats(slot.Itemstack.Collectible.GetItemDamageColor(slot.Itemstack));
        return new(color[0], color[1], color[2]);
    }
}
