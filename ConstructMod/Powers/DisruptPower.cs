using ConstructMod.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ConstructMod.Powers;

public class DisruptPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Disrupt>();
    protected override bool IsPositive => false;
}
