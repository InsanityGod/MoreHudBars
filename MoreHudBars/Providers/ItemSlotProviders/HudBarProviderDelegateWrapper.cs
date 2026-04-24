using Cairo;
using MoreHudBars.Config;
using MoreHudBars.Config.SubConfigs;
using System;
using Vintagestory.API.Common;

namespace MoreHudBars.Providers.ItemSlotProviders;

public class HudBarProviderDelegateWrapper : IItemSlotHudBarProvider
{
    public required string Identifier { get; init; }

    public required Func<HudBarConfig> DefaultConfigProvider { get; init; }

    public required System.Func<IWorldAccessor, ItemSlot, (bool result, float percentage)> PercentageProvider {get; init; }

    public required System.Func<ItemSlot, float, Color?>? ColorProvider { get; init;}

    public HudBarConfig Config
    {
        get
        {
            if(MoreHudBarsConfig.Instance!.ExternalyRegistered.TryGetValue(Identifier, out var config))
            {
                return config;
            } 
            else return MoreHudBarsConfig.Instance.ExternalyRegistered[Identifier] = DefaultConfigProvider();
        }
    }

    public bool TryGetPercentage(IWorldAccessor world, ItemSlot itemSlot, out float percentage)
    {
        var provided = PercentageProvider(world, itemSlot);

        percentage = provided.percentage;
        return provided.result;
    }

    public Color? GetColorOVerride(ItemSlot slot, float percentage) => ColorProvider?.Invoke(slot, percentage);
}
