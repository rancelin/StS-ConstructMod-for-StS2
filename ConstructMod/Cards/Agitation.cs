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
    public override int IntrinsicMaxUpgradeLevel => 2;
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
        // Mega tier draws 1 so playing it doesn't cost you a hand slot: StS1's startInPlay removed
        // the card from the deck and applied the power for free, which Innate + 0-cost alone can't
        // reproduce (see OnUpgrade). The line leads the text, as StS1's EXTENDED_DESCRIPTION does.
        "#{IfMega:mega:Draw 1 card.\n|}Whenever a card *Overheats*, gain {AgitationPower} *Strength* and {AgitationPower} *Dexterity*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AgitationPower>(choiceContext, Owner.Creature,
            DynamicVars["AgitationPower"].IntValue, Owner.Creature, this);
        if (IsMegaUpgraded)
        {
            await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, Owner, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: Innate. Mega: cost 1 → 0 plus "Draw 1 card" (compensating for StS1's
        // startInPlay, which cost the player no hand slot).
        //
        // StS1's mega tier also sets startInPlay = true ("start combat with this card in play").
        // StS2 has no card-level equivalent: a BeforeCombatStart hook fires on the *deck* card
        // while the combat still holds a separate clone in the draw pile, so auto-applying there
        // would hand out the power twice. Innate + 0-cost + self-replacing draw is the closest
        // safe approximation.
        if (CurrentUpgradeLevel == 1)
        {
            AddKeyword(CardKeyword.Innate);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
