using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class UnbalancingBlast : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(10m, ValueProp.Move)];

    public UnbalancingBlast() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Unbalancing Blast",
        "#Deal !Damage! damage.\nSwap your draw pile and discard pile.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        if (Owner.PlayerCombatState is not { } pcs || CombatState is not MegaCrit.Sts2.Core.Combat.CombatState combat) return;
        var drawPile = pcs.DrawPile;
        var discardPile = Owner.PlayerCombatState.DiscardPile;
        var oldDraw = drawPile.Cards.ToList();
        var oldDiscard = discardPile.Cards.ToList();
        foreach (var c in oldDraw) drawPile.RemoveInternal(c);
        foreach (var c in oldDiscard) discardPile.RemoveInternal(c);
        foreach (var c in oldDiscard) drawPile.AddInternal(c);
        foreach (var c in oldDraw) discardPile.AddInternal(c);
        drawPile.RandomizeOrderInternal(Owner, Owner.RunState.Rng.Shuffle, combat);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
