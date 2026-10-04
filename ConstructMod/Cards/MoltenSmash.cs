using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;
using ConstructMod.Hooks;

namespace ConstructMod.Cards;

public class MoltenSmash : AbstractConstructCard
{
    // Neighbors cached by MoltenSmashNeighborPatch (Harmony prefix on CardModel.OnPlayWrapper)
    // while the card is still in the hand pile — by the time OnPlay runs, the card has already
    // moved to the Play pile, so its hand position can no longer be queried.
    private readonly List<CardModel> _cachedNeighbors = [];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(15m, ValueProp.Move), new BlockVar(15m, ValueProp.Move)];

    public MoltenSmash() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        // No overheat of its own — MoltenSmash force-overheats adjacent hand cards.
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Cycle);
            yield return HoverTipFactory.FromKeyword(ConstructKeywords.Overheat);
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
        }
    }

    public override List<(string, string)>? Localization => new CardLoc("Molten Smash",
        "#Deal !Damage! damage.\nGain !Block! *Block*.\n*Overheat* the cards to the left and right of this in your hand.");

    /// <summary>
    /// Called by <c>MoltenSmashNeighborPatch</c> (Harmony prefix on <see cref="CardModel.OnPlayWrapper"/>)
    /// while this card is still in the hand pile. Snapshots the left/right neighbors so
    /// <see cref="OnPlay"/> can overheat them after the card has moved to the Play pile.
    /// </summary>
    internal void CacheHandNeighbors()
    {
        _cachedNeighbors.Clear();
        if (Pile?.Type != PileType.Hand) return;
        var hand = Pile.Cards.ToList();
        var index = hand.IndexOf(this);
        if (index < 0) return;
        if (index > 0) _cachedNeighbors.Add(hand[index - 1]);
        if (index < hand.Count - 1) _cachedNeighbors.Add(hand[index + 1]);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target).WithHitFx("vfx/vfx_heavy_blunt").Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // Force-overheat the cached adjacent cards (any card type — a standard Strike overheats
        // just like a Heat card, matching the original OverheatAction on leftCard/rightCard).
        // The original passes triggerOnOverheat=false, so Agitation/etc. do NOT fire from this —
        // just the Burn replacement.
        foreach (var neighbor in _cachedNeighbors)
        {
            // TransformTo<Burn> does the in-pile Burn replacement. Skip un-transformable cards
            // (e.g. some statuses) rather than throwing.
            if (neighbor.Pile == null || !neighbor.IsTransformable) continue;
            await CardCmd.TransformTo<Burn>(neighbor);
        }
        _cachedNeighbors.Clear();
    }

    protected override void OnUpgrade()
    {
        // Original: +3 dmg/+3 block on upgrade, then mega = +4 dmg/+4 block.
        // Folded: +7 dmg AND +7 block (combined deltas).
        DynamicVars.Damage.UpgradeValueBy(7m);
        DynamicVars.Block.UpgradeValueBy(7m);
    }
}
