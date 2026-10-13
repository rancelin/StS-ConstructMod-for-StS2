using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Chainstrike : AbstractConstructCard
{
    // 2-tier ladder: +1 = +1 card played (1 → 2), +2 = mega (cost → 1).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new CardsVar("Cards", 1)
    ];

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    public Chainstrike() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Chainstrike",
        "#Deal !Damage! damage, then play {Cards} random Attack(s) from your draw pile on the same target.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        var drawPile = PileType.Draw.GetPile(Owner);
        var attacks = drawPile.Cards
            .Where(c => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();
        attacks.StableShuffle(Owner.RunState.Rng.Shuffle);
        int count = (int)System.Math.Min(DynamicVars["Cards"].BaseValue, attacks.Count);
        for (int i = 0; i < count; i++)
        {
            if (CombatManager.Instance.IsOverOrEnding) break;
            var card = attacks[i];
            await CardPileCmd.Add(card, PileType.Play);
            await CardCmd.AutoPlay(choiceContext, card, cardPlay.Target);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +1 card (1 → 2). Mega: cost 2 → 1.
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Cards"].UpgradeValueBy(1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
