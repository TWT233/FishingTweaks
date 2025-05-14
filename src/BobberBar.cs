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
        if (!_autoFishing) return;
        if (!_config.EnableSkipMinigame) return;

        var msg = HUDMessage.ForItemGained(ItemRegistry.Create(bobberBar.whichFish), 1, "minigame");

        if (!_config.SatisfiedSkipMinigame(bobberBar.whichFish))
        {
            _config.FishCounter.CurrentCount(bobberBar.whichFish, out var catchCount, out var perfectCount);

            msg.message = Helper.Translation.Get("bobber-bar.needed",
                new
                {
                    fishName = ItemRegistry.Create(bobberBar.whichFish).DisplayName,
                    catchNeeded = Math.Max(_config.MinCatchCountForSkipFishing - catchCount, 0),
                    perfectNeeded = Math.Max(_config.MinPerfectCountForSkipFishing - perfectCount, 0)
                }
            );
            Game1.addHUDMessage(msg);
            return;
        }

        // Set the progress bar to maximum (2.0 is the value that triggers a catch(>=1.0f))
        bobberBar.distanceFromCatching = 2.0f;

        // Catch treasure
        bobberBar.treasureCaught = bobberBar.treasure && _config.SkipMinigameWithTreasure;

        // Designed perfect calculation
        bobberBar.perfect = CalculateIsPerfect(bobberBar);

        msg.message = Helper.Translation.Get("bobber-bar.familiar");
        Game1.addHUDMessage(msg);
    }

    private bool CalculateIsPerfect(BobberBar bobberBar)
    {
        // Perfect on demand, since it is perfect with default so do not need to check like treasure
        if (_config.SkipMinigameWithPerfect) return true;

        // TODO: config for enable chanced perfect, base chance for each type 
        var baseChance = bobberBar.motionType switch
        {
            1 => 5, // Dart
            2 => 90, // Smooth
            3 or 4 => 22, // Floater & Sinker
            _ => 54
        };
        baseChance = bobberBar.bossFish ? (int)Math.Ceiling(baseChance / 5f) : baseChance;
        var difficultyMultiplier = (-3.72f + 123f / (1 + Math.Pow(bobberBar.difficulty / 44.29f, 2.11f))) / 100;
        return new Random().Next(0, 100) <= baseChance * difficultyMultiplier;
    }


    private void RecordPerfectOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (!_autoFishing) return;
        if (e.OldMenu is not BobberBar bobberBar) return;
        if (!bobberBar.handledFishResult) return;
        if (bobberBar.distanceFromCatching < 0.5f) return; // missed

        IncrFishCounter(bobberBar.whichFish, bobberBar.perfect);

        var fishId = bobberBar.whichFish;
        var catchStatsKey = $"CATCH_STATS_{fishId}_MOD"; // New unified key

        int[] catchStats;
        if (Game1.player.fishCaught.TryGetValue(catchStatsKey, out var existingDataArray) && existingDataArray.Length == 4)
        {
            catchStats = existingDataArray;
        }
        else
        {
            // Initialize or re-initialize if data is missing or not in the expected format
            // [manual_normal, mod_normal, manual_perfect, mod_perfect]
            catchStats = new[] { 0, 0, 0, 0 };
        }

        var isModAssisted = _config.EnableAutoHook || _config.EnableSkipMinigame;
        var isPerfect = bobberBar.perfect;

        int indexToIncrement;
        if (isPerfect)
        {
            indexToIncrement = isModAssisted ? 3 : 2; // 3: MOD-assisted perfect, 2: Manual perfect
        }
        else // Not perfect (normal catch)
        {
            indexToIncrement = isModAssisted ? 1 : 0; // 1: MOD-assisted normal, 0: Manual normal
        }

        catchStats[indexToIncrement]++;
        Game1.player.fishCaught[catchStatsKey] = catchStats;
    }
}