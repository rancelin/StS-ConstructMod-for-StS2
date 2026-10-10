using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class SunScreen : AbstractConstructCard
{
    // Plain DynamicVar (not BlockVar) so the description shows a flat 3 — BlockVar would preview
    // the Dexterity-modified value, but the power grants nonCardUnpowered block that is
    // deliberately NOT boosted by Dexterity (display and behavior must agree).
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Block", 3m)];

    public SunScreen() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            // Matches vanilla Feel No Pain (the canonical 'exhaust a Status' card): Exhaust
            // keyword tip + Block tip. Vanilla has no Status hover-tip (StaticHoverTip has no
            // Status member), so the gold-highlighted word stays un-tippped for consistency.
            yield return HoverTipFactory.FromKeyword(CardKeyword.Exhaust);
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Sun Screen",
        "#At the end of your turn, *Exhaust* a random *Status* card in your hand to gain {Block} *Block*.{IfUpgraded:show:\nAlso *Exhaust* a random *Curse* card in your hand to gain {Block} *Block*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Mega folded into upgrade: the upgraded card applies BOTH powers (Status + Curse exhaust),
        // matching the original mega behavior where the regular power is kept, not replaced.
        // (DynamicVars["Block"] — the indexer, not the Block property, since this is a plain var.)
        if (IsUpgraded)
        {
            await PowerCmd.Apply<SunScreenMegaPower>(choiceContext, Owner.Creature,
                DynamicVars["Block"].IntValue, Owner.Creature, this);
        }
        await PowerCmd.Apply<SunScreenPower>(choiceContext, Owner.Creature,
            DynamicVars["Block"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Original: +2 block on upgrade, then mega +1 block + the Curse-exhaust power.
        // Folded: +3 block (combined); the Curse power is applied in OnPlay above.
        DynamicVars["Block"].UpgradeValueBy(3m);
    }
}
