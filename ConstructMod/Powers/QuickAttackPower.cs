using System.Threading.Tasks;
using ConstructMod.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ConstructMod.Powers;

/// <summary>
/// Grants temporary Dexterity that is removed at the end of this turn (Quick Attack).
/// </summary>
public class QuickAttackPower : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<QuickAttack>();
}
