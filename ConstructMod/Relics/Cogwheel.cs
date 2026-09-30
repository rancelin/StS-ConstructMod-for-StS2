using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

[Pool(typeof(ConstructRelicPool))]
public class Cogwheel : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new PowerVar<ArtifactPower>("Artifact", 1m) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromPower<ArtifactPower>(null) };

    public override async Task BeforeCombatStart()
    {
        if (!Owner.Creature.IsDead)
        {
            Flash();
            await PowerCmd.Apply<ArtifactPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                DynamicVars["Artifact"].BaseValue,
                Owner.Creature,
                null);
        }
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Cogwheel",
        Description: "#Gain *1* Artifact at the start of each combat.",
        Flavor: "A piece of old machinery, carefully preserved.");
}
