using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class BatteryAcid : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(2), new EnergyVar("NextTurn", 1)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip,
        HoverTipFactory.FromCard<Slimed>()
    ];

    public BatteryAcid() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Battery Acid",
        "#Gain {Energy:energyIcons()} energy.{IfMega:mega:\nNext turn, gain {NextTurn:energyIcons()}.|}\nShuffle 1 *Slimed* into your draw pile.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature,
                DynamicVars["NextTurn"].IntValue, Owner.Creature, this);
        }
        if (CombatState is not { } combat) return;
        var slimed = combat.CreateCard(ModelDb.Card<Slimed>(), Owner);
        await CardPileCmd.AddGeneratedCardToCombat(slimed, PileType.Draw, Owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        // Regular: +1 energy (2 → 3). Mega: no stat change — it also pays out 1 energy next
        // turn (see OnPlay; StS1 M_UPGRADE_PLUS_ENERGY_NEXT_TURN).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Energy.UpgradeValueBy(1);
        }
    }
}
