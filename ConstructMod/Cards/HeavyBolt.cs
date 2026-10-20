using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Extensions;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class HeavyBolt : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new DynamicVar("Cards", 2m)
    ];

    public HeavyBolt() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Heavy Bolt",
        "#Deal !Damage! damage.\nDiscard {Cards}{IfMega:mega:| random} cards, then draw {Cards} cards.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        int count = (int)DynamicVars["Cards"].BaseValue;
        List<CardModel> toDiscard;
        if (IsMegaUpgraded)
        {
            // StS1's mega tier discards cards you choose instead of random ones.
            toDiscard = (await CardSelectCmd.FromHandForDiscard(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, count),
                context: choiceContext, player: Owner, filter: null, source: this)).ToList();
        }
        else
        {
            var hand = new List<CardModel>(PileType.Hand.GetPile(Owner).Cards);
            hand.StableShuffle(Owner.RunState.Rng.Shuffle);
            toDiscard = hand.Take(count).ToList();
        }
        if (toDiscard.Count > 0)
        {
            await CardCmd.DiscardAndDraw(choiceContext, toDiscard, count);
        }
        else
        {
            await CardPileCmd.Draw(choiceContext, count, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +4 damage (15 → 19). Mega: cost 2 → 1, and the discard becomes a choice
        // rather than random (see OnPlay; StS1 M_UPGRADE_NEW_COST).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
