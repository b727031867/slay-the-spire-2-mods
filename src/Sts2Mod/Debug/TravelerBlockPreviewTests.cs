using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Sts2Mod.Cards;
using Sts2Mod.Pools;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestGoopyBlockPreview()
    {
        var cards = ModelDb.CardPool<TravelerCardPool>().AllCards
            .Where(card => card.DynamicVars.Values.Any(value => value is BlockVar)).ToArray();
        foreach (var language in new[] { "eng", "zhs" })
        {
            using var json = System.Text.Json.JsonDocument.Parse(Godot.FileAccess.GetFileAsString(
                $"res://Sts2Mod/localization/{language}/cards.json"));
            Check(cards.All(card => json.RootElement.GetProperty(card.Id.Entry + ".description").GetString()!
                .Contains("{Block:diff()}")), "GOOPY_block_format_" + language);
        }
        foreach (var upgraded in new[] { false, true })
        {
            await FreshExpansion();
            var card = await Add<TravelerDefend>(upgraded);
            Check(CardCmd.Enchant<Goopy>(card, 1) is not null, "GOOPY_native_enchantment_" + upgraded);
            for (var played = 0; played < 3; played++)
            {
                if (played > 0) await CardPileCmd.Add(card, PileType.Hand);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                var node = NCard.FindOnTable(card)!;
                node.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
                var expected = (upgraded ? 8 : 5) + played;
                Check(card.DynamicVars.Block.PreviewValue == expected && node.GetNode<RichTextLabel>("%DescriptionLabel").Text
                    .Contains(card.DynamicVars.Block.ToHighlightedString(false)), $"GOOPY_preview_{upgraded}_{played}");
                var before = _player.Creature.Block;
                await Queue(new PlayCardAction(card, null));
                Check(_player.Creature.Block - before == expected && card.Enchantment!.Amount == played + 2
                    && card.Pile?.Type == PileType.Exhaust, $"GOOPY_actual_block_and_growth_{upgraded}_{played}");
            }
            var restored = CardModel.FromSerializable(card.ToSerializable());
            Check(restored.Enchantment is Goopy { Amount: 4 }
                && restored.DynamicVars.Block.BaseValue == (upgraded ? 8 : 5), "GOOPY_serialized_growth_" + upgraded);
        }
        await FreshExpansion();
        GD.Print("STS2MOD_GOOPY_COMPLETE preview=PASS actual_block=PASS growth=PASS rendering=NOT_RUN");
    }
}
