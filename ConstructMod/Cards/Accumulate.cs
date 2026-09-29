using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Accumulate : AbstractConstructCard
{
    private static readonly LocString CopySelectionPrompt = new("cards", "CONSTRUCTMOD-COPY");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(13m, ValueProp.Move)];

    public Accumulate() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Accumulate",
        "#Deal {Damage} damage.[br]Make a copy of a non-Rare card in your draw pile.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
        var drawPile = PileType.Draw.GetPile(Owner);
        var candidates = drawPile.Cards.Where(c => c.Rarity != CardRarity.Rare).ToList();
        if (candidates.Count == 0) return;
        var selection = candidates.Count == 1
            ? new List<CardModel>(candidates)
            : [await CardSelectCmd.FromChooseACardScreen(choiceContext, candidates, Owner)];
        foreach (var card in selection)
        {
            if (card == null) continue;
            var clone = card.CreateClone();
            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Draw, Owner, CardPilePosition.Random);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
