# The Construct — In-game test checklist (fixes 1.1–1.8)

Rebuild/copy the mod first:

```
cd ConstructMod
dotnet build -p:BaseLibPath="/home/robin/.nuget/packages/alchyr.sts2.baselib/3.4.7/lib/net9.0/BaseLib.dll"
```

The build copies `ConstructMod.dll` + `ConstructMod.json` into `<game>/mods/ConstructMod/`. Restart the game after building.

---

## How to reach a "mega" (+2) tier

Mega upgrades are the second upgrade rung. Three sources:

1. **ClockworkPhoenix relic** (`CONSTRUCTMOD-CLOCKWORK_PHOENIX`) — while owned, **out of combat**, a Construct card can be upgraded past +1 to +2 at the smith. Earned by upgrading `ClockworkEgg` to its final rung.
2. **MegaBattery relic** (`CONSTRUCTMOD-MEGA_BATTERY`, Uncommon) — on pickup, force-megas one random eligible deck card.
3. **MegaPotion** (`CONSTRUCTMOD-MEGA_POTION`, Rare, combat only) — force-megas one random eligible card in hand.

Because 2 and 3 pick **random** cards, the deterministic path is **ClockworkPhoenix + two rest-site smiths on the same card** (first smith → +1, second smith → +2).

> If your build enables the **dev console** (debug commands), you can shortcut:
> - `relic add CONSTRUCTMOD-CLOCKWORK_PHOENIX`
> - `card CONSTRUCTMOD-<CARD_ID>` (e.g. `card CONSTRUCTMOD-MOLTEN_SMASH`)
> - `upgrade <hand-index>` (out of combat, with Phoenix, this can reach +2)
> - `potion CONSTRUCTMOD-MEGA_POTION`
> - `energy 9`, `draw 5`, `gold 1000`, `godmode` for convenience

IDs are `CONSTRUCTMOD-` + SCREAMING_SNAKE_CASE of the class name.

---

## A. Targeted tests for the 8 fixes

### 1.1 Dead mega branches
- [x] **Heated Strike** (`CONSTRUCTMOD-HEATED_STRIKE`) — Basic Attack, 1⚡, Cycle if Strength < 0.
  - [x] +0: **6** damage, Overheat **5**.
  - [x] +1: **6** damage, Overheat **10**.
  - [x] **+2 (mega): 12** damage (6+6), Overheat **20** (5+5+10). ← was unreachable/no damage
- [x] **Heated Defend** (`CONSTRUCTMOD-HEATED_DEFEND`) — Basic Skill, 1⚡, Cycle if Dexterity < 0.
  - [x] +0: **5** Block, Overheat **5**.
  - [x] +1: **5** Block, Overheat **10**.
  - [x] **+2: 11** Block (5+6), Overheat **20**.
- [x] **Molten Smash** (`CONSTRUCTMOD-MOLTEN_SMASH`) — Uncommon Attack, 2⚡, 15 dmg + 15 Block.
  - [x] +1: 18/18. **+2: 22/22** (was unreachable).
  - [x] Confirm it still overheats the cards to the left/right in hand (turns them into Burns).
- [x] **Rollout** (`CONSTRUCTMOD-ROLLOUT`) — Common Attack, 1⚡, damage = 3 × cards Cycled this turn.
  - [x] +1: multiplier 3, Overheat 15. **+2: multiplier 4**, Overheat 20 (was unreachable).
  - [x] Play with 3+ Cycles this turn → damage preview matches.
  - [x] **With 0 cards Cycled: the "Deals x damage" line reads 0 even if you have Strength** (and the card deals 0).

### 1.2 Create Cores
- [x] **Create Cores** (`CONSTRUCTMOD-CREATE_CORES`) — Common Skill, 1⚡, Overheat 10.
  - [x] +0: Overheat **10**. +1: Overheat **15**.
  - [x] **+2: Overheat stays 15** (was 20), and the 3 Cores generated are **Upgraded** (show "+").

### 1.3 Isolate
- [x] **Isolate** (`CONSTRUCTMOD-ISOLATE`) — Uncommon Skill, 1⚡, Cycles only with >1 enemy.
  - [x] +0: play, then attack a **second** time → only the **first** attack is doubled.
  - [x] +1: **all** attacks this turn are doubled.
  - [x] **+2: all attacks doubled this turn AND next turn.**
  - [x] Text updates correctly per tier ("next attack" → "your attacks" → "…and next turn").

