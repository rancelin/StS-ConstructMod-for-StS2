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
    // 2-tier ladder: +1 = +5 overheat, +2 = mega (Cores generated Upgraded).
    public override int IntrinsicMaxUpgradeLevel => 2;

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
        "#Shuffle {Cores} random Core(s) into your draw pile.\nDraw 1 card.\n{IfMega:mega:The Cores are Upgraded.\n|}*Overheat*: {Overheat}.");

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
            if (IsMegaUpgraded) CardCmd.Upgrade(core);
            await CardPileCmd.AddGeneratedCardToCombat(core, PileType.Draw, Owner, CardPilePosition.Random);
        }
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, Owner, this);
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +5 overheat tolerance. Mega: description-only (upgraded Cores move
        // here — OnPlay, gated on IsMegaUpgraded).
        UpgradeOverheat(5);
    }
}
