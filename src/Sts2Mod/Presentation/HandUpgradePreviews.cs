using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Sts2Mod.Presentation;

// SimpleSelect owns the real card nodes; only their presentation may be replaced.
internal sealed class HandUpgradePreviews : IDisposable
{
    internal const string PreviewName = "JourneyPlansUpgradePreview";
    private readonly NSelectedHandCardContainer? _container;
    private readonly SceneTree? _tree;
    private readonly Dictionary<NCard, NCard> _previews = new();
    private readonly Dictionary<Control, (Control.MouseFilterEnum Mouse, Control.FocusModeEnum Focus)> _input = new();

    public HandUpgradePreviews(Player player)
    {
        var hand = NCombatRoom.Instance?.Ui.Hand;
        if (hand == null) return;
        _container = hand.GetNode<NSelectedHandCardContainer>("%SelectedHandCardContainer");
        _tree = hand.GetTree();
        _tree.ProcessFrame += Refresh;
        Player = player;
    }

    private Player? Player { get; }

    private void Refresh()
    {
        if (!GodotObject.IsInstanceValid(_container) || !_container!.IsInsideTree()) { Dispose(); return; }
        var selected = _container!.Holders.Select(holder => holder.CardNode)
            .OfType<NCard>()
            .Where(card => card.Model?.Owner == Player && card.Model!.IsUpgradable).ToArray();
        foreach (var card in _previews.Keys.Except(selected).ToArray()) Remove(card);
        foreach (var card in selected)
        {
            if (_previews.ContainsKey(card)) continue;
            var clone = card.Model!.CardScope!.CloneCard(card.Model);
            clone.UpgradeInternal();
            clone.UpgradePreviewType = CardUpgradePreviewType.Combat;
            var preview = NCard.Create(clone)!;
            preview.Name = PreviewName;
            card.AddChild(preview);
            preview.Position = Vector2.Zero;
            foreach (var control in preview.FindChildren("*", recursive: true, owned: false).OfType<Control>().Prepend(preview))
            {
                _input.Add(control, (control.MouseFilter, control.FocusMode));
                control.MouseFilter = Control.MouseFilterEnum.Ignore;
                control.FocusMode = Control.FocusModeEnum.None;
            }
            preview.ShowUpgradePreview();
            card.TreeExiting += preview.Hide;
            _previews.Add(card, preview);
        }
    }

    private void Remove(NCard card)
    {
        var preview = _previews[card];
        _previews.Remove(card);
        if (!GodotObject.IsInstanceValid(preview)) return;
        if (GodotObject.IsInstanceValid(card)) card.TreeExiting -= preview.Hide;
        foreach (var control in preview.FindChildren("*", recursive: true, owned: false).OfType<Control>().Prepend(preview))
        {
            if (!_input.Remove(control, out var input)) continue;
            control.MouseFilter = input.Mouse;
            control.FocusMode = input.Focus;
        }
        preview.GetParent()?.RemoveChild(preview);
        preview.QueueFreeSafely();
    }

    public void Dispose()
    {
        if (GodotObject.IsInstanceValid(_tree)) _tree!.ProcessFrame -= Refresh;
        foreach (var card in _previews.Keys.ToArray()) Remove(card);
        _input.Clear();
    }
}
