using StardewModdingAPI;

namespace FishingTweaks;

/// <summary>
///     The main entry point for the FishingTweaks mod.
///     This mod enhances the fishing experience in Stardew Valley by:
///     1. Auto-casting the fishing rod
///     2. Skipping the fishing minigame
///     3. Auto-collecting treasure from fishing chests
/// </summary>
internal sealed partial class ModEntry : Mod
{
    /// <summary>
    ///     The mod entry point, called after the mod is first loaded.
    ///     Registers event handlers for various game events.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        // Register event handlers for different features
        helper.Events.Display.MenuChanged += SkipFishingOnMenuChanged; // Handles fishing minigame skipping
        helper.Events.Display.MenuChanged += GrabTreasureOnMenuChanged; // Handles treasure collection
        helper.Events.Input.ButtonPressed += ToggleCastingOnButtonPressed; // Handles auto-casting toggle
        helper.Events.GameLoop.OneSecondUpdateTicked += CastingOnOneSecondUpdateTicked; // Handles auto-casting
    }
}