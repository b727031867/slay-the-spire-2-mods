using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Powers.Mocks;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Cards;
using Sts2Mod.Characters;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestTravelerSelection()
    {
        var model = ModelDb.Character<Traveler>();
        Check(ModelDb.AllCharacters.Count(character => character is Traveler) == 1, "CHAR_registered_once");
        Check(model.Title.GetFormattedText() is "旅行者" or "Traveler", "CHAR_localized_name");
        foreach (var path in model.AssetPaths.Concat(model.AssetPathsCharacterSelect).Distinct())
            Check(ResourceLoader.Exists(path), "CHAR_resource_" + System.IO.Path.GetFileName(path));
        Check(model.CharacterSelectIcon is not null && model.MapMarker is not null && model.IconTexture is not null,
            "CHAR_select_map_top_icons_load");
        var screen = NCharacterSelectScreen.Create() ?? throw new InvalidOperationException("Native selection screen unavailable");
        GetTree().Root.AddChild(screen);
        screen.InitializeSingleplayer();
        var button = screen.FindChildren("*", recursive: true, owned: false).OfType<NCharacterSelectButton>()
            .Single(entry => entry.Character is Traveler);
        Check(!button.IsLocked && button.Visible, "CHAR_native_selection_button_unlocked");
        screen.SelectCharacter(button, model);
        Check(screen.GetNode<Control>("AnimatedBg").GetChildren().Any(child => child.Name == "TC_TRAVELER_bg"),
            "CHAR_native_selection_loads_background_and_starter_info");
        await TestLobbyPlayerLayout(screen);
        screen.OnSubmenuClosed();
        screen.QueueFree();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private async Task TestTravelerCharacter()
    {
        var start = _assertions;
        Check(_player.Character is Traveler && _player.Character.StartingHp == 75 && _player.Character.MaxEnergy == 3
            && _player.Character.StartingGold == 99, "CHAR_starting_character_and_stats");
        Check(_player.Deck.Cards.Count == 10 && _player.Deck.Cards.OfType<TravelerStrike>().Count() == 3
            && _player.Deck.Cards.OfType<TravelerDefend>().Count() == 3
            && _player.Deck.Cards.OfType<WindBlade>().Count() == 1 && _player.Deck.Cards.OfType<RockGuard>().Count() == 1
            && _player.Deck.Cards.OfType<LightningThrust>().Count() == 1 && _player.Deck.Cards.OfType<TailwindPrep>().Count() == 1,
            "CHAR_exact_ten_card_starter");
        Check(_player.Relics.Count == 1 && _player.Relics.Single() is JourneyCompass, "CHAR_compass_only_starting_relic");
        var pool = _player.Character.CardPool.GetUnlockedCards(_player.UnlockState, 0).ToArray();
        Check(pool.Length == 29 && ThemeTypes.All(type => pool.Count(card => card.GetType() == type) == 1)
            && pool.All(card => card.Pool is TravelerCardPool), "CHAR_twenty_nine_models_twenty_seven_reward_cards_in_exclusive_pool");
        Check(!ModelDb.CardPool<IroncladCardPool>().AllCards.Any(IsTheme)
            && !ModelDb.CardPool<SilentCardPool>().AllCards.Any(IsTheme)
            && !ModelDb.Character<Ironclad>().StartingDeck.Any(IsTheme), "CHAR_ironclad_and_silent_no_theme_cards");
        Check(!ModelDb.RelicPool<SharedRelicPool>().AllRelics.Any(relic => relic is PaimonCheer or JourneyCompass)
            && !ModelDb.RelicPool<IroncladRelicPool>().AllRelics.Any(relic => relic is PaimonCheer or JourneyCompass)
            && ModelDb.RelicPool<TravelerRelicPool>().AllRelics.Count(relic => relic is PaimonCheer) == 1,
            "CHAR_paimon_and_compass_exclusive_not_shared");
        foreach (var basic in new CardModel[] { ModelDb.Card<TravelerStrike>(), ModelDb.Card<TravelerDefend>() })
        {
            var mutable = basic.ToMutable();
            mutable.UpgradeInternal(); mutable.FinalizeUpgradeInternal();
            var loaded = CardModel.FromSerializable(mutable.ToSerializable());
            Check(loaded.GetType() == basic.GetType() && loaded.CurrentUpgradeLevel == 1
                && (loaded is TravelerStrike ? loaded.DynamicVars.Damage.BaseValue == 9 : loaded.DynamicVars.Block.BaseValue == 8),
                "CHAR_basic_roundtrip_" + basic.Id.Entry);
        }
        foreach (var type in new[] { RoomType.Monster, RoomType.Elite, RoomType.Boss })
        {
            var rewards = CardFactory.CreateForReward(_player, 3, CardCreationOptions.ForRoom(_player, type)).ToArray();
            Check(rewards.Length == 3 && rewards.All(reward => IsTheme(reward.Card)), "CHAR_native_reward_pool_" + type);
        }
        for (var i = 0; i < 5; i++)
        {
            var shop = MerchantInventory.CreateForNormalMerchant(_player);
            Check(shop.CharacterCardEntries.Count == 5 && shop.CharacterCardEntries.All(entry => IsTheme(entry.CreationResult!.Card))
                && shop.ColorlessCardEntries.Count == 2 && shop.PotionEntries.Count == 3, "CHAR_native_shop_five_slots_and_shared_potions_" + i);
        }
        var original = _player.Deck.Cards.OfType<TravelerStrike>().First();
        Check(Enumerable.Range(0, 20).All(_ => IsTheme(CardFactory.CreateRandomCardForTransform(original, false, _player.PlayerRng.Shops))),
            "CHAR_basic_transform_stays_in_exclusive_pool");

        await TestStarterCompassBalance();
        await TestCompassUpgrade();
        await NextTurn();
        await TestRestSiteArt();
        await TestMerchantArt();
        GD.Print($"STS2MOD_CHARACTER_CASES_COMPLETE assertions={_assertions - start} starter=10 exclusive_cards=27 starter_relic=1 rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
