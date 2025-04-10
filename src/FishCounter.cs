using StardewModdingAPI;
using StardewValley;

namespace FishingTweaks;

/// <summary>
///     Tracks and manages statistics for fish catches in the game, including total catches and perfect catches.
/// </summary>
public class FishCounter
{
    /// <summary>
    ///     Dictionary storing catch records for each fish type, indexed by fish ID.
    /// </summary>
    public SortedDictionary<string, Entry> Records = new();

    /// <summary>
    ///     Configures and displays fish catch statistics in the Generic Mod Config Menu.
    /// </summary>
    /// <param name="modManifest">The mod's manifest information.</param>
    /// <param name="helper">SMAPI's mod helper instance.</param>
    /// <param name="configMenu">The Generic Mod Config Menu API instance.</param>
    public void ArrangeMenu(IManifest modManifest, IModHelper helper, IGenericModConfigMenuApi configMenu)
    {
        configMenu.AddSectionTitle(
            modManifest,
            () => helper.Translation.Get("config.fish-counter.title")
        );

        foreach (var (id, entry) in Records)
        {
            var fish = ItemRegistry.Create(id, allowNull: true);

            configMenu.AddParagraph(
                modManifest,
                () =>
                {
                    var fishName = fish is null
                        ? $"{helper.Translation.Get("config.fish-counter.unknown-fish")} @ {id}"
                        : fish.DisplayName;
                    var catchCountMsg =
                        $"{helper.Translation.Get("config.fish-counter.catch-count")}{entry.CatchCount}";
                    var perfectCountMsg =
                        $"{helper.Translation.Get("config.fish-counter.perfect-count")}{entry.PerfectCount}";
                    return $"{fishName} - {catchCountMsg} | {perfectCountMsg}";
                });
        }
    }

    /// <summary>
    ///     Increments the catch count and perfect catch count for a specific fish.
    /// </summary>
    /// <param name="whichFish">The ID of the fish being caught.</param>
    /// <param name="isPerfect">Whether the catch was perfect.</param>
    /// <param name="increment">The amount to increment the counter by (default: 1).</param>
    public void Incr(string whichFish, bool isPerfect, int increment = 1)
    {
        var entry = Records.TryGetValue(whichFish, out var record) ? record : new Entry(whichFish, 0, 0);
        entry.CatchCount += increment;
        if (isPerfect) entry.PerfectCount += increment;
        Records[whichFish] = entry;
    }

    /// <summary>
    ///     Retrieves the current catch counts for a specific fish.
    /// </summary>
    /// <param name="whichFish">The ID of the fish to check.</param>
    /// <param name="catchCount">The total number of catches for this fish.</param>
    /// <param name="perfectCount">The number of perfect catches for this fish.</param>
    public void CurrentCount(string whichFish, out int catchCount, out int perfectCount)
    {
        var entry = Records.TryGetValue(whichFish, out var record) ? record : new Entry(whichFish, 0, 0);
        catchCount = entry.CatchCount;
        perfectCount = entry.PerfectCount;
    }

    /// <summary>
    ///     Checks if the catch requirements for a specific fish have been met.
    /// </summary>
    /// <param name="whichFish">The ID of the fish to check.</param>
    /// <param name="catchRequired">The required number of total catches.</param>
    /// <param name="perfectRequired">The required number of perfect catches.</param>
    /// <returns>True if both catch requirements are met, false otherwise.</returns>
    public bool Satisfied(string whichFish, int catchRequired, int perfectRequired)
    {
        if (!Records.TryGetValue(whichFish, out var entry)) return false;

        return entry.CatchCount >= catchRequired && entry.PerfectCount >= perfectRequired;
    }

    /// <summary>
    ///     Represents a single fish's catch statistics.
    /// </summary>
    public class Entry
    {
        /// <summary>
        ///     Initializes a new instance of the Entry class with the specified catch statistics.
        /// </summary>
        /// <param name="id">The ID of the fish.</param>
        /// <param name="catchCount">The initial total catch count.</param>
        /// <param name="perfectCount">The initial perfect catch count.</param>
        public Entry(string id, int catchCount, int perfectCount)
        {
            Id = id;
            CatchCount = catchCount;
            PerfectCount = perfectCount;
        }

        /// <summary>
        /// Gets or sets the ID of the fish.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the total number of times this fish has been caught.
        /// </summary>
        public int CatchCount { get; set; }

        /// <summary>
        ///     Gets or sets the number of times this fish has been caught perfectly.
        /// </summary>
        public int PerfectCount { get; set; }
    }
}