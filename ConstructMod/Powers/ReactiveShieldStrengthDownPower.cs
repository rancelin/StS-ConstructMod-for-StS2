using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using ConstructMod.Cards;

namespace ConstructMod.Powers;

public class ReactiveShieldStrengthDownPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<ReactiveShield>();
    protected override bool IsPositive => false;
}
