using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Isolate : AbstractCycleCard
{
    // 2-tier ladder: +1 = effect broadens from "next attack" to "all attacks"; +2 = mega (+1 turn).
    public override int IntrinsicMaxUpgradeLevel => 2;

    public Isolate() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Isolate",
        "#*Cycle* if there is more than one enemy.\n{IfPlusOne:plusone:Your attacks deal double damage this turn.|Your next attack deals double damage this turn.}{IfMega:mega:\nYour attacks also deal double damage next turn.|}");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        return CombatState is { } combat && combat.HittableEnemies.Count > 1;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Un-upgraded: next attack only. Upgraded: all attacks this turn. Mega: this and next turn.
        if (!IsUpgraded)
        {
            await PowerCmd.Apply<DoubleNextDamagePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<DoubleDamagePower>(choiceContext, Owner.Creature,
                IsMegaUpgraded ? 2m : 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // No stat delta at either tier — the effect change is driven by IsUpgraded/IsMegaUpgraded
        // in OnPlay (matching the original, whose commented-out cost change was never enabled).
    }
}
