# The Construct — Slay the Spire 2 port

C# port of [Moocowsgomoo's The Construct](https://github.com/Moocowsgomoo/StS-ConstructMod) for Slay the Spire 2.

## Status

Work in progress. The core character, Cycle mechanic, and all 103 playable cards are ported and building clean against StS2 `v0.111.0` (mod declared `min_game_version: 0.111.0`; the game install and the vendored `StS2data/` are both `v0.111.0`).

### Ported
- **Character**: `TheConstruct` (BaseLib `CustomCharacterModel`) with card/relic/potion pools, starting deck, and starting relic. Placeholder Ironclad visuals until a `.pck` ships.
- **Cycle keyword** (BaseLib `CustomEnum` CardKeyword) and `AbstractCycleCard` base: cycles discard-and-draw once per turn when drawn, if the card's condition is met. Per-turn cycle counter (`CycleCount`) backed by a `SpireField<PlayerCombatState, int>` (per-player-per-combat, multiplayer-safe).
- **Cycle hook**: `IAfterCardCycled` interface dispatched via `CycleHook.AfterCardCycled` (BaseLib `HookUtils.Dispatch`), mirroring Downfall's `DownfallHook`/BaseLib's `BaseLibHooks`. Powers/relics implement the interface; no manual subscribe/unsubscribe.
- **Cores**: `AbstractCoreCard` base — 0-cost Cycle cards with an on-cycle bonus, cloning to discard when upgraded.
- **Cards**: all 103 playable cards (the full StS1 set minus the disabled Flamethrower) across Common/Uncommon/Rare. Starting deck is the StS1 starter kit (5 Strike, 5 Defend, AttackMode, DefenseMode — 12 cards). ModeShift is a Common reward-pool card (its StS1 rarity), never in the deck.
- **Powers**: 40 custom powers including `NoCyclePower`, `MultistagePower` (with `{Card}` placeholder via `StringVar`), `ElectricArmorThornsPower` (1-turn Thorns via BaseLib `CustomTemporaryPowerModelWrapper`), `MetallicizePower` and `PlatedArmorPower` (custom, matching Downfall's pattern), temporary Strength/Dexterity down via vanilla `TemporaryStrengthPower`/`TemporaryDexterityPower`.
- **Relics**: 14 ported — `Cogwheel` (starter, grants 1 Artifact each combat; its Orobas "Touch of Orobas" upgrade is the new `ClockworkCogwheel` — keeps 1 Artifact + the first card Cycled each turn draws 1 additional card, via BaseLib's `GetUpgradeReplacement`), `BoolHorns` (Rare, damage doubles between non-combat rooms), `ClawGrip` (Boss/Ancient, retain a random card + it costs 1 less next turn), `FoamFinger` (Uncommon, +1 draw for 3 turns via `ModifyHandDraw`), `IceCubes` (Shop, no Overheat for 3 turns), `MasterCore` (Common, 3 random Cores per combat), `RocketBooster` (Rare, upgrade a random card on Elite victory), `WeddingRing` (Boss/Ancient, marry 2 cards — once per turn, playing one plays the other; the pair is marked with a Marriage enchantment, which persists through save/load, shows which copies are linked, and blocks the Clone enchantment by exclusivity), `LongRangeLanceRelic` + `ExtraLongRangeLanceRelic` (Event, granted by the Lance card's powers), `ClockworkCogwheel` (Starter, Orobas upgrade only).
- **Potions**: `ShiftPotion` (Uncommon — swap Str/Dex, draw 1).
- **Mega-upgrade system (Phase 1)**: built on StS2's native multi-level upgrade system (`MaxUpgradeLevel`/`CurrentUpgradeLevel`, same foundation as Downfall's RefractedBeam and TheTailor's stackable cards). `AbstractConstructCard` implements the tier gating (smith-chosen Mega requires the `ClockworkPhoenix` relic, out of combat only; forced sources bypass via `ForceUpgradeToMax` — the StS2 `forcedUpgrade` equivalent, after Downfall's `ForceUpgradeHelper` pattern), save/load replay safety (unowned cards report their full ceiling), and the `{IfMega:mega:…|}` description macro (`IfMegaVar` + `ShowIfMegaFormatter` via BaseLib `IAutoRegisterFormatSpecifier`; named "mega" — SmartFormat forbids duplicate formatter names, so it can't share vanilla's "show"). Ported sources: **ClockworkPhoenix** (flag relic; granted by ClockworkEgg's final tier, faithful to StS1), **MegaBattery** (Uncommon: mega a random deck card on pickup), **MegaPotion** (Rare potion: mega a random hand card). Unfolded to the 2-tier ladder: OmegaCannon (mega = the Strength-discount behavior), GatlingGun (mega = −1 dmg/+1 shot).
- **Localization**: in-code via BaseLib `CardLoc`/`PowerLoc`/`RelicLoc`/`CharacterLoc` + `SimpleLoc`. `CycleKeywordLocPatch` injects keyword + selection-screen-prompt loc until a `.pck` ships (`has_pck: false`).

### Roadmap (not yet ported)
- **Heat Meter UI**: the visual heat bar (cycles this turn + per-card overheat thresholds). Godot scene + BaseLib ExtraCombatUi integration. (The Heat/Overheat mechanic itself is fully ported.)
- **Mega-upgrade Phase 2 (done)**: PurpleEmber (rest site → next combat's starting hand is mega'd), Enhance's mega tier (EnhanceMegaPower), Stasis + ConstructStasisPower (reclaim force-megas the banked card), ClockworkEgg (4-rung ladder + Scrambled/Chicken mega; grants ClockworkPhoenix at its hatch stage), Zapper mega (loss-trigger) + ShiftingStance mega (swap-and-adjust power).
- **Mega-upgrade Phase 3 (done)**: every card now unfolds to the proper 2-tier ladder, with the per-card deltas re-verified against the StS1 source (22 numeric-only cards plus a ~30-card batch that also touched text, keywords and play behaviour — e.g. Accumulate's copy count, Antimatter's energy refund, Battery Acid's next-turn energy, Cluster Mines' combat-long discount, Crippling Shot's −2 Strength rider, Dark Flames' seed Burns, Flamethrower's Status tally, Forcefield's rebound and Hard Reboot's Exhaust drop). All 93 concrete cards (plus the seven Cores, which inherit from `AbstractCoreCard`) override `IntrinsicMaxUpgradeLevel => 2`. Deliberate divergences: **Agitation** cannot reproduce StS1's `startInPlay` (StS2's `BeforeCombatStart` fires on the deck card while combat holds a clone), so its mega tier is Innate + 0-cost + a self-replacing draw (the only addition not present in the original — see the caveat in TESTING.html); **Scrap Cannon** keeps the port's unrestricted exhaust choice at both tiers, though StS1's base tier may have limited it to non-attacks (still to verify against the original source).
- **Mega-upgrade visuals**: purple title rendering (the original's RenderMegaUpgradePatch) — deferred to the visual pass alongside the Marriage enchantment icon art. Note: downgrade (Reflections event etc.) resets to level 0, losing both tiers (vanilla behavior; Downfall's StackingUpgradeDowngradePatch is the precedent if we ever want per-level downgrade).
- **Remaining relics**: none — `ClockworkPhoenix` (granted by ClockworkEgg's final tier) and `MegaBattery` are ported. (The StS1 `phoenixStart` settings-panel starter alternative is still a possible future config toggle.) The only unported relics are the Challenge-mode relics `Challenge1`–`Challenge5`.
- **Remaining potion**: none — `MegaPotion` and `ShiftPotion` are both ported.
- **Remaining cards**: none — Stasis and ClockworkEgg are ported. (Challenge-mode Heated starters stay out until challenge modes land.)
- **Save/load support**: ClawGrip's retained-card reference is combat-only by design. (WeddingRing persistence is done via the Marriage enchantment.)
- **Godot assets**: `.pck` with card art, power icons, character visuals, localization tables.
- **Flamethrower**: disabled (`showInCardLibrary: false, autoAdd: false`); to be reimagined as a multiplayer card.
- **Future rework notes (parked)**: new card(s)/event(s)/relic designs around the ClockworkEgg mega path (two-copy smith route vs forced-mega shortcuts) for solo and/or multiplayer — revisit together with the Flamethrower rework, not during Phase 2/3. (Regular Egg testing/porting continues normally.)

## Building

Requires the .NET 9 SDK and a Slay the Spire 2 install (v0.111.0), plus a built BaseLib.

```
cd ConstructMod
dotnet build -p:Sts2Path="/path/to/Slay the Spire 2" -p:BaseLibPath="/path/to/BaseLib.dll"
```

Outputs to `publish/` and copies to `<game>/mods/ConstructMod/` when `ModsPath` resolves.

## Porting notes

- The mega-upgrade rollout (Phase 3) is done; the Heat Meter UI, challenge modes, and the settings panel are not yet ported.
- Card/relic/power localization is in-code via BaseLib `CardLoc`/`PowerLoc`/`RelicLoc` with `SimpleLoc` (`#` prefix for markup processing: `*word*` → gold, `$word$` → blue, `!D!`/`!B!`/etc. short var names). Every card description must start with `#`, otherwise the markup prints literally.
- Numeric dynamic vars go in card text as `!Var!` (→ `{Var:diff()}`), matching vanilla: that is what makes the upgrade preview green the value the next upgrade changes. Plain `{Var}` renders the number with no highlight. Two deliberate exceptions: `{Overheat}` (its var renders the live threshold through `ToString()`, which `diff()` would bypass) and `{DexLoss}` (a downside, where green would mislead). Energy stays `{Energy:energyIcons()}`.
- Because a card's `{Var}`-based numbers carry the highlight, per-tier *text* uses two macros: `{IfMega:mega:megaText|normalText}` (mega tier only) and `{IfPlusOne:plusone:plusOneText|hiddenText}`. The latter is the multi-level-aware stand-in for vanilla's `{IfUpgraded:show:...}`, which cannot tell "+1 applied" from "+1 is being previewed" and would re-green the +1 line at the +1→+2 preview. Both are registered via BaseLib's `IAutoRegisterFormatSpecifier` and injected by `AbstractConstructCard.AddExtraArgsToDescription`.
- Power tooltips follow the vanilla split: `Description` (static, no `{Amount}`) for out-of-combat/library, `SmartDescription` (with `{Amount}`) for in-combat.
- The Cycle keyword tooltip text needs a localization table; a `.pck` will be added later (`has_pck` is currently false). `CycleKeywordLocPatch` injects the loc in-code as a workaround.
- Custom powers (`MetallicizePower`, `PlatedArmorPower`) follow Downfall's verified pattern; `ElectricArmorThornsPower` uses BaseLib's `CustomTemporaryPowerModelWrapper` for 1-turn Thorns.
