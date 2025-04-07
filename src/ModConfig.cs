using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace FishingTweaks;

public sealed class ModConfig
{
    public SButton ToggleAutoCasting { get; set; } = SButton.V;
}

internal sealed partial class ModEntry
{
    private void SetupGMCMOnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        // get Generic Mod Config Menu's API (if it's installed)
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            ModManifest,
            () => _config = new ModConfig(),
            () => Helper.WriteConfig(_config)
        );

        // add some config options
        configMenu.AddKeybind(
            ModManifest,
            name: () => Helper.Translation.Get("config.toggle-auto-casting"),
            tooltip: () => Helper.Translation.Get("config.toggle-auto-casting.tooltip"),
            getValue: () => _config.ToggleAutoCasting,
            setValue: value => _config.ToggleAutoCasting = value
        );
    }
}