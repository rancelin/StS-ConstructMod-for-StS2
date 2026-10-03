using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class DarkFlames : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip
    ];

    public DarkFlames() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Dark Flames",
        "#Deal !Damage! damage and gain {Energy:energyIcons()} for each *Burn* in your exhaust pile.");

    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    private int BurnCount()
    {
        return Owner.PlayerCombatState?.ExhaustPile.Cards.Count(c => c is Burn) ?? 0;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        // Card text: "Deal !Damage! damage and gain {Energy} for each Burn in your exhaust pile."
        // (No flat Burns value — the count is dynamic. Note: the hit loop is intentional because
        // each hit also gains 1 energy, so we can't collapse into a single WithHitCount call.)
        var burns = BurnCount();
        for (var i = 0; i < burns; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_molten_fist").Execute(choiceContext);
            await PlayerCmd.GainEnergy(1m, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
