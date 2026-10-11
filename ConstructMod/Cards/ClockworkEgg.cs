using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using BaseLib.Utils;

namespace ConstructMod.Cards;

/// <summary>
/// Clockwork Egg (StS1 Rare): a 0-cost card with a 4-rung manual ladder — each upgrade
/// +1 cost with a new effect (upgraded Modes → +3 Str/Dex → Egg relics + Phoenix and
/// self-removal), then the mega tier: a combat-upgraded ("scrambled") Egg confuses and
/// draws 5, otherwise it grants a random relic. Mapped onto the native multi-level
/// system (IntrinsicMaxUpgradeLevel 4, one rung per level).
///
/// Documented deviations: the mega card keeps its name (StS1 renames to Clockwork
/// Chicken / Scrambled Egg — no title-override API); the random relic is granted
/// immediately rather than added to the post-combat rewards (no relic-to-rewards API).
/// </summary>
public class ClockworkEgg : AbstractConstructCard
{
    // 4 rungs: +1/+2/+3 effect tiers, +4 mega. Scrambled set on ANY in-combat upgrade
    // (StS1 upgrade(forced, inCombat)), but only read at the mega rung.
    public override int IntrinsicMaxUpgradeLevel => 4;

    private static readonly SpireField<CardModel, bool> ScrambledField =
        new SpireField<CardModel, bool>(_ => false).CopyOnClone();

    public ClockworkEgg() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(CardKeyword.Exhaust);
        }
    }

    // Per-rung text (a C# switch, not formatters: IfMega/IfUpgraded cannot split +1/+2/+3).
    // The engine evaluates descriptions per level (cf. upgrade-preview handling), so the
    // hand text always matches the current rung.
    public override List<(string, string)>? Localization => new CardLoc("Clockwork Egg", CurrentTierText());

    private string CurrentTierText() => CurrentUpgradeLevel switch
    {
        1 => "#Add an upgraded *Attack Mode* and an upgraded *Defense Mode* to your hand.\nCan be *Upgraded* 2 more times.",
        2 => "#Gain 3 *Strength*. Gain 3 *Dexterity*.\nCan be *Upgraded* 1 more time.",
        3 => "#Gain Molten, Toxic and Frozen Eggs and a *Clockwork Phoenix*. Permanently remove this card from your deck.",
        _ when IsMegaUpgraded && ScrambledField.Get(this) =>
            "#Gain *Confused*. Draw 5 cards.",
        _ when IsMegaUpgraded =>
            "#Gain a random relic.",
        _ => "#Draw a card.\n*Exhaust*. Can be *Upgraded* 3 more times.",
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        switch (CurrentUpgradeLevel)
        {
            case 0:
                await CardPileCmd.Draw(choiceContext, 1m, Owner);
                break;
            case 1:
                await AddUpgradedMode<AttackMode>(choiceContext);
                await AddUpgradedMode<DefenseMode>(choiceContext);
                break;
            case 2:
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
                await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
                break;
            case 3:
                await ObtainEggRelic<MoltenEgg>();
                await ObtainEggRelic<ToxicEgg>();
                await ObtainEggRelic<FrozenEgg>();
                await ObtainEggRelic<Relics.ClockworkPhoenix>();
                // Permanently remove this card from the run deck (StS1 UUID removal).
                if (cardPlay.Card.DeckVersion is { } deckCard)
                    await CardPileCmd.RemoveFromDeck(deckCard, true);
                break;
            default:
                if (ScrambledField.Get(this))
                {
                    await PowerCmd.Apply<ConfusedPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
                    await CardPileCmd.Draw(choiceContext, 5m, Owner);
                }
                else
                {
                    await GrantRandomRelic(choiceContext);
                }
                break;
        }
    }

    private async Task AddUpgradedMode<T>(PlayerChoiceContext choiceContext)
        where T : CardModel
    {
        if (CombatState is not { } combat) return;
        var copy = combat.CreateCard(ModelDb.Card<T>(), Owner);
        if (copy.IsUpgradable) CardCmd.Upgrade(copy);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
    }

    private async Task ObtainEggRelic<T>() where T : RelicModel
    {
        if (Owner.Relics?.OfType<T>().Any() == true) return;
        await RelicCmd.Obtain<T>(Owner);
    }

    private async Task GrantRandomRelic(PlayerChoiceContext choiceContext)
    {
        // StS1 returnRandomRelicTier: <50 Common, >82 Rare, else Uncommon.
        var roll = Owner.RunState.Rng.CombatCardGeneration.NextFloat() * 100;
        var rarity = roll < 50 ? RelicRarity.Common
            : roll > 82 ? RelicRarity.Rare
            : RelicRarity.Uncommon;
        var options = ModelDb.AllSharedRelicPools
            .SelectMany(p => p.GetUnlockedRelics(Owner.UnlockState))
            .Where(r => r.Rarity == rarity && Owner.Relics?.All(o => o.GetType() != r.GetType()) != false)
            .ToList();
        if (options.Count == 0) return;
        var template = Owner.RunState.Rng.CombatCardGeneration.NextItem(options);
        if (template?.ToMutable() is not { } mutable) return;
        await RelicCmd.Obtain(mutable, Owner, -1);
    }

    protected override void OnUpgrade()
    {
        // Any in-combat upgrade scrambles the eventual mega (StS1 upgrade(forced, inCombat)).
        // (Save/load replay evaluates out of combat, matching StS1's non-persisted flag.)
        if (Owner?.Creature.CombatState != null) ScrambledField.Set(this, true);
        if (CurrentUpgradeLevel < IntrinsicMaxUpgradeLevel)
        {
            // Rungs +1/+2/+3: +1 cost each (0 → 1 → 2 → 3).
            EnergyCost.UpgradeBy(1);
        }
        else
        {
            // Mega rung: back to 0 cost (3 → 0).
            EnergyCost.UpgradeBy(-3);
        }
    }
}
