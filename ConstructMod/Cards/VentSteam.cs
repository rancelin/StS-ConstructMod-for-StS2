using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace ConstructMod.Cards;

public class VentSteam : AbstractConstructCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<WeakPower>(null),
        HoverTipFactory.FromPower<VulnerablePower>(null)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>("Weak", 1m),
        new PowerVar<VulnerablePower>("Vuln", 1m)
    ];

    public VentSteam() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override List<(string, string)>? Localization => new CardLoc("Vent Steam",
        "#Exhaust 1 card.\nApply {Weak} *Weak* and {Vuln} *Vulnerable*.");

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selected = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
            context: choiceContext, player: Owner, filter: null, source: this)).ToList();
        foreach (var card in selected)
        {
            await CardCmd.Exhaust(choiceContext, card);
        }
        var targets = cardPlay.Target != null ? [cardPlay.Target] : CombatState!.HittableEnemies;
        foreach (var target in targets)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars["Vuln"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Weak"].UpgradeValueBy(1m);
        DynamicVars["Vuln"].UpgradeValueBy(1m);
    }
}
