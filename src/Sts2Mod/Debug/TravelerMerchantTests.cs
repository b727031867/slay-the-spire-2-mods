using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestMerchantArt()
    {
        var start = _assertions;
        Check(_player.Character.MerchantAnimPath == "res://Sts2Mod/character/merchant.tscn"
            && ModelDb.Character<Ironclad>().MerchantAnimPath != _player.Character.MerchantAnimPath,
            "SHOP_traveler_scene_does_not_replace_ironclad");
        if (CombatManager.Instance.IsInProgress)
        {
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => !CombatManager.Instance.IsInProgress, "combat ends before shop fixture");
        }
        var gold = _player.Gold;
        await RunManager.Instance.EnterRoomDebug(RoomType.Shop);
        await Until(() => NMerchantRoom.Instance?.PlayerVisuals.Count == 1, "native shop character");
        var room = NMerchantRoom.Instance!;
        var character = room.PlayerVisuals.Single();
        var art = character.GetNode<Sprite2D>("%TravelerMerchantArt");
        Check(art.Texture.ResourcePath == "res://Sts2Mod/character/body.png"
            && VisualDescendants(character).All(n => n.GetClass() != "SpineSprite"), "SHOP_traveler_art_no_ironclad_spine");
        Check(art.Scale.X == art.Scale.Y && art.Texture.GetHeight() * art.Scale.Y is >= 530 and <= 550
            && Mathf.Abs(art.Position.Y + art.Texture.GetHeight() * art.Scale.Y / 2) < 1,
            "SHOP_proportional_art_feet_on_floor");
        character.PlayAnimation("relaxed_loop", true);
        character.PlayAnimation("die");
        var ironclad = ResourceLoader.Load<PackedScene>(ModelDb.Character<Ironclad>().MerchantAnimPath)
            .Instantiate<MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantCharacter>();
        Check(!Characters.TravelerMerchantVisualPatch.IsTraveler(ironclad), "SHOP_native_ironclad_animation_not_intercepted");
        ironclad.Free();
        Check(room.Inventory.IsInsideTree() && room.MerchantButton.IsInsideTree()
            && room.ProceedButton.IsInsideTree() && _player.Gold == gold, "SHOP_native_inventory_and_controls_preserved");
        await StartFreshCombat();
        Check(!GodotObject.IsInstanceValid(room) && !GodotObject.IsInstanceValid(character), "SHOP_character_freed_on_exit");
        GD.Print($"STS2MOD_MERCHANT_COMPLETE assertions={_assertions - start} scene=PASS cleanup=PASS rendering=NOT_RUN");
    }
}
