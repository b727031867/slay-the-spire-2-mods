using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestRestSiteArt()
    {
        var start = _assertions;
        Check(_player.Character.RestSiteAnimPath == "res://Sts2Mod/character/rest_site.tscn",
            "REST_traveler_uses_custom_scene");
        Check(ModelDb.Character<Ironclad>().RestSiteAnimPath != _player.Character.RestSiteAnimPath,
            "REST_ironclad_resource_unchanged");
        if (CombatManager.Instance.IsInProgress)
        {
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => !CombatManager.Instance.IsInProgress, "combat ends before rest-site fixture");
        }
        await RunManager.Instance.EnterRoomDebug(RoomType.RestSite);
        await Until(() => NRestSiteRoom.Instance?.Characters.Count == 1, "native rest-site room and character");
        var room = NRestSiteRoom.Instance!;
        var character = room.Characters.Single();
        Check(character.Player == _player && room.GetCharacterForPlayer(_player) == character,
            "REST_native_room_mounts_traveler");
        var art = character.GetNode<Sprite2D>("ControlRoot/Art");
        Check(art.Texture.ResourcePath == "res://Sts2Mod/character/rest_site.png"
            && art.Texture.GetWidth() > 0 && art.Texture.GetHeight() > 0, "REST_custom_texture_imported");
        Check(VisualDescendants(character).All(node => node.GetClass() != "SpineSprite"),
            "REST_no_inherited_ironclad_spine");
        Check(art.Texture.GetHeight() * art.Scale.Y is >= 440 and <= 480, "REST_seated_illustration_bounded");
        Check(character.Hitbox.IsInsideTree() && character.GetNodeOrNull("%SelectionReticle") is not null
            && character.GetNodeOrNull("%ThoughtBubbleLeft") is not null
            && character.GetNodeOrNull("%ThoughtBubbleRight") is not null, "REST_native_interaction_nodes_exist");
        Check(room.Options.OfType<HealRestSiteOption>().Count() == 1
            && room.Options.OfType<SmithRestSiteOption>().Count() == 1
            && room.Options.All(option => room.GetButtonForOption(option) is not null), "REST_rest_and_smith_buttons_preserved");
        var root = character.GetNode<Control>("ControlRoot");
        character.FlipX();
        Check(root.Scale.X == -1 && character.Hitbox.GetParent() == root && art.GetParent() == root,
            "REST_native_flip_keeps_art_and_hitbox_together");
        character.FlipX();
        Check(root.Scale.X == 1, "REST_flip_restores_original_pose");
        character.ShowHoveredRestSiteOption(room.Options.First());
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Check(VisualDescendants(character).OfType<NThoughtBubbleVfx>().Any(), "REST_native_option_bubble_works");
        character.ShowHoveredRestSiteOption(null);
        character.HideFlameGlow();
        await StartFreshCombat();
        Check(!GodotObject.IsInstanceValid(room) && !GodotObject.IsInstanceValid(character),
            "REST_room_and_character_free_on_exit");
        GD.Print($"STS2MOD_REST_SITE_COMPLETE assertions={_assertions - start} scene=PASS native_options=PASS cleanup=PASS rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
