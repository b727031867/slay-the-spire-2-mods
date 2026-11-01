using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Sts2Mod.Pools;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

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
        await TestPendingCompassDamagePreview(cards);
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
        await TestPreviewDamageResolution(cards);
        await FreshExpansion();
        GD.Print("STS2MOD_DAMAGE_PREVIEW_COMPLETE formats=eng,zhs targeting=PASS readonly=PASS rendering=NOT_RUN");
    }

    private async Task TestPendingCompassDamagePreview(CardModel[] cards)
    {
        await FreshExpansion();
        var compass = await RelicCmd.Obtain<UnboundJourneyCompass>(_player);
        var context = new BlockingPlayerChoiceContext();
        foreach (var canonical in cards.Where(card => card.Type == CardType.Attack && card.TargetType == TargetType.AnyEnemy))
        foreach (var upgraded in new[] { false, true })
        {
            await compass.BeforeCombatStart();
            await PowerCmd.Remove<StrengthPower>(_player.Creature);
            await PowerCmd.Apply<StrengthPower>(context, _player.Creature, 2, _player.Creature, null);
            await PowerCmd.Remove<VulnerablePower>(_target);
            await PowerCmd.Remove<WeakPower>(_target);
            await PowerCmd.Remove<PoisonPower>(_target);
            await PowerCmd.Remove<SoarPower>(_target);
            if (upgraded)
            {
                await PowerCmd.Apply<SoarPower>(context, _target, 1, _target, null);
                await PowerCmd.Apply<VulnerablePower>(context, _target, 2, _player.Creature, null);
            }
            Power.State.ClearCombat();
            var last = CombatCardElements.Get(canonical) == Element.Wind ? Element.Rock : Element.Wind;
            Power.State.Reserve(new object(), last);
            Power.State.Reserve(new object(), last == Element.Wind ? Element.Rock : Element.Wind);
            Power.State.Reserve(new object(), last);
            await PlayerCmd.SetEnergy(100, _player);
            _target.SetCurrentHpInternal(_target.MaxHp);
            var card = _player.Creature.CombatState!.CreateCard(canonical, _player);
            if (upgraded) CardCmd.Upgrade(card);
            await CardPileCmd.Add(card, PileType.Hand);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var node = NCard.FindOnTable(card)!;
            node.SetPreviewTarget(_target);
            var damage = card.DynamicVars.Values.Single(value => value is DamageVar or CalculatedDamageVar);
            var preview = (int)damage.PreviewValue;
            var state = Power.State.Encoded;
            var rng = _player.RunState.Rng.CombatCardGeneration.Counter;
            for (var read = 0; read < 20; read++) node.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            Check(_player.Creature.GetPower<StrengthPower>()!.Amount == 2 && Power.State.Used == 2,
                $"PREVIEW_pending_compass_readonly_{card.Id.Entry}_{upgraded}");
            Check(Power.State.Encoded == state && _player.RunState.Rng.CombatCardGeneration.Counter == rng
                && !UnboundCompassDamagePreviewPatch.InPreview && (int)damage.PreviewValue == preview,
                $"PREVIEW_pending_compass_repeated_reads_{card.Id.Entry}_{upgraded}");
            await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(card, _target));
            var actual = CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
                .First(entry => entry.CardSource == card && entry.Receiver == _target).Result.TotalDamage;
            GD.Print($"STS2MOD_PREVIEW_COMPASS card={card.Id.Entry} upgraded={upgraded} preview={preview} actual={actual}");
            Check(preview == actual, $"PREVIEW_pending_compass_matches_{card.Id.Entry}_{upgraded}");
        }
        await PowerCmd.Remove<SoarPower>(_target);
        await PowerCmd.Remove<VulnerablePower>(_target);
        var control = await Add<WindBlade>();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var controlNode = NCard.FindOnTable(control)!;
        var strength = _player.Creature.GetPower<StrengthPower>()!.Amount;
        Power.State.ClearCombat();
        Power.State.Reserve(new object(), Element.Wind);
        controlNode.SetPreviewTarget(_target);
        Check(control.DynamicVars.Damage.PreviewValue == 6 + strength, "PREVIEW_pending_compass_same_element_no_bonus");
        Power.State.Reserve(new object(), Element.Rock);
        Power.State.Reserve(new object(), Element.Wind);
        Power.State.Reserve(new object(), Element.Rock);
        Power.State.SetTurnLimit(3);
        controlNode.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
        Check(control.DynamicVars.Damage.PreviewValue == 6 + strength, "PREVIEW_pending_compass_exhausted_no_bonus");
        await FreshExpansion();
    }

    private async Task TestPreviewDamageResolution(CardModel[] cards)
    {
        await FreshExpansion();
        var context = new BlockingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(context, _player.Creature, 2, _player.Creature, null);
        foreach (var canonical in cards.Where(card => card.Type == CardType.Attack && card.TargetType == TargetType.AnyEnemy))
        foreach (var upgraded in new[] { false, true })
        foreach (var soaring in new[] { false, true })
        {
            Power.State.ClearCombat();
            await PowerCmd.Remove<VulnerablePower>(_target);
            await PowerCmd.Remove<WeakPower>(_target);
            await PowerCmd.Remove<PoisonPower>(_target);
            await PowerCmd.Remove<SoarPower>(_target);
            if (soaring)
            {
                await PowerCmd.Apply<SoarPower>(context, _target, 1, _target, null);
                await PowerCmd.Apply<VulnerablePower>(context, _target, 2, _player.Creature, null);
            }
            await PlayerCmd.SetEnergy(100, _player);
            _target.SetCurrentHpInternal(_target.MaxHp);
            var card = _player.Creature.CombatState!.CreateCard(canonical, _player);
            if (upgraded) CardCmd.Upgrade(card);
            await CardPileCmd.Add(card, PileType.Hand);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var node = NCard.FindOnTable(card)!;
            node.SetPreviewTarget(_target);
            var damage = card.DynamicVars.Values.Single(value => value is DamageVar or CalculatedDamageVar);
            var preview = (int)damage.PreviewValue;
            var text = node.GetNode<RichTextLabel>("%DescriptionLabel").Text;
            Check(text.Contains(damage.ToHighlightedString(false)), $"PREVIEW_resolution_text_{card.Id.Entry}_{upgraded}_{soaring}");
            await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(card, _target));
            var actual = CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
                .First(entry => entry.CardSource == card && entry.Receiver == _target).Result.TotalDamage;
            GD.Print($"STS2MOD_PREVIEW_RESOLUTION card={card.Id.Entry} upgraded={upgraded} soaring={soaring} preview={preview} actual={actual}");
            Check(preview == actual, $"PREVIEW_resolution_matches_{card.Id.Entry}_{upgraded}_{soaring}");
        }
    }
}
