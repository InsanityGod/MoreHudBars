using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class FoodPortionsHudBarProvider : IItemSlotHudBarProvider
{
    public HudBarConfig Config => MoreHudBarsConfig.Instance!.FoodPortionBar;

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        percentage = 1f;
        var itemStack = itemSlot.Itemstack;
        if(itemStack.Collectible.GetCollectibleInterface<IBlockMealContainer>() is not { } mealContainer) return false;

        var maxServings = GetMaxServings(itemStack);
        if(maxServings <= 0) return false;

        var currentServings = GetCurrentServing(world, mealContainer, itemStack);
        if(currentServings <= 0) return false;
        
        percentage = currentServings / maxServings;
        return true;
    }

    private static float GetMaxServings(ItemStack itemStack)
    {
        if(itemStack.Collectible is BlockPie) return itemStack.Attributes.GetAsInt("pieSize", 4) == 1 ? 0.25f : 1;

        return itemStack.ItemAttributes["servingCapacity"].AsFloat(1);
    }

    private static float GetCurrentServing(IWorldAccessor world, IBlockMealContainer mealContainer, ItemStack itemStack)
    {
        if(itemStack.Collectible is BlockPie)
        {
            var pieSize = itemStack.Attributes.GetAsInt("pieSize", 4);
            if(pieSize != 1)
            {
                return pieSize * 0.25f;
            }
        }

        return mealContainer.GetQuantityServings(world, itemStack);
    }
}
