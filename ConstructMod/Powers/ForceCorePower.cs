using System.Collections.Generic;
using System.Threading.Tasks;
using ConstructMod.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ConstructMod.Powers;

/// <summary>
/// Grants temporary Strength that is removed at the end of this turn (Force Core).
/// </summary>
public class ForceCorePower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<ForceCore>();
}
