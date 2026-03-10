using HarmonyLib;
using MoreHudBars.Config;
using MoreHudBars.Providers;
using MoreHudBars.Providers.ItemSlotProviders;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Common;

namespace MoreHudBars;

public class MoreHudBarsModSystem : ModSystem
{
    internal static List<IItemSlotHudBarProvider> ItemSlotHudBarProviders { get; } = [];

    public static void RegisterForItemSlot<T>() where T : IItemSlotHudBarProvider, new() => RegisterForItemSlot(new T());
    public static void RegisterForItemSlot(IItemSlotHudBarProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        ItemSlotHudBarProviders.Add(provider);
    }

    public override bool ShouldLoad(EnumAppSide forSide) => (forSide & EnumAppSide.Client) != 0;

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        LoadModConfig(api);
        var toolsmithEnabled = api.ModLoader.IsModEnabled("toolsmith");
        if (!Harmony.HasAnyPatches(Mod.Info.ModID))
        {
            new Harmony(Mod.Info.ModID).PatchAllUncategorized();
        }

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

    private void LoadModConfig(ICoreAPI api)
    {
        try
        {
            MoreHudBarsConfig.Instance = api.LoadModConfig<MoreHudBarsConfig>("MoreHudBarsConfig.json");
            if(MoreHudBarsConfig.Instance is null)
            {
                MoreHudBarsConfig.Instance = new MoreHudBarsConfig();
                api.StoreModConfig(MoreHudBarsConfig.Instance, "MoreHudBarsConfig.json");
            }
        }
        catch(Exception ex)
        {
            Mod.Logger.Error("Failed to load 'MoreHudBarsConfig.json', using default values: {0}", ex);
            MoreHudBarsConfig.Instance = new MoreHudBarsConfig();
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        new Harmony(Mod.Info.ModID).UnpatchAll();
        ItemSlotHudBarProviders.Clear();
    }
}
