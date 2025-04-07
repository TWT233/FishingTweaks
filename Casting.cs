using Microsoft.Xna.Framework.Input;
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
    ///     Right-clicking while holding a fishing rod toggles the auto-cast feature.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void ToggleCastingOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (e.Button != Keys.V.ToSButton()) return;

        // Toggle the auto-casting state
        _autoCasting = !_autoCasting;
        
    }

    /// <summary>
    ///     Handles the one-second update tick to perform auto-casting.
    ///     When auto-casting is enabled, this method automatically casts the fishing rod
    ///     if the player is holding one and not already fishing.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void CastingOnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (_autoCasting is false) return;
        if (Game1.player.CurrentTool is not FishingRod fishingRod) return;
        if (fishingRod.inUse()) return;

        // Cast the fishing rod at the player's current position
        fishingRod.beginUsing(Game1.currentLocation, 0, 0, Game1.player);
    }
}