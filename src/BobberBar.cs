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
    private bool _lastCaughtAssisted = false;

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
        if (!_autoFishing) return;
        if (!_config.EnableSkipMinigame) return;

        // Reset buffered state
        _lastCaughtAssisted = false;

        var msg = HUDMessage.ForItemGained(ItemRegistry.Create(bobberBar.whichFish), 1, "minigame");

        var caught = Counter.Get(bobberBar.whichFish, Counter.CatchType.ManualNormal);
        var perfectCaught = Counter.Get(bobberBar.whichFish, Counter.CatchType.ManualPerfect);
        if (caught < _config.MinCatchCountForSkipFishing || perfectCaught < _config.MinPerfectCountForSkipFishing)
        {
            msg.message = Helper.Translation.Get("bobber-bar.needed",
                new
                {
                    fishName = ItemRegistry.Create(bobberBar.whichFish).DisplayName,
                    catchNeeded = Math.Max(_config.MinCatchCountForSkipFishing - caught, 0),
                    perfectNeeded = Math.Max(_config.MinPerfectCountForSkipFishing - perfectCaught, 0)
                }
            );
            Game1.addHUDMessage(msg);
            return;
        }

        // Set the progress bar to maximum (2.0 is the value that triggers a catch(>=1.0f))
        bobberBar.distanceFromCatching = 2.0f;
        _lastCaughtAssisted = true;

        // Catch treasure
        bobberBar.treasureCaught = bobberBar.treasure && _config.SkipMinigameWithTreasure;

        // Designed perfect calculation
        bobberBar.perfect = CalculateIsPerfect(bobberBar);

        msg.message = Helper.Translation.Get("bobber-bar.familiar");
        Game1.addHUDMessage(msg);
    }

    /// <summary>
    /// Calculates whether the fishing attempt should be considered a perfect catch.
    /// This takes into account configuration settings, fish movement type, and difficulty.
    /// </summary>
    /// <param name="bobberBar">The BobberBar instance containing fishing minigame data.</param>
    /// <returns>True if the catch should be considered perfect, false otherwise.</returns>
    private bool CalculateIsPerfect(BobberBar bobberBar)
    {
        // If perfect catches are forced in config, always return true
        if (_config.SkipMinigameWithPerfect) return true;

        // Calculate base perfect chance based on fish movement type:
        var baseChance = bobberBar.motionType switch
        {
            1 => 5, // Dart
            2 => 90, // Smooth
            3 or 4 => 22, // Floater & Sinker
            _ => 54
        };

        // Reduce chance for boss fish (divide by 5)
        baseChance = bobberBar.bossFish ? (int)Math.Ceiling(baseChance / 5f) : baseChance;

        // Calculate difficulty multiplier using sigmoid-like function:
        var difficultyMultiplier = (-3.72f + 123f / (1 + Math.Pow(bobberBar.difficulty / 44.29f, 2.11f))) / 100;

        return new Random().Next(0, 100) <= baseChance * difficultyMultiplier;
    }


    /// <summary>
    /// Records fishing results when the BobberBar menu closes.
    /// This method tracks successful catches, perfect catches, and misses,
    /// distinguishing between manual and mod-assisted catches based on configuration.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The menu changed event data.</param>
    private void RecordOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not BobberBar bobberBar) return;
        if (!bobberBar.handledFishResult) return;

        // Determine catch type based on fishing results and if mod-assisted
        Counter.CatchType type;
        if (bobberBar.distanceFromCatching < 0.5f)
            type = Counter.CatchType.Missed;
        else if (bobberBar.perfect)
            type = _lastCaughtAssisted ? Counter.CatchType.ModAssistedPerfect : Counter.CatchType.ManualPerfect;
        else
            type = _lastCaughtAssisted ? Counter.CatchType.ModAssistedNormal : Counter.CatchType.ManualNormal;

        // Increment the fish counter with the appropriate catch type
        Counter.Incr(bobberBar.whichFish, type);
        
        // Reset again since the last caught is processed
        _lastCaughtAssisted = false;
    }
}