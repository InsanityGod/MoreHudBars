using Cairo;
using HarmonyLib;
using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Info;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MoreHudBars.Patches;

[HarmonyPatch]
public static class ComposeSlotOverlaysPatch
{
    [HarmonyPatch(typeof(GuiElementItemSlotGridBase), "ComposeSlotOverlays")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);
        matcher.Start();
        matcher.MatchEndForward(
            new CodeMatch(instruction => instruction is { operand: MethodInfo { Name: "ShouldDisplayItemDamage" }  }),
            new CodeMatch(OpCodes.Stloc_0)
        );

        matcher.DefineLabel(out var originalPathLabel);

        matcher.InsertAfterAndAdvance(
            CodeInstruction.LoadArgument(0), //instance
            CodeInstruction.LoadArgument(1), //itemSlot
            CodeInstruction.LoadArgument(2), //slotIndex
            CodeInstruction.LoadArgument(0),
            CodeInstruction.LoadField(typeof(GuiElement), "api"), //capi
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ComposeSlotOverlaysPatch), nameof(TryDrawExtraHuds))),
            new CodeInstruction(OpCodes.Brfalse, originalPathLabel),
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Ret)
        );
        matcher.Advance(1);
        matcher.Labels.Add(originalPathLabel);

        return matcher.InstructionEnumeration();
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "slotQuantityTextures")]
    internal static extern ref LoadedTexture[] slotQuantityTextures(GuiElementItemSlotGridBase instance);

    public static bool TryDrawExtraHuds(GuiElementItemSlotGridBase instance, ItemSlot slot, int slotIndex, ICoreClientAPI capi)
    {
        if(slot.Itemstack?.Collectible is not { } collectible ||  collectible.GetMaxDurability(slot.Itemstack) != 0) return false;
        if(slot is ItemSlotCreative || instance.SlotBounds.Length <= slotIndex) return false; //no support for multiple bars atm
        var slotBounds = instance.SlotBounds[slotIndex];
        if(slotBounds is null) return false;

        var world = capi.World;
        foreach(var provider in MoreHudBarsModSystem.ItemSlotHudBarProviders)
        {
            var config = provider.Config;
            if (!config.Enabled || !provider.TryGetPercentage(world, slot, out var percentage)) continue;
            if (!config.ShowEvenIfBarFull && (int)percentage == 1) continue;

            switch (config.HudBarType)
            {
                case EHudBarType.FloodFill:
                    RenderFloodFill(instance, slotIndex, capi, slotBounds, config, percentage);
                    break;

                case EHudBarType.Vertical:
                    RenderVertical(instance, slotIndex, capi, slotBounds, config, percentage);
                    break;

                default:
                    RenderHorizontal(instance, slotIndex, capi, slotBounds, config, percentage);
                    break;
            }

            

            return true; //no support for multiple bars atm
        }

        return false;
    }

    private static void RenderFloodFill(GuiElementItemSlotGridBase instance, int slotIndex, ICoreClientAPI capi, ElementBounds slotBounds, HudBarConfig config, float percentage)
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

        textCtx.SetSourceColor(config.Color);
        var targetY = height * percentage;
        if((config.HudBarStyle & EHudBarStyle.Reverse) != 0)
        {
            GuiElement.Rectangle(textCtx, x, y, width, targetY);
        }
        else GuiElement.Rectangle(textCtx, x, height + y, width, -targetY);
        textCtx.FillPreserve();


        capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures(instance)[slotIndex]);
    }

    private static void RenderHorizontal(GuiElementItemSlotGridBase instance, int slotIndex, ICoreClientAPI capi, ElementBounds slotBounds, Config.SubConfigs.HudBarConfig config, float percentage)
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
        textCtx.SetSourceColor(config.Color);
        width *= percentage;

        if((config.HudBarStyle & EHudBarStyle.Reverse) != 0)
        {
            GuiElement.RoundRectangle(textCtx, textSurface.Width - x, y, -width, height, 1.0);
        }
        else GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
        textCtx.FillPreserve();
        instance.ShadePath(textCtx, 2.0);
        
        
        capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures(instance)[slotIndex]);
    }

    private static void RenderVertical(GuiElementItemSlotGridBase instance, int slotIndex, ICoreClientAPI capi, ElementBounds slotBounds, Config.SubConfigs.HudBarConfig config, float percentage)
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
        textCtx.SetSourceColor(config.Color);
        height *= percentage;

        if((config.HudBarStyle & EHudBarStyle.Reverse) != 0)
        {
            GuiElement.RoundRectangle(textCtx, x, y, width, height, 1.0);
        }
        else GuiElement.RoundRectangle(textCtx, x, textSurface.Height - y, width, -height, 1.0);
        textCtx.FillPreserve();
        instance.ShadePath(textCtx, 2.0);
        
        
        capi.Gui.LoadOrUpdateCairoTexture(textSurface, true, ref slotQuantityTextures(instance)[slotIndex]);
    }
}