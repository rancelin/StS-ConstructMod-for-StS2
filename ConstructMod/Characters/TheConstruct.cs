using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
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
        // Default starter kit (matches the original StS1 Construct starting deck).
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Strike_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<Defend_Construct>(),
        ModelDb.Card<AttackMode>(),
        ModelDb.Card<DefenseMode>(),
        ModelDb.Card<ModeShift>(),
        // Overheat testing pool (minimal): cards that overheat + tools to suppress/raise/trigger it.
        // HeatedStrike/HeatedDefend overheat at 5 cycles; Rollout at 10. Cores cycle every draw to
        // rack up the counter fast. FlashFreeze suppresses, Coolant raises thresholds, Agitation
        // triggers on overheat, MoltenSmash force-overheats adjacent hand cards.
        ModelDb.Card<HeatedStrike>(),
        ModelDb.Card<HeatedDefend>(),
        ModelDb.Card<Rollout>(),
        ModelDb.Card<FlameCore>(),
        ModelDb.Card<ScopeCore>(),
        ModelDb.Card<CreateCores>(),
        ModelDb.Card<FlashFreeze>(),
        ModelDb.Card<ConstructMod.Cards.Coolant>(),
        ModelDb.Card<Agitation>(),
        ModelDb.Card<MoltenSmash>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<Cogwheel>()];

    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;

    // Placeholder visuals: reuse Ironclad assets until a ConstructMod .pck ships.
    public override string? CustomVisualPath => SceneHelper.GetScenePath("creature_visuals/ironclad");
    public override string? CustomTrailPath => SceneHelper.GetScenePath("vfx/card_trail_ironclad");
    public override string? CustomIconPath => SceneHelper.GetScenePath("ui/character_icons/ironclad_icon");
    public override string? CustomIconTexturePath => ImageHelper.GetImagePath("ui/top_panel/character_icon_ironclad.png");
    public override string? CustomEnergyCounterPath => SceneHelper.GetScenePath("combat/energy_counters/ironclad_energy_counter");
    public override string? CustomRestSiteAnimPath => SceneHelper.GetScenePath("rest_site/characters/ironclad_rest_site");
    public override string? CustomMerchantAnimPath => SceneHelper.GetScenePath("merchant/characters/ironclad_merchant");
    public override string? CustomCharacterSelectBg => SceneHelper.GetScenePath("screens/char_select/char_select_bg_ironclad");
    public override string? CustomCharacterSelectIconPath => ImageHelper.GetImagePath("packed/character_select/char_select_ironclad.png");
    public override string? CustomCharacterSelectLockedIconPath => ImageHelper.GetImagePath("packed/character_select/char_select_ironclad_locked.png");
    public override string? CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    public override string? CustomMapMarkerPath => ImageHelper.GetImagePath("packed/map/icons/map_marker_ironclad.png");
    public override string? CustomAttackSfx => "event:/sfx/characters/ironclad/ironclad_attack";
    public override string? CustomCastSfx => "event:/sfx/characters/ironclad/ironclad_cast";
    public override string? CustomDeathSfx => "event:/sfx/characters/ironclad/ironclad_die";

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
