using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Tumble : AbstractConstructCard
{
    private readonly List<CardModel> _cycledCards = [];

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("Cards", 3m)
    ];

    public Tumble() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Tumble",
        "#Draw {Cards} cards.\nDeal !Damage! damage to a random enemy for each card drawn that *Cycled*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        _cycledCards.Clear();
        CycleEvents.CardCycled += OnCardCycled;
        try
        {
            await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext,
                DynamicVars["Cards"].BaseValue, Owner, this);
        }
        finally
        {
            CycleEvents.CardCycled -= OnCardCycled;
        }
        if (CombatState is not { } combat) return;
        for (int i = 0; i < _cycledCards.Count; i++)
        {
            var enemy = Owner.RunState.Rng.CombatTargets.NextItem(combat.HittableEnemies);
            if (enemy == null) break;
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        }
    }

    private Task OnCardCycled(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card != this) _cycledCards.Add(card);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
