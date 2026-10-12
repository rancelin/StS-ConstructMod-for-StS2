using BaseLib.Abstracts;

namespace ConstructMod.Enchantments;

/// <summary>
/// Marker enchantment for Wedding Ring marriages: no stat effect, purely the visible
/// link between the two married deck cards (plus its tooltip names the pair).
///
/// Design consequences (all StS2-native):
/// - Persistence: engine serializes enchantments with deck cards, so the marriage
///   survives save/load with no custom save code.
/// - Clone-blocking: enchantments are exclusive (one per card), so a married card
///   cannot receive the Clone enchantment — the exponential-clone marriage is
///   prevented at the source rather than guarded at the trigger.
/// - Copies inherit the marker, so the GeneratedCopy guard stays necessary.
/// </summary>
public class MarriedEnchantment : CustomEnchantmentModel
{
}
