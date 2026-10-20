using System.Diagnostics;
using System.Text.Json;
using Steamworks;

// Steam client API only. Never writes credentials, uploads content or accepts agreements.
if (args.Length != 2 || !ulong.TryParse(args[1], out var id) || args[0] is not ("inspect" or "subscribe"))
    throw new ArgumentException("Usage: WorkshopProbe inspect|subscribe <workshop-id>");
if (!SteamAPI.Init()) throw new InvalidOperationException("Steam initialization failed. Sign in to the game-owning account.");
try
{
    var item = new PublishedFileId_t(id);
    var before = SteamUGC.GetItemState(item);
    Console.WriteLine(JsonSerializer.Serialize(new { item = id, stateBefore = before }));
    if (args[0] == "subscribe")
    {
        bool completed = false;
        EResult result = EResult.k_EResultNone;
        using var subscribe = CallResult<RemoteStorageSubscribePublishedFileResult_t>.Create((r, io) =>
        {
            result = io ? EResult.k_EResultFail : r.m_eResult;
            completed = true;
        });
        subscribe.Set(SteamUGC.SubscribeItem(item));
        var timer = Stopwatch.StartNew();
        while (!completed && timer.Elapsed.TotalSeconds < 45) { SteamAPI.RunCallbacks(); Thread.Sleep(100); }
        Console.WriteLine($"SUBSCRIBE result={result} completed={completed}");
        if (!completed || result != EResult.k_EResultOK) return 2;
        if (!SteamUGC.DownloadItem(item, true)) { Console.WriteLine("DOWNLOAD request rejected"); return 3; }
        timer.Restart();
        while (timer.Elapsed.TotalSeconds < 90)
        {
            SteamAPI.RunCallbacks();
            var state = (EItemState)SteamUGC.GetItemState(item);
            if (state.HasFlag(EItemState.k_EItemStateInstalled)
                && !state.HasFlag(EItemState.k_EItemStateNeedsUpdate)
                && !state.HasFlag(EItemState.k_EItemStateDownloading)
                && !state.HasFlag(EItemState.k_EItemStateDownloadPending)) break;
            Thread.Sleep(200);
        }
    }
    var installed = SteamUGC.GetItemInstallInfo(item, out var bytes, out var folder, 4096, out var timestamp);
    Console.WriteLine(JsonSerializer.Serialize(new { item = id, state = SteamUGC.GetItemState(item), installed, bytes, folder, timestamp }));
    return args[0] == "subscribe" && !installed ? 4 : 0;
}
finally { SteamAPI.Shutdown(); }
