using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Sts2Mod.Cards;
using Sts2Mod.Pools;
using Sts2Mod.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using Sts2Mod.Presentation;

namespace Sts2Mod.Characters;

[CustomID("TC_TRAVELER")]
public sealed class Traveler : PlaceholderCharacterModel
{
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override Color NameColor => new("79D9C8");
    public override int StartingHp => 75;
    public override CardPoolModel CardPool => ModelDb.CardPool<TravelerCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TravelerRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TravelerPotionPool>();
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<TravelerStrike>(), ModelDb.Card<TravelerStrike>(), ModelDb.Card<TravelerStrike>(),
        ModelDb.Card<TravelerDefend>(), ModelDb.Card<TravelerDefend>(), ModelDb.Card<TravelerDefend>(),
        ModelDb.Card<WindBlade>(), ModelDb.Card<RockGuard>(), ModelDb.Card<LightningThrust>(), ModelDb.Card<TailwindPrep>()];
    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<JourneyCompass>()];
    public override string CustomVisualPath => "res://Sts2Mod/character/traveler.tscn";
    public override float DeathAnimTime => 0.6f;
    public override NCreatureVisuals CreateCustomVisuals()
    {
        // BaseLib converts the scene root. Construct mod C# nodes directly: Godot's script
        // registry belongs to the game assembly and cannot resolve mod .cs scene attachments.
        var visuals = ResourceLoader.Load<PackedScene>(CustomVisualPath).Instantiate<NCreatureVisuals>();
        var placeholder = visuals.GetNode<Node2D>("Visuals");
        visuals.RemoveChild(placeholder);
        placeholder.Free();
        var rig = new TravelerRig { Name = "Visuals" };
        visuals.AddChild(rig);
        rig.Owner = visuals;
        rig.UniqueNameInOwner = true;
        return visuals;
    }
    public override string CustomCharacterSelectBg => "res://Sts2Mod/character/select.tscn";
    public override string CustomRestSiteAnimPath => "res://Sts2Mod/character/rest_site.tscn";
    public override string CustomCharacterSelectIconPath => "res://Sts2Mod/character/icon.png";
    public override string CustomCharacterSelectLockedIconPath => CustomCharacterSelectIconPath;
    public override string CustomIconTexturePath => "res://Sts2Mod/character/head_icon.png";
    public override string CustomIconOutlineTexturePath => CustomIconTexturePath;
    public override string CustomMapMarkerPath => CustomCharacterSelectIconPath;
    public override string CustomIconPath => "res://Sts2Mod/character/icon.tscn";
    public override Color MapDrawingColor => NameColor;
    public override Color EnergyLabelOutlineColor => new("254C49");
}
