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
    public FishCounter FishCounter = new();

    /// <summary>
    ///     The key to toggle auto-fishing functionality.
    ///     When pressed, this key will enable or disable the automatic fishing feature.
    ///     Default is V key.
    /// </summary>
    public SButton ToggleAutoFishing { get; set; } = SButton.V;

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

    /// <summary>
    ///     Whether to enable auto-tackling.
    ///     When enabled, the mod will automatically apply tackle to the fishing rod
    ///     when it breaks, using the first available tackle from the player's inventory.
    /// </summary>
    public bool EnableAutoTackling { get; set; } = true;

    /// <summary>
    ///     The minimum stamina required for auto-fishing.
    ///     When player's stamina falls below this value, auto-fishing will be disabled
    ///     and a notification will be shown.
    /// </summary>
    public int MinStaminaForAutoFishing { get; set; } = 10;


    public int MinCatchCountForSkipFishing { get; set; } = 5;

    public int MinPerfectCountForSkipFishing { get; set; } = 0;


    public bool SatisfiedSkipFishing(string whichFish)
    {
        return FishCounter.Satisfied(whichFish, MinCatchCountForSkipFishing, MinPerfectCountForSkipFishing);
    }
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
    private void SetupGMCMOnGameLaunched(object? sender, GameLaunchedEventArgs? e)
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
            () => _config.ToggleAutoFishing,
            value => _config.ToggleAutoFishing = value,
            () => Helper.Translation.Get("config.toggle-auto-fishing"),
            () => Helper.Translation.Get("config.toggle-auto-fishing.tooltip")
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

        configMenu.AddBoolOption(
            ModManifest,
            () => _config.EnableAutoTackling,
            value => _config.EnableAutoTackling = value,
            () => Helper.Translation.Get("config.enable-auto-tackling"),
            () => Helper.Translation.Get("config.enable-auto-tackling.tooltip")
        );

        configMenu.AddNumberOption(
            ModManifest,
            () => _config.MinStaminaForAutoFishing,
            value => _config.MinStaminaForAutoFishing = value,
            () => Helper.Translation.Get("config.min-stamina-for-auto-fishing"),
            () => Helper.Translation.Get("config.min-stamina-for-auto-fishing.tooltip")
        );

        configMenu.AddNumberOption(
            ModManifest,
            () => _config.MinCatchCountForSkipFishing,
            value => _config.MinCatchCountForSkipFishing = value,
            () => Helper.Translation.Get("config.min-catch-count-for-skip-fishing"),
            () => Helper.Translation.Get("config.min-catch-count-for-skip-fishing.tooltip")
        );

        configMenu.AddNumberOption(
            ModManifest,
            () => _config.MinPerfectCountForSkipFishing,
            value => _config.MinPerfectCountForSkipFishing = value,
            () => Helper.Translation.Get("config.min-perfect-count-for-skip-fishing"),
            () => Helper.Translation.Get("config.min-perfect-count-for-skip-fishing.tooltip")
        );

        _config.FishCounter.ArrangeMenu(ModManifest, Helper, configMenu);
    }

    private void IncrFishCounter(string whichFish, bool isPerfect, int increment = 1)
    {
        _config.FishCounter.Incr(whichFish, isPerfect, increment);
        Helper.WriteConfig(_config);

        Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu")
            ?.Unregister(ModManifest);
        SetupGMCMOnGameLaunched(null, null);
    }
}