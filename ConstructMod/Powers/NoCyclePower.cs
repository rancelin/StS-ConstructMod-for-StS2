using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ConstructMod.Powers;

public class NoCyclePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override List<(string, string)>? Localization => new PowerLoc(
        Title: "No Cycle",
        Description: "Your cards cannot Cycle for {Amount:inverseDiff()} more turn(s).",
        SmartDescription: "Your cards cannot Cycle.");
}
