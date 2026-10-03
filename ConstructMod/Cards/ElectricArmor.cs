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
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromPower<DexterityPower>(null);
            yield return HoverTipFactory.FromPower<ThornsPower>(null);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Turns", 1m)];

    public ElectricArmor() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Electric Armor",
        "#*Cycle* if your *Dexterity* is less than 1.\\nThis turn, when an enemy attacks you, it takes damage equal to your *Dexterity*.");

    public override bool CanCycle()
    {
        if (!base.CanCycle()) return false;
        var dexterity = Owner.Creature.GetPower<DexterityPower>();
        return dexterity is not { Amount: > 0 };
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Grant 1-turn Thorns equal to current Dexterity, using BaseLib's
        // CustomTemporaryPowerModelWrapper pattern (same as Downfall's Piercing Hide / Slime Spikes):
        // the wrapper internally applies vanilla ThornsPower and auto-removes it at the end of the
        // enemy's turn (UntilEndOfOtherSideTurn => true). This is the blessed BaseLib path — no
        // custom BeforeDamageReceived, no manual end-of-turn removal.
        var dexterity = Owner.Creature.GetPower<DexterityPower>()?.Amount ?? 0;
        if (dexterity > 0)
        {
            await PowerCmd.Apply<ElectricArmorThornsPower>(choiceContext, Owner.Creature,
                dexterity, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
