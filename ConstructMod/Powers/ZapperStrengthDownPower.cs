using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class ZapperStrengthDownPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Zapper>();
    protected override bool IsPositive => false;
}
