using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using MegaCrit.Sts2.Core.Localization;

namespace Sts2Mod.Cards;

[CustomID("TC_C15"), Pool(typeof(TravelerCardPool))]
public sealed class WindShelter : ElementCardModel
{
    public WindShelter() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy) { }
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/wind_shelter.png";
    public override bool GainsBlock => true;
    public override TargetType TargetType => CurrentUpgradeLevel > 0 ? TargetType.AllEnemies : TargetType.AnyEnemy;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move), new DynamicVar("Weak", 1m)];
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("WeakTargets", new LocString("cards", CurrentUpgradeLevel > 0 ? "TC_C15.allTargets" : "TC_C15.singleTarget"));
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        ArgumentNullException.ThrowIfNull(CombatState);
        var targets = CurrentUpgradeLevel > 0 ? CombatState.Enemies.Where(e => e.IsAlive).ToArray()
            : new[] { play.Target ?? throw new InvalidOperationException("Wind Shelter requires an enemy target.") };
        foreach (var target in targets)
            await PowerCmd.Apply<WeakPower>(context, target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
