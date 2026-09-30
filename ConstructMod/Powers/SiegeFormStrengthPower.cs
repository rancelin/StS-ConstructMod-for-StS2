using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;
using ConstructMod.Powers;

namespace ConstructMod.Powers;

public class SiegeFormStrengthPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SiegeForm>();
}
