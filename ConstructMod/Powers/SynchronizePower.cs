using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class SynchronizePower : CustomPowerModel
{
    private string? _lastDrawnName;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "Synchronize",
        Description: "Whenever you draw 2 of the same card in a row, deal {Amount} damage to ALL enemies.",
        SmartDescription: "Whenever you draw 2 of the same card in a row, deal {Amount} damage to ALL enemies.");

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner) return;
        var name = card.Title;
        if (_lastDrawnName == name)
        {
            Flash();
            _lastDrawnName = null;
            if (CombatState is { } combat)
            {
                foreach (var enemy in combat.HittableEnemies.ToList())
                {
                    await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner);
                }
            }
        }
        else
        {
            _lastDrawnName = name;
        }
    }
}
