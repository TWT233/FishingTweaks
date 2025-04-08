using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FishingTweaks;

/// <summary>
///     Configuration class for the mod.
///     Contains all configurable options for the mod, including auto-casting,
///     auto-baiting, treasure collection, and fish animation skipping.
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    ///     The key to toggle auto-casting functionality.
    ///     When pressed, this key will enable or disable the automatic fishing rod casting feature.
    ///     Default is V key.
    /// </summary>
    public SButton ToggleAutoCasting { get; set; } = SButton.V;

    /// <summary>
    ///     Whether to enable skipping the fish showing animation.
    ///     When enabled, the mod will skip the animation that plays after catching a fish.
    /// </summary>
    public bool EnableSkipFishShowing { get; set; } = true;

    /// <summary>
    ///     Whether to enable automatic treasure collection.
    ///     When enabled, the mod will automatically collect items from treasure chests
    ///     during the fishing minigame.
    /// </summary>
    public bool EnableGrabTreasure { get; set; } = true;

    /// <summary>
    ///     Whether to enable auto-baiting.
    ///     When enabled, the mod will automatically apply bait to the fishing rod
    ///     when it runs out, using the first available bait from the player's inventory.
    /// </summary>
    public bool EnableAutoBaiting { get; set; } = true;
}

/// <summary>
///     Contains functionality for integrating with Generic Mod Config Menu.
///     This allows players to configure the mod through a user-friendly interface.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Sets up the Generic Mod Config Menu integration when the game is launched.
    ///     Registers the mod's configuration options with the config menu, including
    ///     auto-casting toggle key, auto-baiting, treasure collection, and fish animation skipping.
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
            () => _config.ToggleAutoCasting,
            value => _config.ToggleAutoCasting = value,
            () => Helper.Translation.Get("config.toggle-auto-casting"),
            () => Helper.Translation.Get("config.toggle-auto-casting.tooltip")
        );

        configMenu.AddBoolOption(
            ModManifest,
            () => _config.EnableSkipFishShowing,
            value => _config.EnableSkipFishShowing = value,
            () => Helper.Translation.Get("config.enable-skip-fish-showing"),
            () => Helper.Translation.Get("config.enable-skip-fish-showing.tooltip")
        );

        configMenu.AddBoolOption(
            ModManifest,
            () => _config.EnableGrabTreasure,
            value => _config.EnableGrabTreasure = value,
            () => Helper.Translation.Get("config.enable-grab-treasure"),
            () => Helper.Translation.Get("config.enable-grab-treasure.tooltip")
        );

        configMenu.AddBoolOption(
            ModManifest,
            () => _config.EnableAutoBaiting,
            value => _config.EnableAutoBaiting = value,
            () => Helper.Translation.Get("config.enable-auto-baiting"),
            () => Helper.Translation.Get("config.enable-auto-baiting.tooltip")
        );
    }
}