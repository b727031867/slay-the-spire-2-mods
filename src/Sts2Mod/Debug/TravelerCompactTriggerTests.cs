using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Sts2Mod.Combo;
using Sts2Mod.Presentation;
using Sts2Mod.Relics;
using Sts2Mod.Cards;
using Sts2Mod.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestCompactTriggers()
    {
        await FreshExpansion();
        var host = new Node2D();
        AddChild(host);
        var first = ElementComboVfx.Spawn(host, ComboKind.WindRock, false);
        for (var index = 0; index < 24; index++)
            ElementComboVfx.Spawn(host, (ComboKind)(index % 3 + 1), true);
        Check(host.GetChildren().OfType<ElementComboVfx>().Count() == 1
            && ReferenceEquals(first, host.GetChildren().OfType<ElementComboVfx>().Single()),
            "QUIET_combo_burst_reuses_single_node");
        Check(first.Scale == Vector2.One && first.Modulate.A <= .55f
            && ElementComboVfx.Duration <= .24 && ElementComboVfx.MaxRadius <= 28,
            "QUIET_combo_small_fixed_bounds_short_lifetime");
        Check(!VisualDescendants(host).OfType<TextureRect>().Any()
            && !VisualDescendants(host).OfType<GpuParticles2D>().Any(),
            "QUIET_combo_has_no_images_or_particles");
        var encoded = Power.State.Encoded;
        var rng = _player.RunState.Rng.CombatTargets.Counter;
        await Until(() => !GodotObject.IsInstanceValid(first), "compact combo cleanup", 3);
        Check(Power.State.Encoded == encoded && _player.RunState.Rng.CombatTargets.Counter == rng,
            "QUIET_combo_cleanup_no_rules_or_rng_changes");
        host.QueueFree();

        var relic = await RelicCmd.Obtain<JourneyCompass>(_player);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        var holder = VisualDescendants(GetTree().Root).OfType<NRelicInventoryHolder>()
            .First(h => h.Relic?.Model == relic);
        var icon = holder.Relic.Icon;
        var iconTexture = icon.Texture;
        var iconScale = icon.Scale;
        var iconPosition = icon.Position;
        var flashes = 0;
        void Observe(RelicModel _, IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> __) => flashes++;
        relic.Flashed += Observe;
        try
        {
            encoded = Power.State.Encoded;
            rng = _player.RunState.Rng.CombatTargets.Counter;
            for (var index = 0; index < 24; index++) relic.Flash();
            var pulses = icon.GetChildren().OfType<CompactRelicPulse>().Where(n => !n.IsQueuedForDeletion()).ToArray();
            Check(flashes == 24 && pulses.Length == 1, "QUIET_relic_flash_event_preserved_one_icon_pulse");
            Check(pulses[0].MouseFilter == Control.MouseFilterEnum.Ignore && pulses[0].Size == icon.Size
                && icon.Texture == iconTexture && icon.Scale == iconScale && icon.Position == iconPosition,
                "QUIET_relic_pulse_no_layout_scale_texture_or_input_changes");
            Check(!VisualDescendants(GetTree().Root).OfType<NRelicFlashVfx>().Any(),
                "QUIET_relic_no_creature_portrait_flash");
            foreach (var model in new RelicModel[] { ModelDb.Relic<JourneyCompass>(),
                ModelDb.Relic<UnboundJourneyCompass>(), ModelDb.Relic<PaimonCheer>() })
                Check(NRelicFlashVfx.Create(model) is null, "QUIET_native_portrait_suppressed_" + model.Id.Entry);
            var vanilla = NRelicFlashVfx.Create(ModelDb.Relic<BurningBlood>());
            Check(vanilla is not null && !CompactRelicPulse.Handles(ModelDb.Relic<BurningBlood>()),
                "QUIET_other_relic_factory_unchanged");
            vanilla?.Free();
            await Until(() => pulses.All(p => !GodotObject.IsInstanceValid(p)), "compact relic cleanup", 3);
            Check(Power.State.Encoded == encoded && _player.RunState.Rng.CombatTargets.Counter == rng,
                "QUIET_relic_cleanup_no_rules_or_rng_changes");
        }
        finally { relic.Flashed -= Observe; }
        await RelicCmd.Remove(relic);
        var bubbleCount = VisualDescendants(GetTree().Root).OfType<NThoughtBubbleVfx>().Count();
        var beforeZero = _player.Relics.Count;
        ComboFeedback.ShowReward(_player.Creature, _player.Creature, new Reservation(ComboKind.LightningWind, null, true), 0);
        Check(VisualDescendants(GetTree().Root).OfType<NThoughtBubbleVfx>().Count() == bubbleCount
            && _player.Relics.Count == beforeZero, "QUIET_zero_draw_no_bubble_or_relic_effect");
        await Play<ElementResonance>();
        await Play<ElementResonance>(true);
        // Keep the legacy model's icon regression while new cards share one ordered power.
        await PowerCmd.Apply<FirstElementResonancePower>(ExpansionContext(), _player.Creature, 1, _player.Creature, null);
        foreach (var power in new PowerModel[] { _player.Creature.GetPower<ElementResonancePower>()!,
            _player.Creature.GetPower<FirstElementResonancePower>()! })
        {
            var powerNode = VisualDescendants(GetTree().Root).OfType<NPower>().First(n => n.Model == power);
            var powerIcon = powerNode.GetNode<TextureRect>("%Icon");
            var energy = _player.PlayerCombatState!.Energy;
            encoded = Power.State.Encoded;
            rng = _player.RunState.Rng.CombatTargets.Counter;
            var events = 0;
            void ObservePower(PowerModel _) => events++;
            power.Flashed += ObservePower;
            try
            {
                for (var index = 0; index < 24; index++)
                    HarmonyLib.AccessTools.Method(typeof(PowerModel), "Flash").Invoke(power, null);
                var pulse = powerIcon.GetChildren().OfType<CompactRelicPulse>().Single(p => !p.IsQueuedForDeletion());
                Check(events == 24 && pulse.Size == powerIcon.Size
                    && !powerNode.GetNode<CpuParticles2D>("%PowerFlash").Emitting,
                    "QUIET_resonance_icon_pulse_" + power.Id.Entry);
                Check(NPowerFlashVfx.Create(power) is null
                    && !VisualDescendants(GetTree().Root).OfType<NPowerFlashVfx>().Any(),
                    "QUIET_resonance_no_large_art_" + power.Id.Entry);
                await Until(() => !GodotObject.IsInstanceValid(pulse), "resonance pulse cleanup", 3);
                Check(_player.PlayerCombatState.Energy == energy && Power.State.Encoded == encoded
                    && _player.RunState.Rng.CombatTargets.Counter == rng,
                    "QUIET_resonance_cleanup_no_gameplay_mutation_" + power.Id.Entry);
            }
            finally { power.Flashed -= ObservePower; }
        }
        Check(!CompactRelicPulse.Handles(ModelDb.Power<WindGuidancePower>()), "QUIET_other_power_unchanged");
        GD.Print("STS2MOD_COMPACT_TRIGGERS_COMPLETE compact=PASS cleanup=PASS scope=custom_only rendering=NOT_RUN");
    }
}
