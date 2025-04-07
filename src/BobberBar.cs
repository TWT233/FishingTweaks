using StardewModdingAPI.Events;
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
    private static void SkipFishingOnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Check if the new menu is the fishing minigame
        if (e.NewMenu is not BobberBar bobberBar) return;

        // Set the progress bar to maximum (2.0 is the value that triggers a catch(>=1.0f))
        bobberBar.distanceFromCatching = 2.0f;

        // Catch treasure
        bobberBar.treasureCaught = bobberBar.treasure;
    }
}