using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;

namespace FishingTweaks;

/// <summary>
///     Contains functionality for automatically casting the fishing rod.
///     This part of the mod allows players to toggle auto-casting and
///     automatically casts the fishing rod when conditions are met.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Tracks whether auto-casting is currently enabled.
    /// </summary>
    private bool _autoCasting;

    /// <summary>
    ///     Handles button press events to toggle auto-casting.
    ///     When the configured toggle key is pressed, this method switches
    ///     the auto-casting state and displays a notification to the player.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void ToggleCastingOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (e.Button != _config.ToggleAutoCasting) return;

        // Toggle the auto-casting state
        _autoCasting = !_autoCasting;

        // Display a notification to the player
        var msg = Helper.Translation.Get(_autoCasting ? "casting.start" : "casting.stop");
        Game1.addHUDMessage(HUDMessage.ForCornerTextbox(msg));
    }

    /// <summary>
    ///     Handles the update tick to perform auto-casting and skip holding fish animation.
    ///     When auto-casting is enabled, this method automatically casts the fishing rod
    ///     if the player is holding one and not already fishing.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void AutoFishingOnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (_autoCasting is false) return;
        if (!Context.IsWorldReady) return;
        if (Game1.player?.CurrentTool is not FishingRod fishingRod) return;

        CastRod(fishingRod);
        SkipFishShowing(fishingRod);
    }

    /// <summary>
    ///     Casts the fishing rod at the player's current position.
    ///     This method handles the actual casting of the fishing rod.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to cast.</param>
    private static void CastRod(FishingRod fishingRod)
    {
        if (fishingRod.inUse()) return;
        if (Game1.player?.canMove == false) return;
        if (Game1.activeClickableMenu is not null) return;

        // Cast the fishing rod at the player's current position
        fishingRod.beginUsing(Game1.currentLocation, 0, 0, Game1.player);
        fishingRod.castingPower = 1.0f;
    }

    /// <summary>
    ///     Skips the holding fish animation after catching a fish.
    /// </summary>
    /// <param name="fishingRod">The fishing rod that caught the fish.</param>
    private static void SkipFishShowing(FishingRod fishingRod)
    {
        if (!fishingRod.fishCaught) return;
        if (Game1.player?.canMove == true) return;
        if (Game1.isFestival()) return;

        fishingRod.doneHoldingFish(Game1.player);
    }
}