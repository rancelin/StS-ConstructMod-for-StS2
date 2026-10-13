using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Overcharge : AbstractConstructCard
{
    // 2-tier ladder: +1 = cost 1 → 0, +2 = mega (+2 energy immediately on play).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip,
        HoverTipFactory.FromCard<Burn>()
    ];

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new EnergyVar(1)];

    public Overcharge() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Overcharge",
        "#At the start of your turn, gain {Energy:energyIcons()} and add a *Burn* to your hand.{IfMega:mega:\nGain 2 [E].|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsMegaUpgraded)
        {
            await PlayerCmd.GainEnergy(2m, Owner);
        }
        await PowerCmd.Apply<OverchargePower>(choiceContext, Owner.Creature,
            DynamicVars.Energy.IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: cost 1 → 0. Mega: no cost change — the immediate +2 energy moves here.
        EnergyCost.UpgradeBy(-1);
    }
}
