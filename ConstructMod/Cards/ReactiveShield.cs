using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Cards;

public class ReactiveShield : AbstractConstructCard
{
    // 2-tier ladder: +1 = Innate and +1 damage, +2 = mega (+1 damage and 1 Metallicize).
    // StS1 ReactiveShield UPGRADE_PLUS_POWER_BLOCK_AMT / M_UPGRADE_PLUS_POWER_BLOCK_AMT.
    public override int IntrinsicMaxUpgradeLevel => 2;

    public override bool GainsBlock => true;

    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ReactiveShieldPower>(3m)];

    protected override System.Collections.Generic.IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (var t in base.ExtraHoverTips) yield return t;
            yield return HoverTipFactory.Static(StaticHoverTip.Block);
            if (IsMegaUpgraded) yield return HoverTipFactory.FromPower<MetallicizePower>(null);
        }
    }

    public ReactiveShield() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Reactive Shield",
        "#Whenever you gain *Block*, deal {ReactiveShieldPower} damage to the lowest-HP enemy.{IfMega:mega:\nGain 1 *Metallicize*.|}");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ReactiveShieldPower>(choiceContext, Owner.Creature,
            DynamicVars["ReactiveShieldPower"].BaseValue, Owner.Creature, this);
        // Original: only the mega tier also grants 1 Metallicize.
        if (IsMegaUpgraded)
        {
            await PowerCmd.Apply<MetallicizePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: Innate. Both rungs: +1 damage (3 → 4 → 5).
        if (CurrentUpgradeLevel == 1) AddKeyword(CardKeyword.Innate);
        DynamicVars["ReactiveShieldPower"].UpgradeValueBy(1m);
    }
}
