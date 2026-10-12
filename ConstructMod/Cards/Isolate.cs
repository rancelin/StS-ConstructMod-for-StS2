using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Isolate : AbstractCycleCard
{
    public Isolate() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Isolate",
        "#*Cycle* if there is more than one enemy.\nYour next attack deals double damage this turn.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        return CombatState is { } combat && combat.HittableEnemies.Count > 1;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DoubleDamagePower>(choiceContext, Owner.Creature, 1m,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
