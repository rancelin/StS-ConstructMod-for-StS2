using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class CripplingShot : AbstractConstructCard
{
    public override int IntrinsicMaxUpgradeLevel => 2;
    private const string WeakAmount = "99";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>(null)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0m, ValueProp.Move), new DynamicVar("StrLoss", 0m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            foreach (var k in base.CanonicalKeywords) yield return k;
            yield return CardKeyword.Exhaust;
        }
    }

    public CripplingShot() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Crippling Shot",
        "#Deal !Damage! damage.\nIf this dealt unblocked damage, apply 99 *Weak*{IfMega:mega: and -!StrLoss! *Strength*|}.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash");
        await attack.Execute(choiceContext);

        bool unblocked = attack.Results
            .SelectMany(r => r)
            .Any(r => r.UnblockedDamage > 0);
        if (unblocked)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, 99m, Owner.Creature, this);
            var strLoss = DynamicVars["StrLoss"].BaseValue;
            if (strLoss > 0)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -strLoss, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        // Branch on the level the upgrade just reached (replayed in order on save/load).
        // Regular: +3 damage (0 → 3). Mega: the shot also strips 2 Strength on unblocked
        // damage (StS1 M_UPGRADE_STR_LOSS). The card's "Exhaust" is a keyword.
        if (CurrentUpgradeLevel == 1)
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
        else if (CurrentUpgradeLevel == 2)
        {
            DynamicVars["StrLoss"].UpgradeValueBy(2m);
        }
    }
}