### 1.4 Syphon (applied by Syphon Strike)
- [ ] **Syphon Strike** (`CONSTRUCTMOD-SYPHON_STRIKE`) — Common Attack, 1⚡, 7 dmg.
  - [x] Play it on an enemy → the enemy shows a **Syphon** debuff. **Debuff tip description shows "Whenever {Owner}..."**
  - [ ] Attack that enemy again → you **draw a card** each attack.
  - [ ] **At the end of the enemy's turn (end of round) the Syphon power expires/ticks down** (icon gone after 1 round). It must **not** persist for the whole combat.

### 1.5 Afterburners
- [ ] **Afterburners** (`CONSTRUCTMOD-AFTERBURNERS`) — Rare Skill, 2⚡.
  - [ ] +0: shuffles **3 Burns**; next non-**Rare** card played this turn is played **3 times**.
  - [ ] +1: **2 Burns**; still **3 times**.
  - [ ] **+2: 2 Burns; the next non-Rare card is played 4 times.**
  - [ ] Playing a **Rare** card does **not** consume it (the boost applies to the next non-Rare card).
  - [ ] The power disappears at end of turn if unused.

### 1.6 Backfire
- [ ] **Backfire** (`CONSTRUCTMOD-BACKFIRE`) — Common Attack, 1⚡, take 5, deal 16.
  - [ ] +0: **with ≥5 Block, take NO HP damage** (self-damage is now blockable); deal **16**.
  - [ ] +1: self **4**, deal **19**.
  - [ ] **+2: self 8, deal 30.**
  - [ ] With <5 Block, only the unblocked remainder hits HP.

### 1.7 Reserves
- [ ] **Reserves** (`CONSTRUCTMOD-RESERVES`) — **[Rare]**, **0⚡**, Exhaust, Cycles if HP > threshold.
  - [ ] +0: **threshold 20**; gain 3⚡, draw 3.
  - [ ] +1: threshold **30**. **+2: threshold 50.**
  - [ ] Verify the card frame/rarity shows **Rare** (was Uncommon) and cost **0** (was 1).

### 1.8 Metal Shell
- [ ] **Metal Shell** (`CONSTRUCTMOD-METAL_SHELL`) — **[Uncommon]**, 1⚡.
  - [ ] +0: **4 Block** + **2 Plated Armor**.
  - [ ] +1: 4 Block + **3 Plated Armor** (still Plated Armor — not Metallicize).
  - [ ] **+2: 5 Block + 3 Metallicize.**
  - [ ] Hover tip shows Plated Armor at +0/+1 and Metallicize at +2.

---

## B. Regression / smoke tests

- [ ] Game launches with **no exceptions/stack traces** in `godot.log` after a full combat and a save/reload.
- [ ] Character select shows The Construct; starter deck = 5 Strike + 5 Defend + Attack Mode + Defense Mode; starter relic = Cogwheel.
- [ ] **Cycle** still works (card cycles on draw when its condition is met; once per turn; Cycle counter resets each turn).
- [ ] **Overheat**: a card at/over its Overheat threshold turns into a Burn (e.g. Create Cores, Rollout, Heated cards).
- [ ] **MegaBattery** on pickup megas an eligible deck card; **MegaPotion** megas an eligible hand card.
- [ ] **ClockworkPhoenix**: with it, out of combat, a +1 Construct card can be upgraded to +2; in combat it cannot.
- [ ] Save/reload with a +2 card in the deck → it stays +2 (title "Name+2", correct stats).
- [ ] Other already-mega'd cards still behave (e.g. Backup, Bunker, Omega Cannon, Zapper, Sun Screen, Memory Tap).
- [ ] **Afterburners** + a multi-hit/multi-target card doesn't double-count or crash.
- [ ] **Isolate** + **Double Tap**/**Duplication**-type effects don't stack incorrectly or crash.
- [ ] **Syphon** on a multi-enemy fight only affects the debuffed enemy.
- [ ] No "unknown ID" or missing-power errors introduced by the new `DoubleNextDamagePower`.
- [ ] Removing/decrementing powers (Afterburners, Syphon, Double Next Damage) leaves no stuck icon at 0.

---

## C. Known caveats (not bugs addressed here)

- Mega titles render as vanilla "Name+2" (the purple mega title art is deferred).
- All art/energy icons are placeholder Ironclad assets (`has_pck: false`), and missing-sprite warnings appear in the log.
- The Heat Meter UI and challenge modes are still unported.
- Most cards still lack their mega tier (Phase 3); only the 28 previously-implemented cards plus the ones fixed here have a +2 rung.
