# 0.3.14 branch review / 2026-10-22

What this change does: This branch adds eight reward cards and four relics. It adds combat-only card elements and controlled combo triggers for Element Tuning and Triphase Infusion.

Looks good. Ship.

Reviewed origin/main...b3764e6b2be5, all changed runtime flows, callers of ComboPower.Reserve, model registration, bilingual text, the existing tests and release scripts. Expected load: one game process, one state per player, with seeded native RNG for synchronized combat. This is not a network load test.

The turn-end negative Dexterity effect can be blocked by Artifact. The user explicitly confirmed this is intended on 2026-10-22; it is not a defect. The installed game's TemporaryDexterityPower also uses PowerCmd.Apply<DexterityPower> with a negative amount at turn end. No gameplay code changed during this review.

The player guide and Workshop description/images still described 0.3.13. These release documents have been updated to 0.3.14, 27 reward cards, five Powers and seven relic models. Earlier co-op playtesting is historical evidence, not validation of these new interactions.

Build from clean b3764e6: zero warnings and errors. Pure combo rules: 52 passed. Log classification: 26 passed. Release packaging: 15 passed. Test entry: 11 passed. Native Full and disk results will be recorded in RELEASE-0314.md after completion.

Verdict: Ship.

Not checked: graphical rendering, real mouse/controller choices, multiplayer/reconnect, natural full-run balance and migration of user saves. Headless checks do not prove these.
