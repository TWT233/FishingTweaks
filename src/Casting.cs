using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace FishingTweaks;

internal sealed partial class ModEntry
{
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
        if (_autoFishing is false) return;
        if (!_config.EnableAutoCasting) return;
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
}