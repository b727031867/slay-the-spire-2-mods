using Godot;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Sts2Mod.Pools;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestPortraitFraming()
    {
        for (var number = 19; number <= 28; number++)
        {
            var canonical = ModelDb.CardPool<TravelerCardPool>().AllCards.Single(c => c.Id.Entry == $"TC_C{number}");
            var card = _player.Creature.CombatState!.CreateCard(canonical, _player);
            await CardPileCmd.Add(card, PileType.Hand);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            var portrait = NCard.FindOnTable(card)!.GetNode<TextureRect>("%Portrait");
            var texture = portrait.Texture as AtlasTexture;
            Check(((CustomCardModel)card).CustomPortraitPath == $"res://Sts2Mod/art/TC_C{number}.tres"
                && texture is not null && texture.Atlas.ResourcePath == $"res://Sts2Mod/art/TC_C{number}.png"
                && texture.Region == new Rect2(12, 388, 1000, 760)
                && texture.GetSize() == new Vector2(1000, 760)
                && portrait.StretchMode == TextureRect.StretchModeEnum.KeepAspectCentered
                && texture.GetSize() == portrait.Size * 4,
                "PORTRAIT_fills_native_frame_TC_C" + number);
            await CardPileCmd.RemoveFromCombat([card]);
        }
        GD.Print("STS2MOD_PORTRAIT_COMPLETE cards=10 ratio=25:19 source=unchanged rendering=NOT_RUN");
    }
}
