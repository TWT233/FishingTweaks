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


    private void ToggleCastingOnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (e.Button != _config.ToggleAutoCasting) return;

        // Toggle the auto-casting state
        _autoCasting = !_autoCasting;

        var msg = Helper.Translation.Get(_autoCasting ? "casting.start" : "casting.stop");
        Game1.addHUDMessage(HUDMessage.ForCornerTextbox(msg));
    }

    private void CastingOnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (_autoCasting is false) return;
        if (!Context.IsWorldReady) return;
        if (Game1.player?.CurrentTool is not FishingRod fishingRod) return;

        CastRod(fishingRod);
        SkipHoldingFish(fishingRod);
    }

    private static void CastRod(FishingRod fishingRod)
    {
        if (fishingRod.inUse()) return;
        if (Game1.player?.canMove == false) return;
        if (Game1.activeClickableMenu is not null) return;

        // Cast the fishing rod at the player's current position
        fishingRod.beginUsing(Game1.currentLocation, 0, 0, Game1.player);
        fishingRod.castingPower = 1.0f;
    }

    private static void SkipHoldingFish(FishingRod fishingRod)
    {
        if (!fishingRod.fishCaught) return;
        if (Game1.player?.canMove == true) return;
        if (Game1.isFestival()) return;

        fishingRod.doneHoldingFish(Game1.player);
    }
}