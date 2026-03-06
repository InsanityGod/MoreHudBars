using Cairo;
using MoreHudBars.Info;
using System.ComponentModel;
using Vintagestory.API.Client;

namespace MoreHudBars.Config.SubConfigs;

public class HudBarConfig
{
    /// <summary>
    /// Whether the status bar should be enabled
    /// </summary>
    [DefaultValue(true)]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The color that should be used for this status bar.
    /// </summary>
    public required Color Color { get; set; }

    /// <summary>
    /// Wether the bar should show up even if it is at 100% filled.
    /// </summary>
    public bool ShowEvenIfBarFull { get; set; }

    /// <summary>
    /// The type of bar that should be used
    /// </summary>
    public EHudBarType HudBarType { get; set;}

    /// <summary>
    /// Extra styling rules, some of these might not be applicable depending on the <see cref="HudBarType"/>
    /// </summary>
    public EHudBarStyle HudBarStyle { get; set;}

}
