using Cairo;
using HarmonyLib;
using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Info;
using MoreHudBars.Providers;
using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace MoreHudBars.Patches;

[HarmonyPatch]
public static class ComposeSlotOverlaysPatch
{


    [HarmonyPatch(typeof(GuiElementItemSlotGridBase), "ComposeSlotOverlays")]
    [HarmonyPrefix]
    public static bool ComposeSlotOverlaysPrefix(GuiElementItemSlotGridBase __instance, ItemSlot slot, int slotId, int slotIndex, ref bool __result, OrderedDictionary<int, ItemSlot> ___availableSlots, LoadedTexture[] ___slotQuantityTextures, ICoreClientAPI ___api)
	{
		if (!___availableSlots.ContainsKey(slotId))
		{
            __result = false;
			return false;
		}
		if (slot.Itemstack == null)
		{
            __result = true;
			return false;
		}

        if(!TryDrawExtraHuds(__instance, ___slotQuantityTextures, slot, slotIndex, ___api))
        {
            //Nothing drawn
            ___slotQuantityTextures[slotIndex].Dispose();
			___slotQuantityTextures[slotIndex] = new LoadedTexture(___api);
        }

        __result = true;
		return false;
	}

    const float epsilon = 0.005f;

    public static bool TryDrawExtraHuds(GuiElementItemSlotGridBase instance, LoadedTexture[] slotQuantityTextures, ItemSlot slot, int slotIndex, ICoreClientAPI capi)
    {
        if(
               slot.Itemstack?.Collectible is null
            || slot is ItemSlotCreative
            || instance.SlotBounds.Length <= slotIndex 
            || instance.SlotBounds[slotIndex] is not ElementBounds slotBounds
        ) return false;

        var world = capi.World;
        
        using ImageSurface textSurface = new(Format.Argb32, (int)slotBounds.InnerWidth, (int)slotBounds.InnerHeight);
        using Context textCtx = GuiElement.GenContext(textSurface);
        textCtx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        textCtx.Paint();
        bool didSomething = false;

        var space = new AllowedSpace
        {
            XEnd = slotBounds.InnerWidth,
            YEnd = slotBounds.InnerHeight
        };

        foreach(var provider in MoreHudBarsModSystem.ItemSlotHudBarProviders.OrderBy(OrderProviders))
        {
            var config = provider.Config;
            if (!config.Enabled || !provider.TryGetPercentage(world, slot, out var percentage)) continue;
            if (!config.ShowEvenIfBarFull && Math.Abs(percentage - 1f) < epsilon) continue;
            var color = GetColor(provider, config, slot, percentage);
            switch (config.HudBarType)
            {
                case EHudBarType.FloodFill:
                    RenderFloodFill(textCtx, slotBounds, config, percentage, color);
                    break;

                case EHudBarType.Vertical:
                    RenderVertical(instance, textCtx, slotBounds, config, percentage, color, space);
                    break;

                default:
                    RenderHorizontal(instance, textCtx, slotBounds, config, percentage, color, space);
                    break;
            }
            didSomething = true;
        }
        
        if (didSomething)
        {
            capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures[slotIndex]);
            return true;
        }

        return false;
    }

    private static object OrderProviders(IItemSlotHudBarProvider provider) => provider.Config.HudBarType switch
    {
        EHudBarType.FloodFill => 0,
        EHudBarType.Horizontal => 1,
        EHudBarType.Vertical => 2,
        _ => 3
    };

    private static Color GetColor(IItemSlotHudBarProvider provider, HudBarConfig config, ItemSlot slot, float percentage) => config.AllowColorOverride ? provider.GetColorOVerride(slot, percentage) ?? config.Color : config.Color;

    private static void RenderFloodFill(Context textCtx, ElementBounds slotBounds, HudBarConfig config, float percentage, Color color)
    {
        var width = slotBounds.InnerWidth;
        var height = slotBounds.InnerHeight;
        var x = 0d;
        var y = 0d;

        if((config.HudBarStyle & EHudBarStyle.AlternativeAlignment) == 0)
        {
            width -= GuiElement.scaled(4);
            x += GuiElement.scaled(2);

            height -= GuiElement.scaled(4);
            y += GuiElement.scaled(2);
        }

        textCtx.SetSourceColor(color);
        var targetY = height * percentage;
        if((config.HudBarStyle & EHudBarStyle.Reverse) != 0)
        {
            GuiElement.Rectangle(textCtx, x, y, width, targetY);
        }
        else GuiElement.Rectangle(textCtx, x, height + y, width, -targetY);
        textCtx.FillPreserve();
    }

    private static void RenderHorizontal(GuiElementItemSlotGridBase instance, Context textCtx, ElementBounds slotBounds, HudBarConfig config, float percentage, Color color, AllowedSpace space)
    {
        var invertAlignment = (config.HudBarStyle & EHudBarStyle.AlternativeAlignment) != 0;
        //Background bar
        double x = GuiElement.scaled(4.0);
        double y = !invertAlignment 
            ? slotBounds.InnerHeight - GuiElement.scaled(3.0) - GuiElement.scaled(4.0)
            : GuiElement.scaled(3.0);
        textCtx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
        double width = slotBounds.InnerWidth - GuiElement.scaled(8.0);
        double height = GuiElement.scaled(4.0);

        space.TryReserve(ref x, ref y, ref width, ref height, domimantSide: EXYDominant.X, invertDirection: invertAlignment);
        
        if((config.HudBarStyle & EHudBarStyle.NoBackground) == 0)
        {
            GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
            textCtx.FillPreserve();
            instance.ShadePath(textCtx, 2.0);
        }
        //Foreground bar
        textCtx.SetSourceColor(color);
        width *= percentage;

        if((config.HudBarStyle & EHudBarStyle.Reverse) != 0)
        {
            GuiElement.RoundRectangle(textCtx, slotBounds.InnerWidth - x, y, -width, height, 1.0);
        }
        else GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
        textCtx.FillPreserve();
        instance.ShadePath(textCtx, 2.0);
    }

    private static void RenderVertical(GuiElementItemSlotGridBase instance, Context textCtx, ElementBounds slotBounds, HudBarConfig config, float percentage, Color color, AllowedSpace space)
    {
        var invertAlignment = (config.HudBarStyle & EHudBarStyle.AlternativeAlignment) != 0;
        //Background bar
        double x = invertAlignment 
            ? slotBounds.InnerWidth - GuiElement.scaled(3.0) - GuiElement.scaled(4.0)
            : GuiElement.scaled(3.0);
        double y = GuiElement.scaled(4.0);
        textCtx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
        double width = GuiElement.scaled(4.0);
        double height = slotBounds.InnerHeight - GuiElement.scaled(8.0);

        space.TryReserve(ref x, ref y, ref width, ref height, domimantSide: EXYDominant.Y, invertDirection: !invertAlignment);
        
        if((config.HudBarStyle & EHudBarStyle.NoBackground) == 0)
        {
            GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
            textCtx.FillPreserve();
            instance.ShadePath(textCtx, 2.0);
        }
        //Foreground bar
        textCtx.SetSourceColor(color);
        height *= percentage;

        if((config.HudBarStyle & EHudBarStyle.Reverse) != 0)
        {
            GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
        }
        else GuiElement.RoundRectangle(textCtx, x, slotBounds.InnerWidth - y, width, -height, 1.0);
        textCtx.FillPreserve();
        instance.ShadePath(textCtx, 2.0);
    }
}