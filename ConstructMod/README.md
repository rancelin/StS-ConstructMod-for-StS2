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
- **Relics**: 11 ported — `Cogwheel` (starter, grants 1 Artifact each combat; its Orobas "Touch of Orobas" upgrade is the new `ClockworkCogwheel`, 2 Artifact, via BaseLib's `GetUpgradeReplacement`), `BoolHorns` (Rare, damage doubles between non-combat rooms), `ClawGrip` (Boss/Ancient, retain a random card + it costs 1 less next turn), `FoamFinger` (Uncommon, +1 draw for 3 turns via `ModifyHandDraw`), `IceCubes` (Shop, no Overheat for 3 turns), `MasterCore` (Common, 3 random Cores per combat), `RocketBooster` (Rare, upgrade a random card on Elite victory), `WeddingRing` (Boss/Ancient, marry 2 cards — once per turn, playing one plays the other; selection is in-memory until the save-support pass), `LongRangeLanceRelic` + `ExtraLongRangeLanceRelic` (Event, granted by the Lance card's powers), `ClockworkCogwheel` (Starter, Orobas upgrade only).
- **Potions**: `ShiftPotion` (Uncommon — swap Str/Dex, draw 1).
- **Localization**: in-code via BaseLib `CardLoc`/`PowerLoc`/`RelicLoc`/`CharacterLoc` + `SimpleLoc`. `CycleKeywordLocPatch` injects keyword + selection-screen-prompt loc until a `.pck` ships (`has_pck: false`).

### Roadmap (not yet ported)
- **Heat Meter UI**: the visual heat bar (cycles this turn + per-card overheat thresholds). Godot scene + BaseLib ExtraCombatUi integration. (The Heat/Overheat mechanic itself is fully ported.)
- **Mega-upgrade**: second upgrade tier. Currently folded into the normal upgrade for some cards (e.g. OmegaCannon, Overclock, all Heat cards, LongRangeLance, SunScreen); not implemented as a separate tier. Unblocks Stasis, ClockworkEgg, MegaPotion, and 3 mega relics (ClockworkPhoenix, MegaBattery, PurpleEmber).
- **Remaining relics**: ClockworkPhoenix (the StS1 `phoenixStart` settings-panel starter alternative — gated on mega-upgrade; a possible future config toggle), MegaBattery, PurpleEmber (mega-blocked), FreezeFrame (Cycle toggle with right-click activation).
- **Remaining potion**: MegaPotion (mega-blocked).
- **Remaining cards**: Stasis, ClockworkEgg (mega-blocked).
- **Save/load support**: WeddingRing's chosen cards are in-memory only (lost on load); ClawGrip's retained-card reference is combat-only by design. Needs BaseLib SavedSpireField / ExtendedSaveTypes work.
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
