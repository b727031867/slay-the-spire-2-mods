using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;
using Sts2Mod.Characters;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestLobbyPlayerLayout(NCharacterSelectScreen screen)
    {
        var panel=screen.FindChildren("*",recursive:true,owned:false).OfType<NRemoteLobbyPlayerContainer>().Single();
        var list=panel.GetNode<Container>("Container");
        panel.Show(); list.Show();
        var rows=new List<NRemoteLobbyPlayer>();
        foreach(var (character,index) in new CharacterModel[]{ModelDb.Character<Traveler>(),ModelDb.Character<Traveler>(),
            ModelDb.Character<Ironclad>(),ModelDb.Character<Traveler>()}.Select((c,i)=>(c,i)))
        {
            var row=NRemoteLobbyPlayer.Create(new LobbyPlayer{id=(ulong)(index+1),slotId=index,
                character=ModelDb.Character<Ironclad>(),isReady=index==1},PlatformType.None,false);
            list.AddChild(row); list.MoveChild(row,index); rows.Add(row);
        }
        // Lobby notifications may arrive before FlowContainer's deferred first layout.
        for(var i=0;i<rows.Count;i++)
            rows[i].OnPlayerChanged(new LobbyPlayer{id=(ulong)(i+1),slotId=i,
                character=i==2?ModelDb.Character<Ironclad>():ModelDb.Character<Traveler>(),isReady=i==1});
        rows[0].GetNode<MegaLabel>("%NameplateLabel").SetTextAutoSize("旅行者玩家一");
        rows[1].GetNode<MegaLabel>("%NameplateLabel").SetTextAutoSize("Traveler Player Two With A Long Name");
        for(var frame=0;frame<4;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        foreach(var row in rows)
            GD.Print($"STS2MOD_LOBBY_ROW id={row.PlayerId} list={list.GetClass()} root={row.GetGlobalRect()} min={row.GetCombinedMinimumSize()} icon={row.GetNode<Control>("%CharacterIcon").GetGlobalRect()}");
        Check(!rows[0].GetNode<Control>("%CharacterIcon").GetGlobalRect().Intersects(
            rows[1].GetNode<Control>("%CharacterIcon").GetGlobalRect()),"CHAR_lobby_two_traveler_portraits_do_not_overlap");
        bool Separated() => rows.Zip(rows.Skip(1)).All(pair=>
            !pair.First.GetGlobalRect().Intersects(pair.Second.GetGlobalRect())
            && !pair.First.GetNode<Control>("%NameplateLabel").GetGlobalRect().Intersects(
                pair.Second.GetNode<Control>("%NameplateLabel").GetGlobalRect()));
        Check(Separated(),"CHAR_lobby_four_mixed_players_and_names_separated");
        Check(rows[1].GetNode<Control>("%ReadyIndicator").Visible && !rows[0].GetNode<Control>("%ReadyIndicator").Visible
            && rows[0].GetNode<TextureRect>("%CharacterIcon").Texture.ResourcePath.EndsWith("head_icon.png"),
            "CHAR_lobby_native_portrait_and_ready_state_preserved");
        for(var frame=0;frame<35;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Check(Separated(),"CHAR_lobby_positions_stable_after_shake_duration");
        for(var change=0;change<8;change++)
        {
            rows[1].OnPlayerChanged(new LobbyPlayer{id=2,slotId=1,
                character=change%2==0?ModelDb.Character<Ironclad>():ModelDb.Character<Traveler>(),isReady=change%2==1});
            for(var frame=0;frame<3;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            Check(Separated(),"CHAR_lobby_repeated_character_switch_"+change);
        }
        list.MoveChild(rows[3],0); list.QueueSort();
        for(var frame=0;frame<3;frame++)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Check(rows.SelectMany((row,i)=>rows.Skip(i+1).Select(other=>!row.GetGlobalRect().Intersects(other.GetGlobalRect()))).All(x=>x),
            "CHAR_lobby_container_reflow_keeps_rows_separate");
        foreach(var row in rows)row.QueueFree();
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
    }
}
