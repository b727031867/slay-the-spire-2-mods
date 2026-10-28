using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Sts2Mod.Pools;
using Sts2Mod.Characters;
using Sts2Mod.Combo;
using Sts2Mod.Presentation;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestEnergyIcons()
    {
        const string glyph = "res://Sts2Mod/ui/energy/glyph.svg";
        const string nativeGlyph = "res://images/packed/sprite_fonts/ironclad_energy_icon.png";
        const string expected = "res://Sts2Mod/ui/energy/hover.svg";
        foreach (var pool in new IPoolModel[] { ModelDb.CardPool<TravelerCardPool>(),
            ModelDb.RelicPool<TravelerRelicPool>(), ModelDb.PotionPool<TravelerPotionPool>(),
            ModelDb.CardPool<TravelerEventCardPool>(), ModelDb.RelicPool<TravelerEventRelicPool>() })
            Check(EnergyIconHelper.GetPath((AbstractModel)pool) == expected,
                "ENERGY_pool_" + pool.GetType().Name);
        var textTexture = ResourceLoader.Load<Texture2D>(glyph);
        Check(textTexture.GetSize() == new Vector2(24, 24),
            "ENERGY_native_inline_glyph_bounded");

        // Reproduce a native energy relic acquired by Traveler, not a custom relic artwork preview.
        await RelicCmd.Obtain<Candelabra>(_player);
        var relic = _player.Relics.OfType<Candelabra>().Single();
        Check(relic.HoverTip.Description.Contains(glyph) && !relic.HoverTip.Description.Contains("compass.png"),
            "ENERGY_acquired_native_relic_inline");
        // Native colorless descriptions fall back to the current run's character even without an owner.
        Check(ModelDb.Relic<Candelabra>().HoverTip.Description.Contains(glyph), "ENERGY_unowned_native_relic_inline");
        Check(ModelDb.Relic<Lantern>().HoverTip.Description.Contains(glyph), "ENERGY_other_native_relic_inline");
        var nativeText = new MegaCrit.Sts2.Core.Localization.LocString("relics", ModelDb.Relic<Lantern>().Id.Entry + ".description");
        nativeText.Add("energyPrefix", "ironclad");
        Check(nativeText.GetFormattedText().Contains(nativeGlyph) && EnergyIconHelper.GetPath("ironclad") != expected,
            "ENERGY_explicit_native_prefix_preserved");
        var potion = ModelDb.Potion<EnergyPotion>().ToMutable();
        potion.Owner = _player;
        Check(potion.HoverTip.Description.Contains(glyph), "ENERGY_owned_potion_inline");
        var card = await Add<Offering>();
        Check(card.GetDescriptionForPile(PileType.Hand).Contains(glyph), "ENERGY_card_inline");
        var tip = (HoverTip)HoverTipFactory.ForEnergy(relic);
        Check(tip.Icon?.ResourcePath == expected && tip.Icon.GetSize() == new Vector2(74, 74),
            "ENERGY_native_hover_icon_bounded");
        var owner = new Control();
        NCombatRoom.Instance!.Ui.AddChild(owner);
        try
        {
            var set = NHoverTipSet.CreateAndShow(owner, relic.HoverTips);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var dimensions = set?.GetNode<Control>("textHoverTipContainer").Size ?? Vector2.Zero;
            GD.Print($"STS2MOD_ENERGY_HOVER dimensions={dimensions} icon={tip.Icon!.GetSize()} glyph={textTexture.GetSize()}");
            Check(dimensions.X is > 0 and <= 600 && dimensions.Y is > 0 and <= 700,
                "ENERGY_native_hover_layout_bounded");
        }
        finally
        {
            NHoverTipSet.Remove(owner);
            owner.QueueFree();
            await RelicCmd.Remove(relic);
        }
        var counter = VisualDescendants(NCombatRoom.Instance!.Ui).OfType<NEnergyCounter>().Single();
        Check(ModelDb.Character<Traveler>().EnergyCounterPath == "res://Sts2Mod/ui/energy/counter.tscn"
            && counter.GetNode<TextureRect>("Layers/Core").Texture.ResourcePath == "res://Sts2Mod/ui/energy/orb.svg"
            && counter.GetNode<TextureRect>("%RotationLayers/Orbit").Texture.ResourcePath == "res://Sts2Mod/ui/energy/ring.svg",
            "ENERGY_custom_counter_native_scene");
        var energyBefore = _player.PlayerCombatState!.Energy;
        await PlayerCmd.SetEnergy(0, _player);
        Check(counter.GetNode<Control>("%Layers").Modulate == Colors.DarkGray, "ENERGY_counter_zero_darkens");
        await PlayerCmd.SetEnergy(energyBefore, _player);
        Check(counter.GetNode<Control>("%Layers").Modulate == Colors.White, "ENERGY_counter_gain_restores");
        var orbit = counter.GetNode<Control>("%RotationLayers/Orbit");
        var angle = orbit.Rotation;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Check(orbit.Rotation != angle && orbit.PivotOffset == new Vector2(64, 64), "ENERGY_counter_rotates_about_center");
        TestElementAudio();
        GD.Print("STS2MOD_ENERGY_ICONS_COMPLETE pools=PASS inline=PASS native_hover=PASS rendering=NOT_RUN");
    }

    private void TestElementAudio()
    {
        foreach (var element in new[] { Element.Wind, Element.Rock, Element.Lightning })
        {
            var path = ElementAudio.ElementPath(element)!;
            Check(path.EndsWith(element.ToString().ToLowerInvariant() + ".wav"), "AUDIO_mapping_" + element);
            var stream = ResourceLoader.Load<AudioStreamWav>(path);
            Check(stream is { Stereo: false, MixRate: 44100 } && stream.GetLength() is >= 0.25 and <= 0.6,
                "AUDIO_resource_" + element);
        }
        Check(ElementAudio.ElementPath(Element.None) is null && !ElementAudio.Play(null), "AUDIO_none_silent");
        var combo = ResourceLoader.Load<AudioStreamWav>(ElementAudio.ComboPath);
        Check(combo is { Stereo: false, MixRate: 44100 } && combo.GetLength() is >= 0.5 and <= 0.6, "AUDIO_resource_combo");
        Check(ElementAudio.IsAudible(1, 1) && !ElementAudio.IsAudible(0, 1)
            && !ElementAudio.IsAudible(1, 0), "AUDIO_volume_mute");
        Check(DisplayServer.GetName() == "headless" && !ElementAudio.Play(ElementAudio.ComboPath), "AUDIO_headless_silent");
        GD.Print("STS2MOD_ELEMENT_AUDIO_COMPLETE resources=PASS headless=SILENT listening=NOT_RUN");
    }
}
