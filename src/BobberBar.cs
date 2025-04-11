using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FishingTweaks;

/// <summary>
///     Contains functionality for handling the BobberBar minigame.
///     This part of the mod automatically completes the fishing minigame when it appears,
///     making fishing more convenient by eliminating the need for manual minigame interaction.
/// </summary>
internal sealed partial class ModEntry
{
    /// <summary>
    ///     Handles the BobberBar menu appearance.
    ///     When the fishing minigame appears, it automatically sets the progress
    ///     to maximum and ensures any treasure is caught.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void SkipMinigameOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not BobberBar bobberBar) return;
        if (_autoFishing is false) return;
        if (!_config.EnableSkipMinigame) return;
        
        if (!_config.SatisfiedSkipMinigame(bobberBar.whichFish))
        {
            _config.FishCounter.CurrentCount(bobberBar.whichFish, out var catchCount, out var perfectCount);
            Game1.addHUDMessage(HUDMessage.ForCornerTextbox(
                Helper.Translation.Get("bobber-bar.needed",
                    new
                    {
                        fishName = ItemRegistry.Create(bobberBar.whichFish).DisplayName,
                        catchNeeded = Math.Max(_config.MinCatchCountForSkipFishing - catchCount, 0),
                        perfectNeeded = Math.Max(_config.MinPerfectCountForSkipFishing - perfectCount, 0)
                    }
                )
            ));
            return;
        }

        // Set the progress bar to maximum (2.0 is the value that triggers a catch(>=1.0f))
        bobberBar.distanceFromCatching = 2.0f;

        // Catch treasure
        bobberBar.treasureCaught = bobberBar.treasure;

        // Remove from counter
        IncrFishCounter(bobberBar.whichFish, bobberBar.perfect, -1);

        Game1.addHUDMessage(HUDMessage.ForCornerTextbox(Helper.Translation.Get("bobber-bar.familiar")));
    }


    private void RecordFishingOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (_autoFishing is false) return;
        if (e.OldMenu is not BobberBar bobberBar) return;
        if (!bobberBar.handledFishResult) return;
        if (bobberBar.distanceFromCatching < 0.5f) return; // missed

        IncrFishCounter(bobberBar.whichFish, bobberBar.perfect);
    }
}