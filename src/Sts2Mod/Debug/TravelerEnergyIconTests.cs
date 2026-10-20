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

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestEnergyIcons()
    {
        const string glyph = "res://images/packed/sprite_fonts/ironclad_energy_icon.png";
        var expected = EnergyIconHelper.GetPath("ironclad");
        foreach (var pool in new IPoolModel[] { ModelDb.CardPool<TravelerCardPool>(),
            ModelDb.RelicPool<TravelerRelicPool>(), ModelDb.PotionPool<TravelerPotionPool>() })
            Check(pool.EnergyColorName == "ironclad" && EnergyIconHelper.GetPath((AbstractModel)pool) == expected,
                "ENERGY_pool_" + pool.GetType().Name);
        var textTexture = ResourceLoader.Load<Texture2D>(glyph);
        Check(textTexture.GetWidth() is > 0 and <= 64 && textTexture.GetHeight() is > 0 and <= 64,
            "ENERGY_native_inline_glyph_bounded");

        // Reproduce a native energy relic acquired by Traveler, not a custom relic artwork preview.
        await RelicCmd.Obtain<Candelabra>(_player);
        var relic = _player.Relics.OfType<Candelabra>().Single();
        Check(relic.HoverTip.Description.Contains(glyph) && !relic.HoverTip.Description.Contains("compass.png"),
            "ENERGY_acquired_native_relic_inline");
        Check(ModelDb.Relic<Candelabra>().HoverTip.Description.Contains(glyph), "ENERGY_unowned_native_relic_inline");
        Check(ModelDb.Relic<Lantern>().HoverTip.Description.Contains(glyph), "ENERGY_other_native_relic_inline");
        var potion = ModelDb.Potion<EnergyPotion>().ToMutable();
        potion.Owner = _player;
        Check(potion.HoverTip.Description.Contains(glyph), "ENERGY_owned_potion_inline");
        var card = await Add<Offering>();
        Check(card.GetDescriptionForPile(PileType.Hand).Contains(glyph), "ENERGY_card_inline");
        var tip = (HoverTip)HoverTipFactory.ForEnergy(relic);
        Check(tip.Icon?.ResourcePath == expected && tip.Icon.GetWidth() <= 128 && tip.Icon.GetHeight() <= 128,
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
        GD.Print("STS2MOD_ENERGY_ICONS_COMPLETE pools=PASS inline=PASS native_hover=PASS rendering=NOT_RUN");
    }
}
