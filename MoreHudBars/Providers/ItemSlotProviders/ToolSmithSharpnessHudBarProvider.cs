using Cairo;
using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using Toolsmith;
using Toolsmith.ToolTinkering;
using Toolsmith.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class ToolSmithSharpnessHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.Compatibility.ToolSmithSharpnessBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        if(!TinkeringUtility.ShouldRenderSharpnessBar(itemSlot.Itemstack)) return false;

        percentage = (float)itemSlot.Itemstack.GetToolCurrentSharpness() / (float)itemSlot.Itemstack.GetToolMaxSharpness();

        return true;
    }

    public Color? GetColorOVerride(ItemSlot slot, float percentage)
    {
        float[] colorArray;
        if (ToolsmithModSystem.ClientConfig is { UseGradientForSharpnessInstead: true})
        {
            if (TinkeringUtility.GradiantNeedsInit()) TinkeringUtility.InitializeSharpnessColorGradient();

            colorArray = ColorUtil.ToRGBAFloats(TinkeringUtility.GetItemSharpnessColor(slot.Itemstack));
        }
        else
        {
            colorArray = ColorUtil.ToRGBAFloats(TinkeringUtility.GetFlatItemSharpnessColor((double) percentage switch
            {
                < 0.15 => 0,
                < 0.3 => 1,
                < 0.6 => 2,
                < 0.8 => 3,
                _ => 4,
            }));
        }

        //TODO see about possibility of supporting clientConfig.ShowAllSharpnessBarSections
        return new(colorArray[0], colorArray[1], colorArray[2]);
    }
}
