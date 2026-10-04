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
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(3m, ValueProp.Move)];

    public SunScreen() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Sun Screen",
        "#At the end of your turn, *Exhaust* a random *Status* card in your hand to gain !Block! *Block*.{IfUpgraded:show:\nAlso *Exhaust* a random *Curse* card in your hand to gain !Block! *Block*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Mega folded into upgrade: the upgraded card applies BOTH powers (Status + Curse exhaust),
        // matching the original mega behavior where the regular power is kept, not replaced.
        if (IsUpgraded)
        {
            await PowerCmd.Apply<SunScreenMegaPower>(choiceContext, Owner.Creature,
                DynamicVars.Block.IntValue, Owner.Creature, this);
        }
        await PowerCmd.Apply<SunScreenPower>(choiceContext, Owner.Creature,
            DynamicVars.Block.IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Original: +2 block on upgrade, then mega +1 block + the Curse-exhaust power.
        // Folded: +3 block (combined); the Curse power is applied in OnPlay above.
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
