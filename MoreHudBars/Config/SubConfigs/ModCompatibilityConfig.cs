using MoreHudBars.Info;
using System.ComponentModel;

namespace MoreHudBars.Config.SubConfigs;

public class ModCompatibilityConfig
{
    /// <summary>
    /// The configuration of the filled bar on bags
    /// </summary>
    [Category("ToolSmith")]
    public HudBarConfig ToolSmithSharpnessBar { get; set; } = new()
    {
        Color = new(0.0, 0.0, 0.5, 0.5),
        HudBarType = EHudBarType.Horizontal,
        AllowColorOverride = true
    };
}
