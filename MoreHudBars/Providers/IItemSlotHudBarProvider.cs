using Cairo;
using MoreHudBars.Config.SubConfigs;
using Vintagestory.API.Common;

namespace MoreHudBars.Providers;

public interface IItemSlotHudBarProvider
{
    HudBarConfig Config { get; }

    bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage);

    Color? GetColorOVerride(ItemSlot slot, float percentage) => null;
}
