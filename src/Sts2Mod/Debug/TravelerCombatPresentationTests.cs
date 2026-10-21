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
        Check(rig is not null && rig.PartCount == 7 && rig.Skin.Bones.Count == 18, "ANIM_native_scene_single_mother_connected_skin");
        rig!.ResetForPreview();
        Check(rig.Skin.Layers.All(layer => layer.Texture.ResourcePath == TravelerSkin.TexturePath)
            && rig.Skin.Vertices.Zip(rig.Skin.Uv).All(p => p.First.DistanceTo(p.Second) < .01f),
            "ANIM_bind_pose_reconstructs_original_mother_without_rescaling_parts");
        Check(rig.Skin.Triangles.SelectMany(t => t).Select(t => string.Join(',',t)).Distinct().Count()
            == rig.Skin.Triangles.Sum(t => t.Count), "ANIM_source_triangles_drawn_once_no_duplicate_shoulders");
        rig.PlayAction("Defend");
        for (var i = 0; i < 20; i++) rig.Advance(.01 / CombatPresentation.Speed);
        Check(new[] { "LeftFoot", "RightFoot" }.All(name => rig.Skin.BonePosition(name).DistanceTo(rig.Skin.Bind[name]) < .05f)
            && rig.Skin.Bones["LeftShin"].Rotation != 0, "ANIM_guard_bends_knees_with_planted_feet");
        rig.Advance(.10 / CombatPresentation.Speed);
        var hand=rig.Skin.Bones["Hand"]; var support=rig.Skin.Bones["FreeHand"];
        var palmGap=rig.Skin.Canvas.ToLocal(support.ToGlobal(new Vector2(0,40))).DistanceTo(rig.Skin.BonePosition("Hand"));
        var blade=hand.GlobalTransform.X.Normalized().Rotated(2.18f);
        Check(palmGap<20 && blade.Y<-.95f, "ANIM_guard_two_palms_share_hilt_blade_upright_in_front");
        var before = rig.FrameVertices();
        rig.PlayAction("Hit");
        Check(before.Zip(rig.FrameVertices()).Max(p => Math.Abs(p.First - p.Second)) < .01,
            "ANIM_interrupt_blends_from_current_pose_no_teleport");
        rig.ResetForPreview(); rig.PlayAction("Attack"); rig.Advance(.02);
        var elapsed = rig.Elapsed; before = rig.FrameVertices(); rig.Contact();
        Check(rig.Elapsed == elapsed && before.SequenceEqual(rig.FrameVertices()), "ANIM_contact_does_not_jump_animation_clock");
        rig.Advance(.16 / CombatPresentation.Speed);
        var offset = rig.Skin.Uv.Length-TravelerSkin.Weapon.Length;
        Check(Enumerable.Range(1,TravelerSkin.Weapon.Length-1).All(i =>
            Math.Abs(rig.Skin.Vertices[offset+i].DistanceTo(rig.Skin.Vertices[offset])
                -TravelerSkin.Weapon[i].DistanceTo(TravelerSkin.Weapon[0])) < .02f),
            "ANIM_hand_and_sword_keep_rigid_shape_during_swing");
        rig.ResetForPreview(); rig.PlayAction("Attack");
        for(var i=0;i<30;i++)rig.Advance(.01 / CombatPresentation.Speed);
        blade=rig.Skin.Bones["Hand"].GlobalTransform.X.Normalized().Rotated(2.18f);
        Check(blade.X>.98f && Math.Abs(blade.Y)<.12f && rig.Skin.Bones["Torso"].Scale.X<.9f
            && rig.Skin.BonePosition("Hand").X>900, "ANIM_side_on_forward_thrust_has_horizontal_blade");
        before=rig.FrameVertices(); rig.PlayAction("Defend");
        Check(rig.ActionName=="Defend" && before.Zip(rig.FrameVertices()).Max(p=>Math.Abs(p.First-p.Second))<.01,
            "ANIM_thrust_to_guard_switch_keeps_current_pose");
        for(var i=0;i<12;i++)rig.Advance(.01 / CombatPresentation.Speed);
        before=rig.FrameVertices(); rig.PlayAction("Attack");
        Check(rig.ActionName=="Attack" && before.Zip(rig.FrameVertices()).Max(p=>Math.Abs(p.First-p.Second))<.01,
            "ANIM_guard_to_thrust_reentry_has_no_idle_reset");
        rig.ResetForPreview(); rig.PlayAction("Hit");
        for(var i=0;i<18;i++)rig.Advance(.01 / CombatPresentation.Speed);
        Check(rig.Skin.Bones["Torso"].Rotation<-.19f && rig.Skin.Bones["Torso"].Scale.X<.92f
            && new[]{"LeftFoot","RightFoot"}.All(n=>rig.Skin.BonePosition(n).DistanceTo(rig.Skin.Bind[n])<.05),
            "ANIM_hurt_side_on_leans_back_with_supported_feet");
        ExportNativeMotion(rig);
        rig.ResetForPreview();
        var animation = node.Visuals.GetNode<AnimationPlayer>("AnimationPlayer");
        rig!.CardActive = false;
        node.SetAnimationTrigger("Attack");
        animation.Advance(0);
        Check(rig.ActionName == "Attack", "ANIM_native_attack_reaches_layered_rig");
        rig.Advance(0.08);
        Check(rig.Skin.Bones["SwordArm"].Rotation != 0
            && rig.Skin.Bones["Torso"].Rotation != 0 && rig.Position == Vector2.Zero,
            "ANIM_independent_arm_and_torso_not_whole_image_slide");
        rig.CardActive=true; rig.CardAttacked=true; rig.PlayAction("Defend");
        rig.Advance(1);
        Check(rig.ActionName == "Defend", "ANIM_mixed_card_defense_uses_attack_recovery");
        rig.CardActive=false;
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
            await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(card,
                card.TargetType == MegaCrit.Sts2.Core.Entities.Cards.TargetType.AnyEnemy ? _target : null));
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

    // The review player uses these actual native mesh frames, not a second JS animation implementation.
    private static void ExportNativeMotion(TravelerRig rig)
    {
        var path = System.Environment.GetEnvironmentVariable("STS2MOD_MOTION_EXPORT");
        if (string.IsNullOrWhiteSpace(path)) return;
        var clips = new List<object>();
        foreach (var name in new[] { "Bind", "Attack", "Defend", "Hit", "Continuous", "Interrupted" })
        {
            rig.ResetForPreview();
            var frames = new List<float[]>();
            var boneFrames = new List<float[]>();
            var count = name == "Continuous" ? 90 : 42;
            for (var frame = 0; frame < count; frame++)
            {
                if (frame == 0 && name != "Bind") rig.PlayAction(name is "Attack" or "Defend" or "Hit" ? name : "Attack");
                if (name == "Continuous" && frame is 10 or 31 or 52) rig.PlayAction("Defend");
                if (name == "Continuous" && frame is 19 or 40) rig.PlayAction("Attack");
                if (name == "Interrupted" && frame == 6) rig.PlayAction("Hit");
                if (name == "Interrupted" && frame == 14) rig.PlayAction("Defend");
                frames.Add(rig.FrameVertices().Select(v => MathF.Round(v,3)).ToArray());
                boneFrames.Add(rig.Skin.Bones.Keys.SelectMany(key => {
                    var p = (rig.Skin.BonePosition(key)-TravelerSkin.Origin)*TravelerRig.RigScale;
                    return new[] { MathF.Round(p.X,3),MathF.Round(p.Y,3) };
                }).ToArray());
                if (name != "Bind") rig.Advance(1.0 / 30 / CombatPresentation.Speed);
            }
            clips.Add(new { name, fps = 30, frames, boneFrames });
        }
        var layers = rig.Skin.Layers.Select((layer,index) => new { z = layer.ZIndex, mask = index != 6, triangles = rig.Skin.Triangles[index] });
        System.IO.File.WriteAllText(path,System.Text.Json.JsonSerializer.Serialize(new {
            texture = TravelerSkin.TexturePath, bones = rig.Skin.Bones.Keys,
            weapon = TravelerSkin.Weapon.SelectMany(p => new[] { p.X,p.Y }),
            parents = rig.Skin.Bones.Values.Select(b => Array.IndexOf(rig.Skin.Bones.Keys.ToArray(),b.GetParent().Name.ToString())),
            uv = rig.Skin.Uv.SelectMany(p => new[] { p.X,p.Y }), layers, clips }));
        rig.ResetForPreview();
        GD.Print("STS2MOD_MOTION_EXPORT_COMPLETE native_mesh_frames=PASS preview_rendering=OFFLINE");
    }
}
