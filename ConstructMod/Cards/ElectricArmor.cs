using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class ElectricArmor : AbstractCycleCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<DexterityPower>(null);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Turns", 1m)];

    public ElectricArmor() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Electric Armor",
        // StS1's upgraded text says "this or next turn", which reads like a condition although the
        // power simply lasts two turns; "and" matches what the card actually does.
        "#*Cycle* if your *Dexterity* is less than 1.\nThis{IfMega:mega: and next|} turn, when an enemy attacks you, it takes damage equal to your *Dexterity*.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        var dexterity = Owner.Creature.GetPower<DexterityPower>();
        return dexterity is not { Amount: > 0 };
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Faithful StS1 behavior: grant the live-Dexterity reflect power (reads Dexterity
        // at attack time — Dexterity gained after play still counts).
        await PowerCmd.Apply<ElectricArmorPower>(choiceContext, Owner.Creature,
            DynamicVars["Turns"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: cost 1 → 0. Mega: the reflect lasts an extra turn (1 → 2;
        // StS1 M_UPGRADE_PLUS_TURNS).
        if (CurrentUpgradeLevel == 1)
        {
            EnergyCost.UpgradeBy(-1);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Turns"].UpgradeValueBy(1m);
        }
    }
}
