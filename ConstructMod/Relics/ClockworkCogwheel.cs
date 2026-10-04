using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using ConstructMod.Hooks;
using ConstructMod.Pools;

namespace ConstructMod.Relics;

/// <summary>
/// The upgraded Cogwheel, granted by the Ancient Orobas's "Touch of Orobas" choice
/// (via <see cref="Cogwheel.GetUpgradeReplacement"/> and BaseLib's StarterUpgradePatches).
/// Keeps the starter's 1 Artifact and adds a reactive trigger on the Construct's core
/// mechanic — the first card Cycled each turn draws 1 additional card — matching the
/// dominant modded-character upgrade pattern (Guardian/Champ-style: base effect + new
/// trigger on the character's signature mechanic, ~1 Uncommon relic of added value).
/// Starter rarity means it never rolls as a random reward — only obtainable through
/// the Orobas upgrade, like vanilla's BlackBlood.
/// </summary>
[Pool(typeof(ConstructRelicPool))]
public class ClockworkCogwheel : CustomRelicModel, IAfterCardCycled
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    private bool _drewExtraThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new PowerVar<ArtifactPower>("Artifact", 1m) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new IHoverTip[] { HoverTipFactory.FromPower<ArtifactPower>(null) };

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.IsDead) return;
        Flash();
        await PowerCmd.Apply<ArtifactPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            DynamicVars["Artifact"].BaseValue,
            Owner.Creature,
            null);
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner) _drewExtraThisTurn = false;
        return Task.CompletedTask;
    }

    public async Task AfterCardCycled(PlayerChoiceContext ctx, CardModel card)
    {
        // Only the relic owner's own cycles, and only the first cycle each turn.
        if (card.Owner != Owner || _drewExtraThisTurn) return;
        _drewExtraThisTurn = true;
        Flash();
        await CardPileCmd.Draw(ctx, 1, Owner);
    }

    public override List<(string, string)>? Localization => new RelicLoc(
        Title: "Clockwork Cogwheel",
        Description: "#Gain *1* Artifact at the start of each combat. The first card you *Cycle* each turn draws 1 additional card.",
        Flavor: "A piece of old machinery, lovingly over-engineered.");
}
