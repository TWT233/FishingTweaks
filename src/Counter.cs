using StardewValley;

namespace FishingTweaks;

/// <summary>
///     Tracks and manages statistics for fish catches in the game, including total catches and perfect catches.
/// </summary>
public class Counter
{
    /// <summary>
    /// Represents different types of fish catches that can be recorded.
    /// </summary>
    public enum CatchType
    {
        /// <summary>Fish caught manually without perfect catch</summary>
        ManualNormal = 0,
        /// <summary>Fish caught with mod assistance without perfect catch</summary>
        ModAssistedNormal = 1,
        /// <summary>Fish caught manually with perfect catch</summary>
        ManualPerfect = 2,
        /// <summary>Fish caught with mod assistance with perfect catch</summary>
        ModAssistedPerfect = 3,
        /// <summary>Fish that got away (missed catch)</summary>
        Missed = 4
    }

    /// <summary>
    /// Increments the catch count for a specific fish and catch type.
    /// </summary>
    /// <param name="whichFish">The ID of the fish being counted.</param>
    /// <param name="type">The type of catch (normal/perfect, manual/mod-assisted).</param>
    public static void Incr(string whichFish, CatchType type)
    {
        var key = Key(whichFish);

        // Initialize a new array to hold all catch type counts
        var counts = new int[Enum.GetNames(typeof(CatchType)).Length];
        
        // If the fish already has existing counts, copy them to preserve other catch types
        if (Game1.player.fishCaught.TryGetValue(key, out var existing))
        {
            for (var i = 0; i < existing.Length && i < counts.Length; i++) 
                counts[i] = existing[i];
        }

        counts[(int)type]++;
        Game1.player.fishCaught[key] = counts;
    }

    /// <summary>
    /// Gets the catch count for a specific fish and catch type.
    /// </summary>
    /// <param name="whichFish">The ID of the fish to query.</param>
    /// <param name="type">The type of catch to count.</param>
    /// <returns>The number of times the fish was caught with the specified catch type.</returns>
    public static int Get(string whichFish, CatchType type)
    {
        if (!Game1.player.fishCaught.TryGetValue(Key(whichFish), out var existing)) return 0;
        if ((int)type < 0 || (int)type >= existing.Length) return 0; // for compatibility, i wanna a one-liner indeed
        return existing[(int)type];
    }

    /// <summary>
    /// Generates a unique key for storing fish catch statistics in the player's data.
    /// The key format is "FT_[fishID]_FT" to avoid conflicts with vanilla game stats.
    /// </summary>
    /// <param name="whichFish">The ID of the fish.</param>
    /// <returns>A formatted key string for storing/retrieving fish catch data.</returns>
    private static string Key(string whichFish)
    {
        return $"FT_{whichFish}_FT";
    }
}