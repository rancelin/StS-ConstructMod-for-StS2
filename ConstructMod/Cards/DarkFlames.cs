using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class DarkFlames : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new EnergyVar(1),
        new DynamicVar("Burns", 0m)
    ];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip
    ];

    public DarkFlames() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Dark Flames",
        "#{IfMega:mega:Add {Burns} *Burns* to your exhaust pile.\n|}Deal !Damage! damage and gain {Energy:energyIcons()} for each *Burn* in your exhaust pile.");

    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            if (!IsUpgraded) yield return CardKeyword.Exhaust;
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
        if (CombatState is { } c && DynamicVars["Burns"].BaseValue > 0)
        {
            for (var i = 0; i < (int)DynamicVars["Burns"].BaseValue; i++)
            {
                var burn = c.CreateCard(ModelDb.Card<Burn>(), Owner);
                await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Exhaust, Owner, CardPilePosition.Bottom);
            }
        }
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
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: drop Exhaust. Mega: +2 Burns, seeded into the exhaust pile before the
        // damage loop (StS1 M_UPGRADE_ADD_BURNS).
        if (CurrentUpgradeLevel == 1)
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Burns"].UpgradeValueBy(2m);
        }
    }
}
