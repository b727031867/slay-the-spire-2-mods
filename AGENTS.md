# Repository working instructions

## Windows terminal

Use PowerShell 7 for Windows shell commands. On PC-A, explicitly select `C:\Users\GXF\AppData\Local\Programs\PowerShell\7\pwsh.exe` when the tool supports a shell parameter. On another PC, locate its installed PowerShell 7 executable; do not copy PC-A's user path or use Windows PowerShell 5.1 as the development shell.

## Game tests must run without a GUI

The user requested on 2026-10-20 that game testing must not interfere with their other work.

- Run game startup checks, regression and acceptance tests with `scripts/Start-ModGame.ps1 -Headless`, adding the appropriate test option. Headless uses Godot's `--headless` argument and a hidden process window; merely minimizing a graphical window is insufficient.
- `-TravelerTest` and `-TravelerAcceptance` automatically enable headless mode as a safeguard. Use the matching `-Headless` option of the log checker where supported.
- Do not activate game windows, send keyboard/mouse input, or launch graphical game tests unless the user explicitly requests graphical testing later. Tests that require manual UI interaction remain NOT_RUN; use available headless acceptance cases instead.
- Do not start legacy `-TravelerRewards` flows that wait for manual card selection as an unattended test.
- Track the process ID returned by each test launch. After collecting results, clean up only the test process you started; do not stop an existing user game or other user applications. Do not restart the game graphically after testing.
- Headless logic results do not prove graphical rendering, manual UI, or natural playthrough correctness. Record these validation scopes separately.

## TASK-003 disk acceptance

- Automated test launches create a unique, marked metadata directory under `.local/task003-saves`. Native save data lives in the matching `user://sts2mod-acceptance/<id>` namespace using the real `SaveManager` and `GodotFileIo`, with no Steam cloud backend. Never copy a user's saves into it.
- `-TravelerEndToEnd` prepares a fresh fixture and exits through the native menu/quit path. Redeploy the same package, then use `-ResumeTask003 -SandboxDirectory <the returned directory>` to resume that disk save in a new process. Both flags force headless mode.
- Test display/audio settings use explicit fixture defaults; run/progress/prefs data uses native disk persistence. This is not a test of user graphics settings persistence.
- Continuous-battle acceptance uses a controlled deck, high player HP and a four-turn budget per combat. Record forced cleanup separately from natural victories; do not present it as a natural full-run or balance validation.

## Workshop visibility and optional playtesting

User instruction dated 2026-10-21: when multiplayer playtests are requested, they use friends and the Friends Only Workshop item. User instruction dated 2026-10-23: manual playtesting and multiplayer verification are optional after development. They are not required to commit, merge, package or release, and must not be run unless the user explicitly requests them.

- Keep the existing Workshop item `3823282901` set to **Friends Only** throughout development, uploads/updates and testing. The official uploader configuration value is `friends_only`.
- Change visibility only if the user explicitly requests a different setting later. Do not restore the older private/hidden policy from historical acceptance reports or cached upload directories.
- Before every upload, verify the actual upload workspace `workshop.json` uses `friends_only`; regenerate or deliberately refresh stale workspace metadata. After uploading, verify remote visibility with `WorkshopProbe inspect 3823282901` (expected `k_ERemoteStoragePublishedFileVisibilityFriendsOnly`).
- If optional playtesting or multiplayer verification is performed, keep its results separate from headless logic and rendering checks. Planning friend sessions does not establish multiplayer compatibility.
