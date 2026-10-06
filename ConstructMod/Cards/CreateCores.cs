using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class CreateCores : AbstractConstructCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Cores", 3m), new OverheatVar()];

    public CreateCores() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        Overheat = 10;
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Create Cores",
        "#Shuffle {Cores} random Core(s) into your draw pile.\nDraw 1 card.\n{IfUpgraded:show:The Cores are Upgraded.\n|}*Overheat*: {Overheat}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars["Cores"].BaseValue;
        for (int i = 0; i < count; i++)
        {
            CardModel canonical = ConstructCores.RandomCore(Owner);
            if (CombatState is not { } combat)
            {
                break;
            }
            CardModel core = combat.CreateCard(canonical, Owner);
            if (IsUpgraded) CardCmd.Upgrade(core);
            await CardPileCmd.AddGeneratedCardToCombat(core, PileType.Draw, Owner, CardPilePosition.Random);
        }
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, Owner, this);
    }

    protected override void OnUpgrade()
    {
        // Upgraded CreateCores generates upgraded Cores; count stays the same.
        // Original: upgradeOverheat(+5) on upgrade; mega-upgrade is description-only (no overheat delta).
        UpgradeOverheat(5);
    }
}
