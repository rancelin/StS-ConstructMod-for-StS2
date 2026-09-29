using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace ConstructMod;

[ModInitializer(nameof(Initialize))]
public static class ConstructModMain
{
    public const string ModId = "ConstructMod";
    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        SimpleLoc.EnableSimpleLoc(ModId);
        Logger.Info("Construct Mod initialized.");
    }
}
