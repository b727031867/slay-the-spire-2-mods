# Headless Steam Workshop probe

Uses the Steam client and Steamworks.NET package shipped in Mega Crit's official uploader source (v0.2.0). It inspects item state, or explicitly subscribes and asks Steam to download the item. It never copies files into the Steam cache, publishes content, changes item visibility, unsubscribes, or accepts agreements. Requires Steam running and signed in.

Download the official `steam/Steamworks.NET.15.0.1.nupkg` into an ignored local NuGet feed, restore with `dotnet restore --source <feed>`, and build normally. Copy `steam_api64.dll` from the official Windows uploader alongside the executable. Set `SteamAppId` and `SteamGameId` to `2868840` for this process only. Run `WorkshopProbe.exe inspect <id>` or `subscribe <id>` hidden. A successful download does not prove game loading or dependency compatibility.

Source: https://github.com/megacrit/sts2-mod-uploader/tree/v0.2.0/steam . Record the package hash and uploader version in each acceptance report. Do not publish raw Steam logs or account details.
