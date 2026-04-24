using InsanityLib.Generators.Attributes;
using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Providers;
using MoreHudBars.Providers.ItemSlotProviders;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MoreHudBars;

public partial class MoreHudBarsModSystem : ModSystem
{
    [AutoClear] internal static List<IItemSlotHudBarProvider> ItemSlotHudBarProviders { get; } = [];

    public static void RegisterForItemSlot(string identifier, Func<HudBarConfig> defaultConfigProvider, System.Func<IWorldAccessor, ItemSlot, (bool result, float percentage)> percentageProvider) => ItemSlotHudBarProviders.Add(new HudBarProviderDelegateWrapper
    {
        Identifier = identifier,
        DefaultConfigProvider = defaultConfigProvider,
        PercentageProvider = percentageProvider
    });

    public static void RegisterForItemSlot<T>() where T : IItemSlotHudBarProvider, new() => RegisterForItemSlot(new T());
    public static void RegisterForItemSlot(IItemSlotHudBarProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        
        ItemSlotHudBarProviders.Add(provider);
    }

    public override bool ShouldLoad(EnumAppSide forSide) => (forSide & EnumAppSide.Client) != 0;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        AutoSetup(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        AutoAssetsLoaded(api);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);

        var toolsmithEnabled = api.ModLoader.IsModEnabled("toolsmith");
        if (toolsmithEnabled)
        {
            RegisterForItemSlot<ToolSmithDurabilityHudBarProvider>();
        }
        else RegisterForItemSlot<DurabilityHudBarProvider>();

        RegisterForItemSlot<FoodPortionsHudBarProvider>();
        RegisterForItemSlot<LiquidHudBarProvider>();
        RegisterForItemSlot<ConditionHudBarProvider>();
        RegisterForItemSlot<BagHudBarProvider>();

        if (toolsmithEnabled)
        {
            RegisterForItemSlot<ToolSmithSharpnessHudBarProvider>();
        }

        api.Event.PlayerJoin += OnPlayerJoin;
    }

    private static void OnPlayerJoin(IClientPlayer byPlayer)
    {
        if(byPlayer.Entity?.World is not IClientWorldAccessor clientWorld || clientWorld.Player != byPlayer) return;

        if(!clientWorld.Player.InventoryManager.Inventories.TryGetValue($"backpack-{byPlayer.PlayerUID}", out var bagInventory)) return;
        bagInventory.SlotModified += slotNr =>
        {
            if(bagInventory[slotNr] is not ItemSlotBagContent bagContentSlot) return;
            bagInventory.MarkSlotDirty(bagContentSlot.BagIndex);
        };
    }

    public override void Dispose()
    {
        base.Dispose();
        AutoDispose();
    }
}
