using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class Agitation : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<AgitationPower>(1m)];

    public Agitation() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
        // No overheat (default -1) — Agitation reacts to overheat events, doesn't generate them.
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Agitation",
        "#Whenever a card *Overheats*, gain {AgitationPower} *Strength* and {AgitationPower} *Dexterity*.{IfUpgraded:show: *Innate*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AgitationPower>(choiceContext, Owner.Creature,
            DynamicVars["AgitationPower"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Original: +Innate on upgrade, then mega = cost 0 + startInPlay (free application at combat start).
        // Folded: +Innate + cost 0 (the startInPlay auto-application at combat start is harder to
        // replicate in StS2 without a dedicated hook; the Innate + 0-cost covers most of the value).
        AddKeyword(CardKeyword.Innate);
        EnergyCost.UpgradeBy(-1);
    }
}
