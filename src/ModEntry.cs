using StardewModdingAPI;

namespace FishingTweaks;

/// <summary>
///     The main entry point for the FishingTweaks mod.
///     This mod overhaul the fishing experience in Stardew Valley by:
///     1. Auto-casting the fishing rod
///     2. Skipping the fishing minigame
///     3. Auto-collecting treasure from fishing chests
///     4. Skipping the fish showing after catching a fish
/// </summary>
internal sealed partial class ModEntry : Mod
{
    /*********
     ** Properties
     *********/
    /// <summary>The mod configuration from the player.</summary>
    private ModConfig _config = null!;
    
    /// <summary>Tracks player fishing statistics.</summary>
    private Counter _counter = new();


    /// <summary>
    ///     The mod entry point, called after the mod is first loaded.
    ///     Registers event handlers for various game events and loads the configuration.
    /// </summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        // Load the mod configuration
        _config = Helper.ReadConfig<ModConfig>();

        // Register the Generic Mod Config Menu integration
        helper.Events.GameLoop.GameLaunched += SetupGMCMOnGameLaunched;

        // Register event handlers for different features
        helper.Events.Display.MenuChanged += SkipMinigameOnMenuChanged; // Handles fishing minigame skipping
        helper.Events.Display.MenuChanged += RecordOnMenuChanged;
        helper.Events.Display.MenuChanged += GrabTreasureOnMenuChanged; // Handles treasure collection
        helper.Events.Input.ButtonPressed += ToggleAutoFishingOnButtonPressed; // Handles auto-casting toggle
        helper.Events.GameLoop.UpdateTicked += AutoFishingOnUpdateTicked; // Handles auto-fishing
    }
}