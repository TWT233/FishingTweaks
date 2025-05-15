using StardewValley;

namespace FishingTweaks;

/// <summary>
///     Tracks and manages statistics for fish catches in the game, including total catches and perfect catches.
/// </summary>
public class FishCounter
{
    public enum CatchType
    {
        ManualNormal = 0,
        ModAssistedNormal = 1,
        ManualPerfect = 2,
        ModAssistedPerfect = 3,
        Missed = 4
    }

    public void Incr(string whichFish, CatchType type)
    {
        var key = Key(whichFish);

        var counts = new int[Enum.GetNames(typeof(CatchType)).Length];
        if (Game1.player.fishCaught.TryGetValue(key, out var existing))
        {
            for (var i = 0; i < existing.Length && i < counts.Length; i++) counts[i] = existing[i];
        }

        counts[(int)type]++;
        Game1.player.fishCaught[key] = counts;
    }

    public int Get(string whichFish, CatchType type)
    {
        if (!Game1.player.fishCaught.TryGetValue(Key(whichFish), out var existing)) return 0;
        if ((int)type < 0 || (int)type >= existing.Length) return 0; // for compatibility, i wanna a one-liner indeed
        return existing[(int)type];
    }

    private string Key(string whichFish)
    {
        return $"FT_{whichFish}_FT";
    }
}