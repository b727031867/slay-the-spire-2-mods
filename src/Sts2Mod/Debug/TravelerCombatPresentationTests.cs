using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Presentation;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestCombatPresentation()
    {
        var start = _assertions;
        var creature = _player.Creature;
        var node = creature.GetCreatureNode()!;
        var rig = CombatPresentation.Rig(creature)!;
        Check(rig is not null && rig.PartCount == 11, "ANIM_native_scene_eleven_separate_parts");
        var animation = node.Visuals.GetNode<AnimationPlayer>("AnimationPlayer");
        rig!.CardActive = false;
        node.SetAnimationTrigger("Attack");
        animation.Advance(0);
        Check(rig.ActionName == "Attack", "ANIM_native_attack_reaches_layered_rig");
        rig.Advance(0.08);
        Check(rig.GetNode<Node2D>("Torso/SwordArm").Rotation != 0.28f
            && rig.GetNode<Node2D>("Torso").Rotation != 0 && rig.Position == Vector2.Zero,
            "ANIM_independent_arm_and_torso_not_whole_image_slide");
        rig.PlayAction("Defend");
        rig.Advance(1);
        Check(rig.ActionName == "Defend", "ANIM_mixed_card_defense_uses_attack_recovery");
        rig.Advance(1);
        Check(rig.ActionName == "Idle" && rig.Position == Vector2.Zero && rig.Rotation == 0,
            "ANIM_pose_returns_without_drift");
        rig.PlayAction("Hit"); rig.Advance(0.05);
        rig.PlayAction("Dead"); rig.Advance(1);
        node.SetAnimationTrigger("Attack"); animation.Advance(0);
        Check(rig.ActionName == "Dead" && rig.Rotation < -1, "ANIM_death_cannot_be_overwritten_by_old_actions");
        node.SetAnimationTrigger("Revive"); animation.Advance(0); rig.Advance(1);
        Check(rig.ActionName == "Idle" && rig.Rotation == 0, "ANIM_native_revive_restores_pose");
        rig.PlayAction("Dead"); rig.Advance(1);
        node.SetAnimationTrigger("revive"); animation.Advance(0); rig.Advance(1);
        Check(rig.ActionName == "Idle" && rig.Rotation == 0, "ANIM_lowercase_revive_alias_restores_pose");

        var state = Power.State.Encoded;
        var rng = _player.RunState.Rng.CombatTargets.Counter;
        var parent = new Node2D(); AddChild(parent);
        var effects = new List<CombatElementVfx>();
        foreach (var element in new[] { Element.Wind, Element.Rock, Element.Lightning })
        foreach (var effect in new[] { CombatEffect.Attack, CombatEffect.Defend })
        {
            var vfx = CombatElementVfx.Spawn(parent, element, effect);
            effects.Add(vfx);
            Check(vfx.Element == element && vfx.Effect == effect && vfx.IsInsideTree(), "ANIM_six_effect_node_" + element + "_" + effect);
        }
        for (var i = 0; i < 20; i++) CombatElementVfx.Spawn(parent, Element.None, CombatEffect.Contact);
        Check(parent.GetChildren().OfType<CombatElementVfx>().Count(v => !v.IsQueuedForDeletion()) == CombatElementVfx.MaxEffectsPerAnchor,
            "ANIM_simultaneous_effects_bounded");
        await Until(() => parent.GetChildCount() == 0, "combat effect cleanup", 5);
        Check(effects.All(v => !GodotObject.IsInstanceValid(v)) && Power.State.Encoded == state
            && _player.RunState.Rng.CombatTargets.Counter == rng, "ANIM_effects_free_without_combo_or_rng_changes");
        parent.QueueFree();

        await NextTurn();
        var impacts = rig.ImpactEvents;
        await Play<TravelerStrike>();
        Check(rig.ImpactEvents > impacts && rig.LastImpactElement == Element.None, "ANIM_native_plain_strike_contact");
        var defenses = rig.DefenseEvents;
        await Play<TravelerDefend>();
        Check(rig.DefenseEvents > defenses && rig.LastDefenseElement == Element.None, "ANIM_native_plain_defend_gain");
        foreach (var type in new[] { typeof(WindBlade), typeof(ShatteringRock), typeof(LightningThrust) })
        {
            Power.State.ClearCombat();
            var card = creature.CombatState!.CreateCard((CardModel)ModelDb.GetById<CardModel>(ModelDb.GetId(type)), _player);
            await CardPileCmd.Add(card, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand);
            impacts = rig.ImpactEvents;
            await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(card, _target));
            Check(rig.ImpactEvents > impacts && rig.LastImpactElement == ((ElementCardModel)card).CardElement,
                "ANIM_native_element_attack_" + ((ElementCardModel)card).CardElement);
        }
        foreach (var type in new[] { typeof(WindShelter), typeof(RockGuard), typeof(ThunderPrep) })
        {
            Power.State.ClearCombat();
            var card = creature.CombatState!.CreateCard((CardModel)ModelDb.GetById<CardModel>(ModelDb.GetId(type)), _player);
            await CardPileCmd.Add(card, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand);
            defenses = rig.DefenseEvents;
            await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(card, null));
            Check(rig.DefenseEvents > defenses && rig.LastDefenseElement == ((ElementCardModel)card).CardElement,
                "ANIM_native_element_defend_" + ((ElementCardModel)card).CardElement);
        }
        Power.State.ClearCombat();
        impacts = rig.ImpactEvents; defenses = rig.DefenseEvents;
        await Play<RockAdvance>();
        Check(rig.ImpactEvents > impacts && rig.DefenseEvents > defenses, "ANIM_native_mixed_card_both_events");

        await CreatureCmd.LoseBlock(creature, creature.Block);
        await CreatureCmd.GainBlock(creature, 10, ValueProp.Unpowered, null, true);
        var context = new BlockingPlayerChoiceContext();
        await CreatureCmd.Damage(context, creature, 4, ValueProp.Move | ValueProp.Unpowered, _target);
        Check(rig.LastReaction == "BlockHit", "ANIM_native_fully_blocked_reaction");
        await CreatureCmd.Damage(context, creature, 9, ValueProp.Move | ValueProp.Unpowered, _target);
        Check(rig.LastReaction == "PartialHit", "ANIM_native_partially_blocked_reaction");
        await CreatureCmd.Damage(context, creature, 2, ValueProp.Move | ValueProp.Unpowered, _target);
        Check(rig.LastReaction == "Hit", "ANIM_native_unblocked_reaction");
        var reactions = rig.ReactionEvents;
        await CreatureCmd.Damage(context, creature, 0, ValueProp.Move | ValueProp.Unpowered, _target);
        Check(rig.ReactionEvents == reactions, "ANIM_zero_damage_has_no_hurt_pose");
        await CreatureCmd.Damage(context, creature, 1, ValueProp.Unblockable | ValueProp.Unpowered, dealer: null, cardSource: null);
        Check(rig.LastReaction == "LifeLoss", "ANIM_non_attack_hp_loss_has_no_weapon_contact");
        rig.Advance(1);
        Check(rig.ActionName == "Idle" && rig.Position == Vector2.Zero, "ANIM_repeated_hits_restore_pose");
        GD.Print($"STS2MOD_COMBAT_PRESENTATION_COMPLETE assertions={_assertions - start} native_events=PASS cleanup=PASS rendering=NOT_RUN multiplayer=NOT_RUN");
        await NextTurn();
    }
}
