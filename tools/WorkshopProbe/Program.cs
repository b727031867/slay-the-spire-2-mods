using System.Diagnostics;
using System.Text.Json;
using Steamworks;

// Steam client API only. Preview replacement is explicit; never uploads mod content or accepts agreements.
var repair = args.Length == 4 && args[0] == "replace-preview";
if ((!repair && (args.Length != 2 || args[0] is not ("inspect" or "subscribe" or "unsubscribe")))
    || !ulong.TryParse(args[1], out var id))
    throw new ArgumentException("Usage: WorkshopProbe inspect|subscribe|unsubscribe <workshop-id> OR replace-preview <workshop-id> <index> <image-path>");
uint previewIndex = 0;
string? previewPath = null;
if (repair)
{
    if (!uint.TryParse(args[2], out previewIndex)) throw new ArgumentException("Invalid preview index.");
    previewPath = Path.GetFullPath(args[3]);
    var file = new FileInfo(previewPath);
    if (!file.Exists || file.Length is <= 0 or >= 1000000 || file.Extension.ToLowerInvariant() is not (".png" or ".jpg" or ".jpeg"))
        throw new ArgumentException("Preview must be an existing PNG/JPEG smaller than 1 MB.");
}
if (!SteamAPI.Init()) throw new InvalidOperationException("Steam initialization failed. Sign in to the game-owning account.");
try
{
    var item = new PublishedFileId_t(id);
    var before = SteamUGC.GetItemState(item);
    Console.WriteLine(JsonSerializer.Serialize(new { item = id, stateBefore = before }));
    if (args[0] == "inspect" || repair)
    {
        var query = SteamUGC.CreateQueryUGCDetailsRequest([item], 1);
        try
        {
            SteamUGC.SetReturnAdditionalPreviews(query, true);
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
            {
                Console.WriteLine(JsonSerializer.Serialize(new { item = id, visibility = detail.m_eVisibility.ToString(), result = detail.m_eResult.ToString(), consumerApp = detail.m_nConsumerAppID.m_AppId, title = detail.m_rgchTitle }));
                SteamUGC.GetQueryUGCPreviewURL(query, 0, out var coverUrl, 4096);
                Console.WriteLine(JsonSerializer.Serialize(new { coverUrl }));
                for (uint index = 0; index < SteamUGC.GetQueryUGCNumAdditionalPreviews(query, 0); index++)
                {
                    if (!SteamUGC.GetQueryUGCAdditionalPreview(query, 0, index, out var url, 4096, out var name, 4096, out var type))
                        throw new InvalidOperationException($"Cannot read preview {index}.");
                    Console.WriteLine(JsonSerializer.Serialize(new { index, url, name, type = type.ToString() }));
                }
                if (repair)
                {
                    if (detail.m_nConsumerAppID.m_AppId != 2868840
                        || !SteamUGC.GetQueryUGCAdditionalPreview(query, 0, previewIndex, out _, 4096, out _, 4096, out var previewType)
                        || previewType != EItemPreviewType.k_EItemPreviewType_Image)
                        throw new InvalidOperationException("Expected an existing image preview for Slay the Spire 2.");
                    var update = SteamUGC.StartItemUpdate(detail.m_nConsumerAppID, item);
                    if (!SteamUGC.UpdateItemPreviewFile(update, previewIndex, previewPath!))
                        throw new InvalidOperationException("Steam rejected the preview replacement.");
                    bool submitted = false;
                    EResult result = EResult.k_EResultNone;
                    bool agreementRequired = false;
                    using var submit = CallResult<SubmitItemUpdateResult_t>.Create((r, io) =>
                    {
                        result = io ? EResult.k_EResultFail : r.m_eResult;
                        agreementRequired = r.m_bUserNeedsToAcceptWorkshopLegalAgreement;
                        submitted = true;
                    });
                    submit.Set(SteamUGC.SubmitItemUpdate(update, "Repair unavailable Workshop gallery image."));
                    timer.Restart();
                    while (!submitted && timer.Elapsed.TotalSeconds < 45) { SteamAPI.RunCallbacks(); Thread.Sleep(100); }
                    Console.WriteLine(JsonSerializer.Serialize(new { previewIndex, submitted, result = result.ToString(), agreementRequired }));
                    return submitted && result == EResult.k_EResultOK && !agreementRequired ? 0 : 6;
                }
            }
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
        while (timer.Elapsed.TotalSeconds < 45)
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
    var finalState = (EItemState)SteamUGC.GetItemState(item);
    SteamUGC.GetItemDownloadInfo(item, out var downloadedBytes, out var totalDownloadBytes);
    Console.WriteLine(JsonSerializer.Serialize(new { item = id, state = (uint)finalState, installed, bytes, folder, timestamp, downloadedBytes, totalDownloadBytes }));
    var current = installed && finalState.HasFlag(EItemState.k_EItemStateSubscribed)
        && finalState.HasFlag(EItemState.k_EItemStateInstalled)
        && !finalState.HasFlag(EItemState.k_EItemStateNeedsUpdate)
        && !finalState.HasFlag(EItemState.k_EItemStateDownloading)
        && !finalState.HasFlag(EItemState.k_EItemStateDownloadPending);
    return args[0] == "subscribe" && !current ? 4 : 0;
}
finally { SteamAPI.Shutdown(); }
