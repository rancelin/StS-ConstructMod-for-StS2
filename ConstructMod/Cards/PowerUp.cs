using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class PowerUp : AbstractConstructCard
{
    // 2-tier ladder: +1 = +1 Boost and drop Exhaust, +2 = mega (description-only; the
    // original's mega also just clears exhaust, which the regular rung already did).
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar("Boost", 2m)
    ];

    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            if (!IsUpgraded) yield return CardKeyword.Exhaust;
        }
    }

    public PowerUp() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Power Up",
        "#Deal !Damage! damage.\nALL of your *Strike* and *Defend* cards gain {Boost} power for this combat.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        var boost = DynamicVars["Boost"].BaseValue;
        foreach (var pile in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
        {
            foreach (var card in pile.GetPile(Owner).Cards)
            {
                // StS1 BoostBasicCardsAction: boost anything tagged BASIC_STRIKE / BASIC_DEFEND.
                // StS2 has no "basic" tag, so the faithful equivalent is CardTag.Strike /
                // CardTag.Defend combined with a Basic rarity check (the engine's own convention
                // for basic Strikes/Defends — cf. Pandora's Box). That covers the Construct's
                // Strike, Defend, Heated Strike and Heated Defend plus any vanilla starter
                // Strikes/Defends in your piles. The original applies the two tags independently.
                if (card.Rarity != CardRarity.Basic) continue;
                if (card.Tags.Contains(CardTag.Strike)
                    && card.DynamicVars.TryGetValue("Damage", out var damage))
                {
                    damage.UpgradeValueBy(boost);
                }
                if (card.Tags.Contains(CardTag.Defend)
                    && card.DynamicVars.TryGetValue("Block", out var block))
                {
                    block.UpgradeValueBy(boost);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: drop Exhaust and +1 Boost (2 → 3; StS1 UPGRADE_PLUS_POWER_PLUS). Mega:
        // the original only clears exhaust (already cleared) and re-renders the same text —
        // no stat change, so the mega rung is description/eligibility only.
        if (CurrentUpgradeLevel == 1)
        {
            RemoveKeyword(CardKeyword.Exhaust);
            DynamicVars["Boost"].UpgradeValueBy(1m);
        }
    }
}
