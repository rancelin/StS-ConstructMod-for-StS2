using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace ConstructMod.Patches;

/// <summary>
/// Injects localization entries that the mod needs but cannot ship via a .pck yet (has_pck is false).
/// This includes the Cycle custom keyword (registered via [CustomEnum] but with no loc file) and the
/// card-selection-screen prompts referenced by Backup / MassProduction / Multistage / Accumulate.
/// When the mod ships a .pck with proper loc files, this patch should be removed and the strings
/// moved into localization/card_keywords.json and localization/cards.json respectively.
/// </summary>
[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
public static class CycleKeywordLocPatch
{
    private static readonly FieldInfo LocDictionaryField =
        AccessTools.Field(typeof(LocTable), "_translations");

    [HarmonyPostfix]
    static void AddKeywordLoc()
    {
        // Cycle keyword (card_keywords table).
        Inject("card_keywords", "CONSTRUCTMOD-CYCLE.title", "Cycle");
        Inject("card_keywords", "CONSTRUCTMOD-CYCLE.description",
            "When drawn, discard this and draw a new card. Only works once per turn.");

        // Overheat keyword (card_keywords table).
        Inject("card_keywords", "CONSTRUCTMOD-OVERHEAT.title", "Overheat");
        Inject("card_keywords", "CONSTRUCTMOD-OVERHEAT.description",
            "When too many cards Cycle in one turn, this card turns into a Burn for the rest of this combat.");

        // Card selection screen prompts (cards table). Referenced via LocString("cards", ...)
        // by Backup, MassProduction, Multistage, Accumulate, and WeddingRing.
        Inject("cards", "CONSTRUCTMOD-BACKUP.selectionScreenPrompt", "Choose a card to copy.");
        Inject("cards", "CONSTRUCTMOD-MASSPRODUCTION.selectionScreenPrompt", "Choose a card to copy.");
        Inject("cards", "CONSTRUCTMOD-MULTISTAGE.selectionScreenPrompt", "Choose an Attack to copy.");
        Inject("cards", "CONSTRUCTMOD-ACCUMULATE.selectionScreenPrompt", "Choose a card to copy.");
        Inject("cards", "CONSTRUCTMOD-WEDDINGRING.selectionScreenPrompt", "Choose 2 cards to marry.");
    }

    private static void Inject(string tableName, string key, string value)
    {
        var table = LocManager.Instance.GetTable(tableName);
        if (table == null) return;
        if (LocDictionaryField.GetValue(table) is not Dictionary<string, string> dict) return;
        dict[key] = value;
    }
}
