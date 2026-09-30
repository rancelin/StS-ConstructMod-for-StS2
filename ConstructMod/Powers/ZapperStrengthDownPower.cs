using ConstructMod.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Powers;

public class ZapperStrengthDownPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Zapper>();
    protected override bool IsPositive => false;

    public string? CustomPackedIconPath => "BaseLib/images/powers/baselib-power_temp_down.png";
    public string? CustomBigIconPath => "BaseLib/images/powers/big/baselib-power_temp_down_big.png";
    public string? CustomBigBetaIconPath => "BaseLib/images/powers/big/baselib-power_temp_down.png";
}
