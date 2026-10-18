using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Antimatter : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Mult", 4m)];

    public Antimatter() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Antimatter",
        "#Reduce your energy to 0{IfMega:mega:, and regain it next turn|}.\nDeal damage equal to !Mult! times the number of other cards in your hand.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        // Mega banks the spent energy and returns it next turn (StS1 EnergizedPower).
        var spent = Owner.PlayerCombatState?.Energy ?? 0;
        await PlayerCmd.SetEnergy(0m, Owner);
        if (IsMegaUpgraded && spent > 0)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, spent, Owner.Creature, this);
        }
        var handCount = PileType.Hand.GetPile(Owner).Cards.Count(c => c != this);
        await DamageCmd.Attack(handCount * DynamicVars["Mult"].BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // Regular: +1 per card (4 → 5). Mega: no stat change — the spent energy now comes back
        // (see OnPlay; StS1's EXTENDED_DESCRIPTION[2]).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Mult"].UpgradeValueBy(1m);
        }
    }
}
