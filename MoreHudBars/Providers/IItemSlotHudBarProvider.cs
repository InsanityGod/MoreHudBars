using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Info;
using Vintagestory.API.Common;

namespace MoreHudBars.Providers;

public interface IItemSlotHudBarProvider
{
    HudBarConfig Config { get; }

    bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage);
}
