using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Patches;

/// <summary>
/// Injects the Cycle keyword's localization into the card_keywords loc table,
/// since the mod does not ship a .pck with loc files yet.
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
public static class CycleKeywordLocPatch
{
    private static readonly FieldInfo LocDictionaryField =
        AccessTools.Field(typeof(LocTable), "_translations");

    [HarmonyPostfix]
    static void AddKeywordLoc()
    {
        var table = LocManager.Instance.GetTable("card_keywords");
        if (table == null) return;
        var dict = LocDictionaryField.GetValue(table) as Dictionary<string, string>;
        if (dict == null) return;

        dict["CONSTRUCTMOD-CYCLE.title"] = "Cycle";
        dict["CONSTRUCTMOD-CYCLE.description"] =
            "When drawn, discard this and draw a new card. Only works once per turn.";
    }
}
