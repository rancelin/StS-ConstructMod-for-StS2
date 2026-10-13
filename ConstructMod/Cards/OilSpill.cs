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
    // 2-tier ladder: +1 = +2 oil, +2 = mega (+3 oil, hits ALL enemies).
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Oil", 9m), new OverheatVar()];

    public OilSpill() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        Overheat = 5;
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
            yield return HoverTipFactory.FromCard<MegaCrit.Sts2.Core.Models.Cards.Burn>();
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Oil Spill",
        "#Whenever a *Burn* hits you, {IfMega:mega:ALL enemies|target enemy} take {Oil} damage.\n*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsMegaUpgraded)
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
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        if (CurrentUpgradeLevel == 1)
        {
            // Regular: +2 oil (9 → 11).
            DynamicVars["Oil"].UpgradeValueBy(2m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            // Mega: +3 oil (11 → 14); ALL-enemies targeting moves here.
            DynamicVars["Oil"].UpgradeValueBy(3m);
        }
    }
}
