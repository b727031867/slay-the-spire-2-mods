using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2Mod.Powers;

[CustomID("TC_ROCK_HEART")]
public sealed class RockHeartPower : TravelerAbilityPower
{
    public override string CustomPackedIconPath => "res://Sts2Mod/art/rock_heart.svg";
    private int _carryCap;
    public override Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) _carryCap = Math.Min(Owner.Block, Amount);
        return Task.CompletedTask;
    }
    public override bool ShouldClearBlock(Creature creature) => creature != Owner || _carryCap <= 0;
    public override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
    {
        if (creature != Owner || _carryCap <= 0) return;
        var cap = _carryCap;
        _carryCap = 0; // One native clearing boundary only, even if hooks re-enter.
        // Native Barricade/Blur (and other native preventers) retain the full remainder.
        // Never lower another listener's protection or recreate already-spent block.
        if (preventer != this || Owner.CombatState!.IterateHookListeners()
            .Any(model => model != this && !model.ShouldClearBlock(Owner))) return;
        Flash();
        await CreatureCmd.LoseBlock(Owner, Math.Max(0, Owner.Block - cap));
    }
}
