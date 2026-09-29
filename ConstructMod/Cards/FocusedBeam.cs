using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class FocusedBeam : AbstractConstructCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move)];

    public FocusedBeam() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Focused Beam",
        "#Deal {Damage} damage.[br]Put this on top of your draw pile.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target != null)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_magic_beam", null, "magic_beam.mp3")
                .Execute(choiceContext);
        }
        await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
