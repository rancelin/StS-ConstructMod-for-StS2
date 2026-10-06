using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class FierceBash : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move)];

    public FierceBash() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Fierce Bash",
        "#Deal !Damage! damage.\nHits twice if the enemy doesn't intend to *Attack* or *Defend*.");

    private static bool IntendsToAttackOrDefend(MegaCrit.Sts2.Core.Entities.Creatures.Creature target)
    {
        if (target.Monster == null) return true;
        foreach (var intent in target.Monster.NextMove.Intents)
        {
            if (intent.IntentType is MegaCrit.Sts2.Core.MonsterMoves.Intents.IntentType.Attack
                or MegaCrit.Sts2.Core.MonsterMoves.Intents.IntentType.Defend
                or MegaCrit.Sts2.Core.MonsterMoves.Intents.IntentType.DeathBlow)
            {
                return true;
            }
        }
        return false;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        if (!IntendsToAttackOrDefend(cardPlay.Target))
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
