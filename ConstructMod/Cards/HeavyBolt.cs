using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Extensions;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HeavyBolt : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new DynamicVar("Cards", 2m)
    ];

    public HeavyBolt() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Heavy Bolt",
        "#Deal !Damage! damage.\nDiscard {Cards} random cards, then draw {Cards} cards.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        int count = (int)DynamicVars["Cards"].BaseValue;
        var hand = new List<CardModel>(PileType.Hand.GetPile(Owner).Cards);
        hand.StableShuffle(Owner.RunState.Rng.Shuffle);
        var toDiscard = hand.Take(count).ToList();
        if (toDiscard.Count > 0)
        {
            await CardCmd.DiscardAndDraw(choiceContext, toDiscard, toDiscard.Count);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
