using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Cards;
using Sts2Mod.Combo;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestEchoingBlight()
    {
        foreach (var up in new[] { false, true })
        {
            await FreshExpansion();
            var other = _player.Creature.CombatState!.Enemies.First(e => e != _target);
            await PowerCmd.Apply<WeakPower>(ExpansionContext(), _target, 2, _player.Creature, null);
            await PowerCmd.Apply<VulnerablePower>(ExpansionContext(), _target, 3, _player.Creature, null);
            await PowerCmd.Apply<PoisonPower>(ExpansionContext(), _target, 4, _player.Creature, null);
            await PowerCmd.Apply<StrengthPower>(ExpansionContext(), _target, -2, _player.Creature, null);
            await PowerCmd.Apply<DexterityPower>(ExpansionContext(), _target, -1, _player.Creature, null);
            await PowerCmd.Apply<NoDrawPower>(ExpansionContext(), _target, 1, _player.Creature, null);
            await PowerCmd.Apply<PlatingPower>(ExpansionContext(), _target, 5, _target, null);
            await PowerCmd.Apply<WeakPower>(ExpansionContext(), other, 1, _player.Creature, null);
            var effects = _target.Powers.Where(p => p.TypeForCurrentAmount == PowerType.Debuff && p.StackType == PowerStackType.Counter)
                .Select(p => (Power: p, Amount: p.Amount)).ToArray();
            var hp = _target.CurrentHp;
            var energy = _player.PlayerCombatState!.Energy;
            var card = await Add<EchoingBlight>(up);
            await Queue(new PlayCardAction(card, _target));
            Check(effects.Length == 5 && effects.All(e => e.Power.Amount == 2 * e.Amount)
                && _target.GetPower<NoDrawPower>()?.Amount == 1 && _target.GetPower<PlatingPower>()?.Amount == 5
                && other.GetPower<WeakPower>()?.Amount == 1 && _target.CurrentHp == hp
                && _player.PlayerCombatState.Energy == energy - (up ? 0 : 1)
                && card.Keywords.Contains(CardKeyword.Ethereal) == !up && card.Pile?.Type == PileType.Discard
                && Power.State.Last == Element.Lightning && Power.State.Used == 0,
                "BLIGHT_all_stackable_debuffs_only_" + up);

            await FreshExpansion();
            await Play<RockGuard>();
            hp = _target.CurrentHp;
            await Play<EchoingBlight>(up);
            Check(hp - _target.CurrentHp == 4 && Power.State.Used == 1 && Power.State.RewardsThisTurn == 1,
                "BLIGHT_empty_target_still_resolves_one_combo_" + up);

            await FreshExpansion();
            await PowerCmd.Apply<WeakPower>(ExpansionContext(), _target, 2, _player.Creature, null);
            await PowerCmd.Apply<StrengthPower>(ExpansionContext(), _target, -2, _player.Creature, null);
            await PowerCmd.Apply<ArtifactPower>(ExpansionContext(), _target, 2, _target, null);
            await Play<EchoingBlight>(up);
            Check(_target.GetPower<WeakPower>()?.Amount == 2 && _target.GetPower<StrengthPower>()?.Amount == -2
                && _target.GetPower<ArtifactPower>() is null, "BLIGHT_native_artifact_blocks_each_increase_" + up);

            await FreshExpansion(); await SeedDraw();
            card = await Add<EchoingBlight>(up);
            await ExpansionNativeTurn();
            Check(card.Pile?.Type == (up ? PileType.Discard : PileType.Exhaust), "BLIGHT_native_ethereal_upgrade_removal_" + up);
        }
    }
}
