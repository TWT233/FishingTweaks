using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace FishingTweaks;

/// <summary>
///     Contains functionality for automatically casting the fishing rod and managing fishing-related features.
///     This part of the mod includes auto-casting, auto-baiting, and fish animation skipping.
///     It also handles the auto-hook enchantment for automatic fish pulling.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Tracks whether auto-fishing is currently enabled.
    ///     This state can be toggled using the configured key (default: V).
    /// </summary>
    private bool _autoFishing;

    /// <summary>
    ///     Handles button press events to toggle auto-fishing.
    ///     When the configured toggle key is pressed, this method switches
    ///     the auto-fishing state and displays a notification to the player.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data containing the pressed button information.</param>
    private void ToggleAutoFishingOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (e.Button != _config.ToggleAutoFishing) return;

        // Toggle the auto-fishing state
        _autoFishing = !_autoFishing;

        // Display a notification to the player
        var msg = Helper.Translation.Get(_autoFishing ? "fishing.start" : "fishing.stop");
        Game1.addHUDMessage(HUDMessage.ForCornerTextbox(msg));
    }

    /// <summary>
    ///     Handles the update tick to perform auto-fishing and manage fishing-related features.
    ///     This method coordinates all automatic fishing features including:
    ///     - Auto-casting the fishing rod
    ///     - Auto-applying bait
    ///     - Skipping fish animations
    ///     - Applying auto-hook enchantment
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void AutoFishingOnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!_autoFishing) return;
        if (!Context.IsWorldReady) return;
        if (Game1.player?.CurrentTool is not FishingRod fishingRod) return;

        AutoBaiting(fishingRod);
        AutoTackling(fishingRod);
        AutoCasting(fishingRod);

        ApplyAutoHook(fishingRod);
        SkipFishShowing(fishingRod);
    }

    /// <summary>
    ///     Automatically applies the auto-hook enchantment to the fishing rod.
    ///     This method adds the AutoHookEnchantment to the fishing rod if it doesn't already have one,
    ///     which allows fish to be automatically hooked when they bite, without requiring player input.
    ///     The method only runs when auto-fishing is enabled and the EnableAutoHook configuration option is set to true.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to apply the auto-hook enchantment to.</param>
    private void ApplyAutoHook(FishingRod fishingRod)
    {
        if (!_config.EnableAutoHook) return;
        // Apply auto hook enchantment for auto pull
        if (!fishingRod.hasEnchantmentOfType<AutoHookEnchantment>())
            fishingRod.enchantments.Add(new AutoHookEnchantment());
    }

    /// <summary>
    ///     Skips the fish showing animation after catching a fish.
    ///     This method is called when a fish is caught and the player
    ///     has enabled the skip fish showing feature. It completes the
    ///     holding fish animation immediately.
    /// </summary>
    /// <param name="fishingRod">The fishing rod that caught the fish.</param>
    private void SkipFishShowing(FishingRod fishingRod)
    {
        if (!_autoFishing) return;
        if (!_config.EnableSkipFishShowing) return;
        if (!fishingRod.fishCaught) return;
        if (Game1.player?.canMove == true) return;
        if (Game1.isFestival()) return;

        fishingRod.doneHoldingFish(Game1.player);
    }
}
