using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Sts2Mod.Pools;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestTargetDamagePreview()
    {
        var cards = ModelDb.CardPool<TravelerCardPool>().AllCards
            .Where(c => c.DynamicVars.Values.Any(v => v is DamageVar or CalculatedDamageVar)).ToArray();
        foreach (var language in new[] { "eng", "zhs" })
        {
            using var json = System.Text.Json.JsonDocument.Parse(Godot.FileAccess.GetFileAsString(
                $"res://Sts2Mod/localization/{language}/cards.json"));
            Check(cards.All(card => card.DynamicVars.Values.Where(v => v is DamageVar or CalculatedDamageVar)
                .All(v => json.RootElement.GetProperty(card.Id.Entry + ".description").GetString()!
                    .Contains("{" + v.Name + ":diff()}"))), "PREVIEW_damage_format_" + language);
        }
        var other = _player.Creature.CombatState!.Enemies.First(enemy => enemy != _target);
        var context = new BlockingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(context, _player.Creature, 2, _player.Creature, null);
        await PowerCmd.Apply<WeakPower>(context, _player.Creature, 1, _target, null);
        await PowerCmd.Apply<VulnerablePower>(context, other, 2, _player.Creature, null);
        await PowerCmd.Apply<WeakPower>(context, other, 2, _player.Creature, null);
        var encoded = Power.State.Encoded;
        var rng = _player.RunState.Rng.CombatTargets.Counter;
        var energy = _player.PlayerCombatState!.Energy;
        var hp = _player.Creature.CombatState.Enemies.Select(e => e.CurrentHp).ToArray();
        foreach (var canonical in cards.Where(c => c.TargetType == TargetType.AnyEnemy))
        foreach (var upgraded in new[] { false, true })
        {
            var card = _player.Creature.CombatState.CreateCard(canonical, _player);
            if (upgraded) CardCmd.Upgrade(card);
            await CardPileCmd.Add(card, PileType.Hand);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var node = NCard.FindOnTable(card)!;
            var damage = card.DynamicVars.Values.Single(v => v is DamageVar or CalculatedDamageVar);
            var baseValue = damage.BaseValue;
            node.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            node.SetPreviewTarget(_target);
            var plain = damage.PreviewValue;
            node.SetPreviewTarget(other);
            var vulnerable = damage.PreviewValue;
            Check(vulnerable > plain && node.GetNode<RichTextLabel>("%DescriptionLabel").Text
                .Contains(damage.ToHighlightedString(false)), "PREVIEW_target_damage_" + card.Id.Entry + "_" + upgraded);
            node.SetPreviewTarget(_target);
            Check(damage.PreviewValue == plain && node.GetNode<RichTextLabel>("%DescriptionLabel").Text
                .Contains(damage.ToHighlightedString(false)), "PREVIEW_switch_back_" + card.Id.Entry + "_" + upgraded);
            node.SetPreviewTarget(null);
            Check(damage.PreviewValue == plain && damage.BaseValue == baseValue
                && node.GetNode<RichTextLabel>("%DescriptionLabel").Text.Contains(damage.ToHighlightedString(false)),
                "PREVIEW_clear_target_" + card.Id.Entry + "_" + upgraded);
            await CardPileCmd.RemoveFromCombat([card]);
        }
        Check(Power.State.Encoded == encoded && _player.RunState.Rng.CombatTargets.Counter == rng
            && _player.PlayerCombatState.Energy == energy
            && _player.Creature.CombatState.Enemies.Select(e => e.CurrentHp).SequenceEqual(hp),
            "PREVIEW_readonly_state_rng_energy_hp");
        await FreshExpansion();
        GD.Print("STS2MOD_DAMAGE_PREVIEW_COMPLETE formats=eng,zhs targeting=PASS readonly=PASS rendering=NOT_RUN");
    }
}
