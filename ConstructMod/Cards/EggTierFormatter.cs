using System;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

/// <summary>
/// Display state for ClockworkEgg's per-rung text: 0-3 = effect tiers, 4 = Chicken mega,
/// 5 = Scrambled mega. Mirrors IfMegaVar (injected per description evaluation, so the
/// text follows the live level — a C# switch in the Localization getter does NOT: the
/// template bakes per canonical and only formatter macros stay live).
/// </summary>
public class EggTierVar : DynamicVar
{
    public const string DefaultName = "EggTier";

    public int Tier;

    public EggTierVar(int tier) : base(DefaultName, tier)
    {
        Tier = tier;
    }
}

/// <summary>
/// Formatter for {EggTier:eggtier:rung0|rung1|...} — selects the option by EggTierVar value.
/// Single-use for ClockworkEgg (IfMega caps at 2 options; the Egg needs 6).
/// </summary>
public class EggTierFormatter : IFormatter, IAutoRegisterFormatSpecifier
{
    public string Name
    {
        get => "eggtier";
        set => throw new NotSupportedException("Setting the 'Names' property is not supported.");
    }

    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is not EggTierVar tierVar)
        {
            return false;
        }
        var options = formattingInfo.Format?.Split('|');
        if (options == null || tierVar.Tier < 0 || tierVar.Tier >= options.Count)
        {
            throw new LocException(
                $"EggTier {tierVar.Tier} out of range (0-{options?.Count ?? 0}). format={formattingInfo.Format}.");
        }
        formattingInfo.FormatAsChild(options[tierVar.Tier], formattingInfo.CurrentValue);
        return true;
    }
}
