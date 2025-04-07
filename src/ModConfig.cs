using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FishingTweaks;

/// <summary>
///     Configuration class for the mod.
///     Contains all configurable options for the mod.
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    ///     The key to toggle auto-casting functionality.
    ///     Default is V key.
    /// </summary>
    public SButton ToggleAutoCasting { get; set; } = SButton.V;
}

/// <summary>
///     Contains functionality for integrating with Generic Mod Config Menu.
///     This allows players to configure the mod through a user-friendly interface.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Sets up the Generic Mod Config Menu integration when the game is launched.
    ///     Registers the mod's configuration options with the config menu.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void SetupGMCMOnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // Get Generic Mod Config Menu's API (if it's installed)
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        // Register the mod with the config menu
        configMenu.Register(
            ModManifest,
            () => _config = new ModConfig(),
            () => Helper.WriteConfig(_config)
        );

        // Add config options
        configMenu.AddKeybind(
            ModManifest,
            name: () => Helper.Translation.Get("config.toggle-auto-casting"),
            tooltip: () => Helper.Translation.Get("config.toggle-auto-casting.tooltip"),
            getValue: () => _config.ToggleAutoCasting,
            setValue: value => _config.ToggleAutoCasting = value
        );
    }
}