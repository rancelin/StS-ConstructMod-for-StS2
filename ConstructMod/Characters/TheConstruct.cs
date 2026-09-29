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

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_scratch"
    ];
}
