using StardewModdingAPI;
using StardewValley;

namespace FishingTweaks;

public class FishCounter
{
    public SortedDictionary<string, Entry> Records = new();

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

    public void Incr(string whichFish, bool isPerfect, int increment = 1)
    {
        var entry = Records.TryGetValue(whichFish, out var record) ? record : new Entry(whichFish, 0, 0);
        entry.CatchCount += increment;
        if (isPerfect) entry.PerfectCount += increment;
        Records[whichFish] = entry;
    }

    public void CurrentCount(string whichFish, out int catchCount, out int perfectCount)
    {
        var entry = Records.TryGetValue(whichFish, out var record) ? record : new Entry(whichFish, 0, 0);
        catchCount = entry.CatchCount;
        perfectCount = entry.PerfectCount;
    }

    public bool Satisfied(string whichFish, int catchRequired, int perfectRequired)
    {
        if (!Records.TryGetValue(whichFish, out var entry)) return false;

        return entry.CatchCount >= catchRequired && entry.PerfectCount >= perfectRequired;
    }

    public class Entry
    {
        public Entry(string id, int catchCount, int perfectCount)
        {
            Id = id;
            CatchCount = catchCount;
            PerfectCount = perfectCount;
        }

        public string Id { get; set; }
        public int CatchCount { get; set; }
        public int PerfectCount { get; set; }

        public void Incr(bool isPerfect)
        {
            CatchCount++;
            if (isPerfect) PerfectCount++;
        }
    }
}