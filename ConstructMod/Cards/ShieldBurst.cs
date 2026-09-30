using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class ShieldBurst : AbstractConstructCard
{
    protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Mult", 1.5m)];

    public ShieldBurst() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override System.Collections.Generic.List<(string, string)>? Localization => new CardLoc("Shield Burst",
        "#Lose all *Block*. Deal {IfUpgraded:show:{Mult} times that much damage|one and a half times that much damage} to ALL enemies.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = Owner.Creature.Block;
        if (block <= 0) return;
        await CreatureCmd.LoseBlock(choiceContext, Owner.Creature, block, Owner.Creature);
        if (CombatState is not { } combat) return;
        decimal damage = System.Decimal.Floor(block * DynamicVars["Mult"].BaseValue);
        foreach (var enemy in combat.HittableEnemies.ToList())
        {
            await DamageCmd.Attack(damage).FromCard(this, cardPlay).Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Mult"].UpgradeValueBy(0.5m);
    }
}
