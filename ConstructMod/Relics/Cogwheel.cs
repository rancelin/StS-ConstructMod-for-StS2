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

    /// <summary>
    /// The Orobas "Touch of Orobas" Ancient choice replaces this starter relic with the
    /// returned relic (BaseLib's StarterUpgradePatches dispatches here for CustomRelicModels;
    /// vanilla's fallback is the inert Circlet). Returns the upgraded Cogwheel: keeps 1
    /// Artifact and adds "the first card you Cycle each turn draws 1 additional card" —
    /// the dominant modded-character upgrade pattern (base effect + new reactive trigger
    /// on the character's core mechanic, ~1 Uncommon relic of added value).
    /// </summary>
    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<ClockworkCogwheel>();

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
