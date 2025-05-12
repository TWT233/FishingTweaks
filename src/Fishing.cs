using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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
    ///     The original bite time of the fishing rod.
    /// </summary>
    private float originalTimeUntilFishingBite = -1f;

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
    ///     Handles auto-hook functionality for the fishing rod.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to handle auto-hook.</param>
    private void ApplyAutoHook(FishingRod fishingRod)
    {
        if (!_config.EnableAutoHook) return;
        if (!fishingRod.isFishing) return;

        // reset the nibble accumulator
        // to prevent fish stop nibbling
        // until hooking is done
        if (fishingRod.isNibbling)
        {
            fishingRod.fishingNibbleAccumulator = 0;
        }

        if (!Equals(fishingRod.timeUntilFishingBite, -1f))
        {
            // make the in-game bite time 5 secs longer
            // to prevent race condition with the mod
            if (Equals(originalTimeUntilFishingBite, -1f))
            {
                originalTimeUntilFishingBite = fishingRod.timeUntilFishingBite;
                fishingRod.timeUntilFishingBite += 5 * 1000f;
            }

            // the mod however will use the original bite time
            // so we need to reset it when the bite is over
            if (fishingRod.fishingBiteAccumulator > originalTimeUntilFishingBite)
            {
                originalTimeUntilFishingBite = -1f;

                fishingRod.fishingBiteAccumulator = 0f;
                fishingRod.timeUntilFishingBite = -1f;
                fishingRod.isNibbling = true;

                // also, let's keep the nibble effect
                Game1.player.PlayFishBiteChime();
                Rumble.rumble(0.75f, 250f);
                fishingRod.timeUntilFishingNibbleDone = FishingRod.maxTimeToNibble;
                Point standingPixel3 = Game1.player.StandingPixel;
                Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(395, 497, 3, 8), new Vector2(standingPixel3.X - Game1.viewport.X, standingPixel3.Y - 128 - 8 - Game1.viewport.Y), flipped: false, 0.02f, Color.White)
                {
                    scale = 5f,
                    scaleChange = -0.01f,
                    motion = new Vector2(0f, -0.5f),
                    shakeIntensityChange = -0.005f,
                    shakeIntensity = 1f
                });

                // now, ftw
                fishingRod.timePerBobberBob = 1f;
                fishingRod.timeUntilFishingNibbleDone = FishingRod.maxTimeToNibble;
                fishingRod.DoFunction(Game1.player.currentLocation, (int)fishingRod.bobber.X, (int)fishingRod.bobber.Y, 1, Game1.player);
            }
        }
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
