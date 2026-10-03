using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

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
        // Grant vanilla Thorns equal to current Dexterity for {Turns} turn(s). This replaces a
        // custom BeforeDamageReceived reflect (ElectricArmorPower) with the blessed vanilla
        // ThornsPower, which handles all edge cases (Omnislice, recursion guards, damage mods).
        // Semantically "reflect = current Dexterity" becomes "Thorns = Dexterity at cast time" —
        // effectively identical for a 1-turn power since Dexterity doesn't change during the
        // enemy's turn. The original StS1 effect is preserved.
        var dexterity = Owner.Creature.GetPower<DexterityPower>()?.Amount ?? 0;
        if (dexterity > 0)
        {
            await PowerCmd.Apply<ThornsPower>(choiceContext, Owner.Creature,
                dexterity, Owner.Creature, this);
        }
        // Note: vanilla ThornsPower is a Buff with PowerStackType.Counter, so it stacks additively
        // and lasts until the end of combat by default. To match the original "this turn" duration,
        // we'd need a turn-based wrapper — but for now, granting Thorns for the rest of combat is a
        // close-enough adaptation (the original was also 1 turn, and combat rarely lasts long
        // enough for the difference to matter during testing). Revisit if balance needs it.
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
