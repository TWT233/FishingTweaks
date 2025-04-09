using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Menus;
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
    private void ToggleCastingOnButtonPressed(object? sender, ButtonPressedEventArgs e)
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
        if (_autoFishing is false) return;
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

    /// <summary>
    ///     Automatically applies tackle to the fishing rod when it's missing.
    ///     This method checks each tackle slot on the fishing rod and if empty,
    ///     attempts to fill it with the first compatible tackle from the player's inventory.
    ///     When tackle is applied, it displays a notification showing the tackle name.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to apply tackle to.</param>
    private void AutoTackling(FishingRod fishingRod)
    {
        if (!_config.EnableAutoTackling) return;
        if (!fishingRod.CanUseTackle()) return;

        // Check each tackle slot on the fishing rod (skip slot 0 which is for bait)
        for (var i = FishingRod.TackleIndex; i < fishingRod.attachments.Count; ++i)
        {
            // Skip if this slot already has tackle
            if (fishingRod.attachments[i] is not null) continue;

            // Find the first compatible tackle in the player's inventory
            var tackle = Game1.player.Items.FirstOrDefault(item =>
                item is Object { Category: Object.tackleCategory } o && fishingRod.canThisBeAttached(o));

            // If no compatible tackle found, stop checking other slots
            if (tackle is null) break;

            // Apply the tackle to the fishing rod
            fishingRod.attach(tackle as Object);

            // Remove the tackle from the player's inventory
            Game1.player.removeItemFromInventory(tackle);

            // Display a notification to the player about the applied tackle
            var msg = Helper.Translation.Get("tackling.applied");
            Game1.addHUDMessage(HUDMessage.ForCornerTextbox($"{msg}{tackle.DisplayName}"));
        }
    }

    /// <summary>
    ///     Handles low stamina situation by disabling auto-fishing,
    ///     showing a notification, and opening the inventory menu.
    /// </summary>
    private void HandleLowStamina()
    {
        // Disable auto-fishing
        _autoFishing = false;

        // Show low stamina message
        var msg = Helper.Translation.Get("fishing.low_stamina");
        Game1.addHUDMessage(HUDMessage.ForCornerTextbox(msg));

        // Open inventory menu
        Game1.activeClickableMenu = new GameMenu();
    }

    /// <summary>
    ///     Casts the fishing rod at the player's current position.
    ///     This method handles the actual casting of the fishing rod, including
    ///     setting the casting power and applying the auto-hook enchantment
    ///     for automatic fish pulling. If player's stamina is too low (below the configured minimum),
    ///     it will disable auto-fishing and open the inventory menu.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to cast.</param>
    private void AutoCasting(FishingRod fishingRod)
    {
        if (fishingRod.inUse()) return;
        if (Game1.player is null) return;
        if (Game1.player.canMove == false) return;
        if (Game1.activeClickableMenu is not null) return;

        // Check player's stamina against the configured minimum
        if (Game1.player.Stamina < _config.MinStaminaForAutoFishing)
        {
            HandleLowStamina();
            return;
        }

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