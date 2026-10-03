using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Hooks;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class OilSpill : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<OilSpillPower>("Oil", 9m)];

    public OilSpill() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        Overheat = 5;
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
            yield return HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Burn>();
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Oil Spill",
        "#Whenever a *Burn* hits the target, it takes {Oil} damage.\n*Overheat*: !O!.{IfUpgraded:show: Targets ALL enemies.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            // Mega folded into upgrade: targets ALL enemies.
            if (CombatState is { } combat)
            {
                foreach (var enemy in combat.HittableEnemies.ToList())
                {
                    await PowerCmd.Apply<OilSpillPower>(choiceContext, enemy,
                        DynamicVars["Oil"].IntValue, Owner.Creature, this);
                }
            }
        }
        else
        {
            if (cardPlay.Target == null) return;
            await PowerCmd.Apply<OilSpillPower>(choiceContext, cardPlay.Target,
                DynamicVars["Oil"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Original: +2 oil dmg on upgrade, then mega = +3 oil dmg + target ALL_ENEMY.
        // Folded: +5 oil dmg (combined) + target ALL enemies in OnPlay.
        DynamicVars["Oil"].UpgradeValueBy(5m);
    }
}
