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
