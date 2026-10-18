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
    public override int IntrinsicMaxUpgradeLevel => 2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(13m, ValueProp.Move), new DynamicVar("Copies", 1m)];

    public Accumulate() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Accumulate",
        "#Deal !Damage! damage.\nMake {IfMega:mega:!Copies! copies|a copy} of a non-*Rare* card in your draw pile.");

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
            : (await CardSelectCmd.FromCombatPile(choiceContext, drawPile, Owner,
                new CardSelectorPrefs(new LocString("cards", "CONSTRUCTMOD-ACCUMULATE.selectionScreenPrompt"), 1),
                c => c.Rarity != CardRarity.Rare)).ToList();
        // Mega copies the chosen card several times (StS1's M_UPGRADE_PLUS_COPIES).
        var copies = (int)DynamicVars["Copies"].BaseValue;
        var added = new List<CardPileAddResult>();
        foreach (var card in selection)
        {
            if (card == null) continue;
            for (var i = 0; i < copies; i++)
            {
                var clone = card.CreateClone();
                Relics.WeddingRing.MarkGeneratedCopy(clone);
                added.Add(await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Draw, Owner, CardPilePosition.Random));
            }
        }
        CardCmd.PreviewCardPileAdd(added); // keep the draw-pile badge in sync (vanilla pattern)
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +4 damage (13 → 17). Mega: copy instead of a copy — +2 copies, so the
        // text flips to "Make N copies" (StS1 M_UPGRADE_PLUS_COPIES).
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["Copies"].UpgradeValueBy(2m);
        }
    }
}
