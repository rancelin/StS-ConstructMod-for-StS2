# The Construct — Slay the Spire 2 port

C# port of [Moocowsgomoo's The Construct](https://github.com/Moocowsgomoo/StS-ConstructMod) for Slay the Spire 2.

## Status

Work in progress. The core character, Cycle mechanic, and ~95 cards are ported and building clean against StS2 `v0.111.0`.

### Ported
- **Character**: `TheConstruct` (BaseLib `CustomCharacterModel`) with card/relic/potion pools, starting deck, and starting relic. Placeholder Ironclad visuals until a `.pck` ships.
- **Cycle keyword** (BaseLib `CustomEnum` CardKeyword) and `AbstractCycleCard` base: cycles discard-and-draw once per turn when drawn, if the card's condition is met. Per-turn cycle counter (`CycleCount`) backed by a `SpireField<PlayerCombatState, int>` (per-player-per-combat, multiplayer-safe).
- **Cycle hook**: `IAfterCardCycled` interface dispatched via `CycleHook.AfterCardCycled` (BaseLib `HookUtils.Dispatch`), mirroring Downfall's `DownfallHook`/BaseLib's `BaseLibHooks`. Powers/relics implement the interface; no manual subscribe/unsubscribe.
- **Cores**: `AbstractCoreCard` base — 0-cost Cycle cards with an on-cycle bonus, cloning to discard when upgraded.
- **Cards**: ~95 cards across Common/Uncommon/Rare. Starting deck includes the default starter kit (5 Strike, 5 Defend, AttackMode, DefenseMode, ModeShift) plus one copy of each reviewed card for verification.
- **Powers**: 30 custom powers including `NoCyclePower`, `MultistagePower` (with `{Card}` placeholder via `StringVar`), `ElectricArmorThornsPower` (1-turn Thorns via BaseLib `CustomTemporaryPowerModelWrapper`), `MetallicizePower` and `PlatedArmorPower` (custom, matching Downfall's pattern), temporary Strength/Dexterity down via vanilla `TemporaryStrengthPower`/`TemporaryDexterityPower`.
- **Relic**: `Cogwheel` (starter, grants 1 Artifact at the start of each combat).
- **Localization**: in-code via BaseLib `CardLoc`/`PowerLoc`/`RelicLoc`/`CharacterLoc` + `SimpleLoc`. `CycleKeywordLocPatch` injects keyword + selection-screen-prompt loc until a `.pck` ships (`has_pck: false`).

### Roadmap (not yet ported)
- **Heat/Overheat system**: the original mod's signature mechanic — per-card `overheat` threshold that turns cards into Burns after N cycles in a turn, plus the Heat Meter UI. `Rollout` is the only currently-ported card that referenced it (overheat field not yet ported). The `IAfterCardCycled` hook is now in place as the foundation.
- **Mega-upgrade**: second upgrade tier. Currently folded into the normal upgrade for some cards (e.g. OmegaCannon, Overclock); not implemented as a separate tier.
- **Remaining relics**: ~13 relics from the original mod not yet ported.
- **Remaining powers**: Agitation, FlashFreeze, LongRangeLance, and other Heat-gated powers not yet ported.
- **Remaining cards**: ~unported cards beyond the current ~95.
- **Godot assets**: `.pck` with card art, power icons, character visuals, localization tables.
- **Flamethrower**: disabled (`showInCardLibrary: false, autoAdd: false`); to be reimagined as a multiplayer card.

## Building

Requires the .NET 9 SDK and a Slay the Spire 2 install (v0.111.0), plus a built BaseLib.

```
cd ConstructMod
dotnet build -p:Sts2Path="/path/to/Slay the Spire 2" -p:BaseLibPath="/path/to/BaseLib.dll"
```

Outputs to `publish/` and copies to `<game>/mods/ConstructMod/` when `ModsPath` resolves.

## Porting notes

- STS1 mega-upgrades, Heat Meter UI, challenge modes, and the settings panel are not yet ported.
- Card/relic/power localization is in-code via BaseLib `CardLoc`/`PowerLoc`/`RelicLoc` with `SimpleLoc` (`#` prefix for markup processing: `*word*` → gold, `$word$` → blue, `!D!`/`!B!`/etc. short var names).
- Power tooltips follow the vanilla split: `Description` (static, no `{Amount}`) for out-of-combat/library, `SmartDescription` (with `{Amount}`) for in-combat.
- The Cycle keyword tooltip text needs a localization table; a `.pck` will be added later (`has_pck` is currently false). `CycleKeywordLocPatch` injects the loc in-code as a workaround.
- Custom powers (`MetallicizePower`, `PlatedArmorPower`) follow Downfall's verified pattern; `ElectricArmorThornsPower` uses BaseLib's `CustomTemporaryPowerModelWrapper` for 1-turn Thorns.
