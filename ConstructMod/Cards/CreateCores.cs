using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class CreateCores : AbstractConstructCard
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Cores", 3m)];

    public CreateCores() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Create Cores",
        "#Shuffle {Cores} random Core(s) into your draw pile.\nDraw 1 card.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = (int)DynamicVars["Cores"].BaseValue;
        for (int i = 0; i < count; i++)
        {
            CardModel canonical = Owner.RunState.Rng.CombatCardSelection.NextInt(0, 5) switch
            {
                0 => ModelDb.Card<FlameCore>(),
                1 => ModelDb.Card<LaserCore>(),
                2 => ModelDb.Card<ScopeCore>(),
                3 => ModelDb.Card<ForceCore>(),
                _ => ModelDb.Card<GuardCore>()
            };
            CardModel core = CombatState.CreateCard(canonical, Owner);
            if (IsUpgraded) CardCmd.Upgrade(core);
            await CardPileCmd.AddGeneratedCardToCombat(core, PileType.Draw, Owner, CardPilePosition.Random);
        }
        await CardPileCmd.DrawWithoutBlockingOnOtherPlayers(choiceContext, 1, Owner, this);
    }

    protected override void OnUpgrade()
    {
        // Upgraded CreateCores generates upgraded Cores; count stays the same.
    }
}
