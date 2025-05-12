using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace FishingTweaks;

internal sealed partial class ModEntry
{
    /// <summary>
    ///     Automatically applies bait to the fishing rod when it runs out.
    ///     This method will use the first available bait item from the player's inventory
    ///     that is compatible with the fishing rod. When bait is applied, it displays
    ///     a notification showing the bait name and quantity.
    /// </summary>
    /// <param name="fishingRod">The fishing rod to apply bait to.</param>
    private void AutoBaiting(FishingRod fishingRod)
    {
        if (!_autoFishing) return;
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
        var msg = HUDMessage.ForItemGained(bait, bait.Stack, "auto-baiting");
        msg.message = Helper.Translation.Get("baiting.applied");
        Game1.addHUDMessage(msg);
    }
}
