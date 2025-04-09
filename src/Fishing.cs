using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace FishingTweaks;

/// <summary>
///     Contains functionality for automatically casting the fishing rod and managing fishing-related features.
///     This part of the mod includes auto-casting, auto-baiting, and fish animation skipping.
///     It also handles the auto-hook enchantment for automatic fish pulling.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Tracks whether auto-casting is currently enabled.
    ///     This state can be toggled using the configured key (default: V).
    /// </summary>
    private bool _autoCasting;

    /// <summary>
    ///     Handles button press events to toggle auto-casting.
    ///     When the configured toggle key is pressed, this method switches
    ///     the auto-casting state and displays a notification to the player.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data containing the pressed button information.</param>
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
    ///     Handles the update tick to perform auto-casting and manage fishing-related features.
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
        if (_autoCasting is false) return;
        if (!Context.IsWorldReady) return;
        if (Game1.player?.CurrentTool is not FishingRod fishingRod) return;

        AutoBaiting(fishingRod);
        AutoTackling(fishingRod);
        AutoCasting(fishingRod);
        SkipFishShowing(fishingRod);
    }

    /// <summary>
    ///     Automatically applies bait to the fishing rod when it runs out.
    ///     This method will use the first available bait item from the player's inventory
    ///     that is compatible with the fishing rod. When bait is applied, it displays
    ///     a notification showing the bait name and quantity.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to apply bait to.</param>
    private void AutoBaiting(FishingRod fishingRod)
    {
        if (!_config.EnableAutoBaiting) return;
        if (!fishingRod.CanUseBait()) return;
        if (fishingRod.GetBait() is not null) return;

        // Find the first bait item in the player's inventory
        var bait = Game1.player.Items.FirstOrDefault(item =>
            item is Object { Category: Object.baitCategory } o && fishingRod.canThisBeAttached(o));

        if (bait is null) return;

        // Apply the bait to the fishing rod
        fishingRod.attach(bait as Object);
        Game1.player.removeItemFromInventory(bait);

        // Display a notification to the player
        var msg = Helper.Translation.Get("baiting.applied");
        Game1.addHUDMessage(HUDMessage.ForCornerTextbox($"{msg}{bait.DisplayName} x {bait.Stack}"));
    }


    private void AutoTackling(FishingRod fishingRod)
    {
        if (!_config.EnableAutoTackling) return;
        if (!fishingRod.CanUseTackle()) return;

        for (var i = 1; i < fishingRod.attachments.Count; ++i)
        {
            if (fishingRod.attachments[i] is not null) continue;

            var tackle = Game1.player.Items.FirstOrDefault(item =>
                item is Object { Category: Object.tackleCategory } o && fishingRod.canThisBeAttached(o));

            if (tackle is null) break;

            fishingRod.attach(tackle as Object);
            Game1.player.removeItemFromInventory(tackle);

            // Display a notification to the player
            var msg = Helper.Translation.Get("tackling.applied");
            Game1.addHUDMessage(HUDMessage.ForCornerTextbox($"{msg}{tackle.DisplayName}"));
        }
    }

    /// <summary>
    ///     Casts the fishing rod at the player's current position.
    ///     This method handles the actual casting of the fishing rod, including
    ///     setting the casting power and applying the auto-hook enchantment
    ///     for automatic fish pulling.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to cast.</param>
    private static void AutoCasting(FishingRod fishingRod)
    {
        if (fishingRod.inUse()) return;
        if (Game1.player?.canMove == false) return;
        if (Game1.activeClickableMenu is not null) return;

        // Cast the fishing rod at the player's current position
        fishingRod.beginUsing(Game1.currentLocation, 0, 0, Game1.player);
        fishingRod.castingPower = 1.0f;

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
        if (!_config.EnableSkipFishShowing) return;
        if (!fishingRod.fishCaught) return;
        if (Game1.player?.canMove == true) return;
        if (Game1.isFestival()) return;

        fishingRod.doneHoldingFish(Game1.player);
    }
}