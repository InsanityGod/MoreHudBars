using Cairo;
using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class ConditionHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.ClothingConditionBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(itemStack.Collectible is not ItemWearable { }) return false;
        
        percentage = itemStack.Attributes.GetFloat("condition", -1);
        return percentage >= 0;
    }

    public Color? GetColorOVerride(ItemSlot slot)
    {
        var color = ColorUtil.ToRGBAFloats(GuiStyle.DamageColorGradient[(int)Math.Min(99f, slot.Itemstack.Attributes.GetFloat("condition", 1) * 200f)]);
        return new(color[0], color[1], color[2]);
    }
}
