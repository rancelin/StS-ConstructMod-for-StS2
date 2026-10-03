using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Cards;

public class Tumble : AbstractConstructCard, IAfterCardCycled
{
    private readonly List<CardModel> _cycledCards = [];
    private bool _listening;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("Cards", 3m)
    ];

    public Tumble() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(ConstructKeywords.Cycle)];

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Tumble",
        "#Draw {Cards} cards.\nDeal !Damage! damage to a random enemy for each card drawn that *Cycled*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        _cycledCards.Clear();
        _listening = true;
        try
        {
            await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext,
                DynamicVars["Cards"].BaseValue, Owner, this);
        }
        finally
        {
            _listening = false;
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

    public Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card)
    {
        // Only count cycles that happen during this card's own draw (not other cards' cycles).
        if (_listening && card != this) _cycledCards.Add(card);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
