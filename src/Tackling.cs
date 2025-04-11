using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace FishingTweaks;

internal sealed partial class ModEntry
{
    /// <summary>
    ///     Automatically applies tackle to the fishing rod when it's missing.
    ///     This method checks each tackle slot on the fishing rod and if empty,
    ///     attempts to fill it with the first compatible tackle from the player's inventory.
    ///     When tackle is applied, it displays a notification showing the tackle name.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to apply tackle to.</param>
    private void AutoTackling(FishingRod fishingRod)
    {
        if (_autoFishing is false) return;
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
}