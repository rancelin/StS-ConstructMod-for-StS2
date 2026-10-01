using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class Backup : AbstractConstructCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            if (IsUpgraded) yield return HoverTipFactory.FromKeyword(CardKeyword.Retain);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Copies", 2m)];

    public Backup() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Backup",
        "#Choose a non-*Rare* card in your hand. Put {Copies} copies of it on top of your draw pile.");

    protected override bool IsPlayable =>
        PileType.Hand.GetPile(Owner).Cards.Any(c => c != this && c.Rarity != CardRarity.Rare);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat) return;
        var candidates = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c != this && c.Rarity != CardRarity.Rare).ToList();
        if (candidates.Count == 0) return;
        CardModel? chosen = candidates.Count == 1
            ? candidates[0]
            : (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(
                    new LocString("cards", "CONSTRUCTMOD-BACKUP.selectionScreenPrompt"), 1),
                context: choiceContext, player: Owner,
                filter: c => c != this && c.Rarity != CardRarity.Rare, source: this)).FirstOrDefault();
        if (chosen == null) return;
        for (var i = 0; i < DynamicVars["Copies"].IntValue; i++)
        {
            var copy = combat.CreateCard(chosen, Owner);
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Draw, Owner, CardPilePosition.Top);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
