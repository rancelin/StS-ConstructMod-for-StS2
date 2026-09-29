using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using BaseLib.Abstracts;
using ConstructMod.Cards;
using ConstructMod.Pools;
using ConstructMod.Relics;

namespace ConstructMod.Characters;

public class TheConstruct : CustomCharacterModel
{
    public const string EnergyColorNameValue = "construct";
    public static readonly Color ConstructGold = new("AA9632");

    public override Color NameColor => ConstructGold;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 80;
    public override int StartingGold => 99;
    public override int MaxEnergy => 3;

    public override CardPoolModel CardPool => ModelDb.CardPool<ConstructCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<ConstructRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<ConstructPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<ModeShift>(),
        ModelDb.Card<ModeShift>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<Cogwheel>()];

    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;

    public override List<(string, string)>? Localization => new CharacterLoc(
        Title: "The Construct",
        TitleObject: "the Construct",
        Description: "A mechanical being assembled from the Spire's discarded machines. It shifts between modes, cycling cards to maintain perfect operation.",
        PronounObject: "it",
        PronounSubject: "it",
        PronounPossessive: "its",
        PossessiveAdjective: "its",
        AromaPrinciple: "Rust and ozone",
        EndTurnPingAlive: "Systems nominal.",
        EndTurnPingDead: "Critical failure...",
        EventDeathPrevention: "NL #rThe Construct's core refuses to halt.",
        GoldMonologue: "Currency accepted. Adding to stores.",
        CardsModifierTitle: "Mode Shift",
        CardsModifierDescription: "The Construct adapts its combat routines on the fly.");

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_scratch"
    ];
}
