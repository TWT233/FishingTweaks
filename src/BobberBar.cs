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

        var caught = _counter.Get(bobberBar.whichFish, FishCounter.CatchType.ManualNormal);
        var perfectCaught = _counter.Get(bobberBar.whichFish, FishCounter.CatchType.ManualPerfect);
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


    private void RecordOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not BobberBar bobberBar) return;
        if (!bobberBar.handledFishResult) return;

        // can be bypassed by disable auto fishing while pulling fish out of water
        // but oh that is the choice of the player itself,
        // and it is not so easy to handle that technically
        // maybe introduce a state to record assisted in skip minigame
        var assisted = _autoFishing && _config.EnableSkipMinigame; 

        FishCounter.CatchType type;
        if (bobberBar.distanceFromCatching < 0.5f)
            type = FishCounter.CatchType.Missed;
        else if (bobberBar.perfect)
            type = assisted ? FishCounter.CatchType.ModAssistedPerfect : FishCounter.CatchType.ManualPerfect;
        else
            type = assisted ? FishCounter.CatchType.ModAssistedNormal : FishCounter.CatchType.ManualNormal;

        _counter.Incr(bobberBar.whichFish, type);
    }
}