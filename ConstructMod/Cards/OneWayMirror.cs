using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class OneWayMirror : AbstractConstructCard
{
    public override bool GainsBlock => true;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12m, ValueProp.Move),
        new PowerVar<VulnerablePower>("Vuln", 2m)
    ];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VulnerablePower>(null)];

    public OneWayMirror() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("One-Way Mirror",
        "#Gain !Block! *Block*. Apply {Vuln} *Vulnerable* to ALL enemies.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars["Vuln"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Vuln"].UpgradeValueBy(1m);
    }
}
