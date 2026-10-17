using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
namespace ConstructMod.Cards;

public class Afterburners : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Burn>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Plays", 3m),
        new DynamicVar("Burns", 3m)
    ];

    public Afterburners() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int IntrinsicMaxUpgradeLevel => 2;

    public override List<(string, string)>? Localization => new CardLoc("Afterburners",
        "#Shuffle {Burns} *Burn* into your draw pile.\nThis turn, your next non-*Rare* card is played {Plays} times.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        // OnUpgrade already decrements the Burns var by 1, so just read the resolved value here.
        var burns = DynamicVars["Burns"].IntValue;
        for (var i = 0; i < burns; i++)
        {
            var burn = combat.CreateCard(ModelDb.Card<Burn>(), Owner);
            await CardPileCmd.AddGeneratedCardToCombat(burn, PileType.Draw, Owner, CardPilePosition.Random);
        }
        await PowerCmd.Apply<Powers.AfterburnersPower>(choiceContext, Owner.Creature,
            DynamicVars["Plays"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // Regular: Burns 3 → 2 (Plays stays 3). Mega: Plays 3 → 4 (Burns stays 2).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars["Burns"].UpgradeValueBy(-1m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Plays"].UpgradeValueBy(1m);
        }
    }
}
