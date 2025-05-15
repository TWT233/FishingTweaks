using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace FishingTweaks;

/// <summary>
///     Contains functionality for automatically collecting treasure from fishing chests.
///     This part of the mod automatically collects all items from treasure chests when they appear,
///     as long as the player has inventory space.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Handles the treasure chest menu appearance.
    ///     When a treasure chest menu appears, it automatically attempts to collect all items
    ///     that can fit in the player's inventory.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void GrabTreasureOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (!_autoFishing) return;
        if (!_config.EnableGrabTreasure) return;
        if (e.NewMenu is not ItemGrabMenu itemGrabMenu) return;
        if (itemGrabMenu.context is not FishingRod) return;

        // Iterate through all items in the treasure chest
        for (var i = 0; i < itemGrabMenu.ItemsToGrabMenu.actualInventory.Count; i++)
        {
            var item = itemGrabMenu.ItemsToGrabMenu.actualInventory[i];

            // Skip if player's inventory is full
            if (Game1.player?.couldInventoryAcceptThisItem(item) == false) continue;

            // Get the item's position in the menu and simulate a click to collect it
            var bounds = itemGrabMenu.ItemsToGrabMenu.inventory[i].bounds;
            itemGrabMenu.receiveLeftClick(bounds.X, bounds.Y);
        }

        // Close the menu if all items have been collected
        if (itemGrabMenu.areAllItemsTaken()) Game1.exitActiveMenu();
    }
}