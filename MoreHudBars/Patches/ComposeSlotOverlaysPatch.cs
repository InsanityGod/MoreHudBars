using Cairo;
using HarmonyLib;
using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Info;
using MoreHudBars.Providers;
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

    public static bool TryDrawExtraHuds(GuiElementItemSlotGridBase instance, LoadedTexture[] slotQuantityTextures, ItemSlot slot, int slotIndex, ICoreClientAPI capi)
    {
        if(slot.Itemstack?.Collectible is null) return false;
        if(slot is ItemSlotCreative || instance.SlotBounds.Length <= slotIndex) return false; //no support for multiple bars atm
        var slotBounds = instance.SlotBounds[slotIndex];
        if(slotBounds is null) return false;

        var world = capi.World;
        foreach(var provider in MoreHudBarsModSystem.ItemSlotHudBarProviders)
        {
            var config = provider.Config;
            if (!config.Enabled || !provider.TryGetPercentage(world, slot, out var percentage)) continue;
            if (!config.ShowEvenIfBarFull && (int)percentage == 1) continue;
            var color = GetColor(provider, config, slot);
            switch (config.HudBarType)
            {
                case EHudBarType.FloodFill:
                    RenderFloodFill(instance, slotQuantityTextures, slotIndex, capi, slotBounds, config, percentage, color);
                    break;

                case EHudBarType.Vertical:
                    RenderVertical(instance, slotQuantityTextures, slotIndex, capi, slotBounds, config, percentage, color);
                    break;

                default:
                    RenderHorizontal(instance, slotQuantityTextures, slotIndex, capi, slotBounds, config, percentage, color);
                    break;
            }

            return true; //no support for multiple bars atm
        }

        return false;
    }

    private static Color GetColor(IItemSlotHudBarProvider provider, HudBarConfig config, ItemSlot slot) => config.AllowColorOverride ? provider.GetColorOVerride(slot) ?? config.Color : config.Color;

    private static void RenderFloodFill(GuiElementItemSlotGridBase instance, LoadedTexture[] slotQuantityTextures, int slotIndex, ICoreClientAPI capi, ElementBounds slotBounds, HudBarConfig config, float percentage, Color color)
    {
        using ImageSurface textSurface = new(Format.Argb32, (int)slotBounds.InnerWidth, (int)slotBounds.InnerHeight);
        using Context textCtx = GuiElement.GenContext(textSurface);
        textCtx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        textCtx.Paint();
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


        capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures[slotIndex]);
    }

    private static void RenderHorizontal(GuiElementItemSlotGridBase instance, LoadedTexture[] slotQuantityTextures, int slotIndex, ICoreClientAPI capi, ElementBounds slotBounds, HudBarConfig config, float percentage, Color color)
    {
        using ImageSurface textSurface = new(Format.Argb32, (int)slotBounds.InnerWidth, (int)slotBounds.InnerHeight);
        using Context textCtx = GuiElement.GenContext(textSurface);
        textCtx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        textCtx.Paint();

        //Background bar
        double x = GuiElement.scaled(4.0);
        double y = (config.HudBarStyle & EHudBarStyle.AlternativeAlignment) == 0 
            ? slotBounds.InnerHeight - GuiElement.scaled(3.0) - GuiElement.scaled(4.0)
            : GuiElement.scaled(3.0);
        textCtx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
        double width = slotBounds.InnerWidth - GuiElement.scaled(8.0);
        double height = GuiElement.scaled(4.0);
        
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
            GuiElement.RoundRectangle(textCtx, textSurface.Width - x, y, -width, height, 1.0);
        }
        else GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
        textCtx.FillPreserve();
        instance.ShadePath(textCtx, 2.0);
        
        
        capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures[slotIndex]);
    }

    private static void RenderVertical(GuiElementItemSlotGridBase instance, LoadedTexture[] slotQuantityTextures, int slotIndex, ICoreClientAPI capi, ElementBounds slotBounds, HudBarConfig config, float percentage, Color color)
    {
        using ImageSurface textSurface = new(Format.Argb32, (int)slotBounds.InnerWidth, (int)slotBounds.InnerHeight);
        using Context textCtx = GuiElement.GenContext(textSurface);
        textCtx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        textCtx.Paint();

        //Background bar
        double x = (config.HudBarStyle & EHudBarStyle.AlternativeAlignment) != 0 
            ? slotBounds.InnerWidth - GuiElement.scaled(3.0) - GuiElement.scaled(4.0)
            : GuiElement.scaled(3.0);
        double y = GuiElement.scaled(4.0);
        textCtx.SetSourceRGBA(GuiStyle.DialogStrongBgColor);
        double width = GuiElement.scaled(4.0);
        double height = slotBounds.InnerHeight - GuiElement.scaled(8.0);
        
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
        else GuiElement.RoundRectangle(textCtx, x, textSurface.Height - y, width, -height, 1.0);
        textCtx.FillPreserve();
        instance.ShadePath(textCtx, 2.0);
        
        
        capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures[slotIndex]);
    }
}