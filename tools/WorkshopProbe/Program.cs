using System.Diagnostics;
using System.Text.Json;
using Steamworks;

// Steam client API only. Never writes credentials, uploads content or accepts agreements.
if (args.Length != 2 || !ulong.TryParse(args[1], out var id) || args[0] is not ("inspect" or "subscribe" or "unsubscribe"))
    throw new ArgumentException("Usage: WorkshopProbe inspect|subscribe|unsubscribe <workshop-id>");
if (!SteamAPI.Init()) throw new InvalidOperationException("Steam initialization failed. Sign in to the game-owning account.");
try
{
    var item = new PublishedFileId_t(id);
    var before = SteamUGC.GetItemState(item);
    Console.WriteLine(JsonSerializer.Serialize(new { item = id, stateBefore = before }));
    if (args[0] == "inspect")
    {
        var query = SteamUGC.CreateQueryUGCDetailsRequest([item], 1);
        try
        {
            bool completed = false;
            bool ok = false;
            using var details = CallResult<SteamUGCQueryCompleted_t>.Create((r, io) =>
            {
                ok = !io && r.m_eResult == EResult.k_EResultOK;
                completed = true;
            });
            details.Set(SteamUGC.SendQueryUGCRequest(query));
            var timer = Stopwatch.StartNew();
            while (!completed && timer.Elapsed.TotalSeconds < 45) { SteamAPI.RunCallbacks(); Thread.Sleep(100); }
            if (ok && SteamUGC.GetQueryUGCResult(query, 0, out var detail))
                Console.WriteLine(JsonSerializer.Serialize(new { item = id, visibility = detail.m_eVisibility.ToString(), result = detail.m_eResult.ToString(), consumerApp = detail.m_nConsumerAppID.m_AppId, title = detail.m_rgchTitle }));
            else { Console.WriteLine("DETAILS unavailable"); return 5; }
        }
        finally { SteamUGC.ReleaseQueryUGCRequest(query); }
    }
    if (args[0] == "unsubscribe")
    {
        bool completed = false;
        EResult result = EResult.k_EResultNone;
        using var unsubscribe = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create((r, io) =>
        {
            result = io ? EResult.k_EResultFail : r.m_eResult;
            completed = true;
        });
        unsubscribe.Set(SteamUGC.UnsubscribeItem(item));
        var timer = Stopwatch.StartNew();
        while (!completed && timer.Elapsed.TotalSeconds < 45) { SteamAPI.RunCallbacks(); Thread.Sleep(100); }
        Console.WriteLine($"UNSUBSCRIBE result={result} completed={completed} state={SteamUGC.GetItemState(item)}");
        return completed && result == EResult.k_EResultOK ? 0 : 2;
    }
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
