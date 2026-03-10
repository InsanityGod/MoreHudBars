using MoreHudBars.Config.SubConfigs;
using MoreHudBars.Info;

namespace MoreHudBars.Config;

public class MoreHudBarsConfig
{
    public static MoreHudBarsConfig? Instance { get; internal set; }

    /// <summary>
    /// The configuration of the durability bar
    /// </summary>
    public HudBarConfig DurabilityBar { get; set; } = new()
    {
        Color = new(0.482, 0.521, 0.211, 0.5),
        AllowColorOverride = true,
    };

    /// <summary>
    /// The configuration of the condition bar shown on wearable items
    /// </summary>
    public HudBarConfig ClothingConditionBar { get; set; } = new()
    {
        Color = new(1.0, 0.5, 0.0, 0.7),
        HudBarType = EHudBarType.Vertical,
        AllowColorOverride = true,
    };

    /// <summary>
    /// The configuration of the food portion bar shown on meals
    /// </summary>
    public HudBarConfig FoodPortionBar { get; set; } = new()
    {
        Color = new(0.482, 0.521, 0.211, 0.5)
    };

    /// <summary>
    /// The configuration of the liquid bar shown on liquid containers
    /// </summary>
    public HudBarConfig LiquidBar { get; set; } = new()
    {
        Color = new(0, 0.4, 0.5, 0.5),
        HudBarType = EHudBarType.Vertical,
        ShowEvenIfBarFull = true,
    };

    /// <summary>
    /// The configuration of the filled bar on bags
    /// </summary>
    public HudBarConfig BagBar { get; set; } = new()
    {
        Color = new(1.0, 0.5, 0.0, 0.5),
        HudBarType = EHudBarType.FloodFill,
        ShowEvenIfBarFull = true
    };

    /// <summary>
    /// Configuration regarding compatibility with other mods
    /// </summary>
    public ModCompatibilityConfig Compatibility {  get; set; } = new();
}
