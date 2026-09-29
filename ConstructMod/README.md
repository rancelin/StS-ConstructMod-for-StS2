# The Construct — Slay the Spire 2 port

C# port of [Moocowsgomoo's The Construct](https://github.com/Moocowsgomoo/StS-ConstructMod) for Slay the Spire 2.

## Status

Early skeleton:
- Custom character `TheConstruct` (BaseLib `CustomCharacterModel`) with card/relic/potion pools and starting deck.
- `Cycle` keyword (BaseLib `CustomEnum` CardKeyword) and `AbstractCycleCard` base: cycles discard-and-draw once per turn when drawn, if the card's condition is met.
- Cards: Strike_Construct, Defend_Construct, ModeShift.
- Relic: Cogwheel (starter, no effect yet).
- Power: NoCyclePower (stub).

## Building

Requires the .NET 9 SDK and a Slay the Spire 2 install (v0.111.0), plus a built BaseLib.

```
cd ConstructMod
dotnet build -p:Sts2Path="/path/to/Slay the Spire 2" -p:BaseLibPath="/path/to/BaseLib.dll"
```

Outputs to `publish/` and copies to `<game>/mods/ConstructMod/` when `ModsPath` resolves.

## Porting notes

- STS1 mega-upgrades, Heat Meter UI, challenge modes, and the settings panel are not yet ported.
- Card/relic/power localization is in-code via BaseLib `CardLoc`/`RelicLoc`/`PowerLoc`.
- The Cycle keyword tooltip text needs a localization table; a .pck will be added later (`has_pck` is currently false).
