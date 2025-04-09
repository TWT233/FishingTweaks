using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace FishingTweaks;

internal class FishCounterConfig
{
    private SortedDictionary<string, Entry>? _records;

    public string Serialize()
    {
        return JsonConvert.SerializeObject(_records);
    }

    public static FishCounterConfig? Deserialize(string value)
    {
        var r = JsonConvert.DeserializeObject(value);
        return r is SortedDictionary<string, Entry> rr ? new FishCounterConfig { _records = rr } : null;
    }

    public void ArrangeMenu(IManifest modManifest, IModHelper helper, IGenericModConfigMenuApi configMenu)
    {
        configMenu.AddSectionTitle(
            modManifest,
            () => helper.Translation.Get("config.fish-counter.title")
        );

        if (_records is null)
        {
            configMenu.AddParagraph(
                modManifest,
                () => helper.Translation.Get("config.fish-counter.no-records")
            );
            return;
        }

        foreach (var (id, entry) in _records)
        {
            var fish = ItemRegistry.Create(id, allowNull: true);

            var fishName = fish is null
                ? $"{helper.Translation.Get("config.fish-counter.unknown-fish")} @ {id}"
                : fish.DisplayName;
            var catchCountMsg = $"{helper.Translation.Get("config.fish-counter.catch-count")}{entry.CatchCount}";
            var perfectCountMsg = $"{helper.Translation.Get("config.fish-counter.perfect-count")}{entry.PerfectCount}";

            configMenu.AddParagraph(
                modManifest,
                () => $"{fishName} - {catchCountMsg}  {perfectCountMsg}"
            );
        }
    }

    public void Incr(string whichFish, bool isPerfect)
    {
        if (_records is null) return;

        var entry = _records.TryGetValue(whichFish, out var record) ? record : new Entry(whichFish, 0, 0);
        entry.Incr(isPerfect);
        _records[whichFish] = entry;
    }

    private class Entry
    {
        public Entry(string id, int catchCount, int perfectCount)
        {
            Id = id;
            CatchCount = catchCount;
            PerfectCount = perfectCount;
        }

        public string Id { get; private set; }
        public int CatchCount { get; private set; }
        public int PerfectCount { get; private set; }

        public void Incr(bool isPerfect)
        {
            CatchCount++;
            if (isPerfect) PerfectCount++;
        }

        public void Reset()
        {
            CatchCount = 0;
            PerfectCount = 0;
        }
    }
}