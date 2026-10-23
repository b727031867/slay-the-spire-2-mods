using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Relics;
using Sts2Mod.Powers;

namespace Sts2Mod.Combo;

/// <summary>Reserves validated native actions and resolves rewards in their original order.</summary>
public static class ComboResolver
{
    public static async Task<Reservation> Reserve(CardModel card, PlayerChoiceContext context, CardPlay play, Element element)
    {
        element = CombatCardElements.Get(card, element);
        var power = card.Owner.Creature.GetPower<ComboPower>();
        var chosen = CombatCardElements.Chosen.Get(card);
        if ((play.IsAutoPlay && !chosen) || !play.IsFirstInSeries)
        {
            GD.Print($"STS2MOD_STEP player={card.Owner.NetId} card={card.Id.Entry} auto={play.IsAutoPlay} index={play.PlayIndex} advanced=False reason=automatic_or_extra_resolution");
            return default;
        }
        // Only a validated, active native hand play may advance the sequence.
        if (power is null) return default;
        object identity;
        uint? actionId = null;
        if (chosen) identity = play;
        else if (context is GameActionPlayerChoiceContext { Action: PlayCardAction action }
            && action.Player == card.Owner && action.NetCombatCard.ToCardModel() == card)
        { identity = action; actionId = action.Id; }
        else return default;
        power.RefreshLimit();
        var step = power.State.Reserve(identity, element);
        if (step.Triggered)
        {
            foreach (var blessing in card.Owner.Relics.OfType<StatueBlessing>())
                await blessing.OnComboTriggered(context);
            foreach (var compass in card.Owner.Relics.OfType<UnboundJourneyCompass>())
                await compass.OnComboTriggered(context);
        }
        var paimonDoubles = step.Triggered
            && card.Owner.Relics.Any(r => r is PaimonCheer { IsMelted: false });
        var rewardAmount = ComboPresentation.Reward(step.Kind, paimonDoubles, ComboPresentation.EchoCopies(card.Owner));
        var reward = step.Kind switch
        {
            ComboKind.WindRock => $"block{rewardAmount}",
            ComboKind.RockLightning => $"attack{rewardAmount}",
            ComboKind.LightningWind => $"draw{rewardAmount}",
            _ => "none"
        };
        power.Publish(step.Advanced ? "reserve" : "ignored");
        GD.Print($"STS2MOD_STEP player={card.Owner.NetId} action={actionId} card={card.Id.Entry} auto={play.IsAutoPlay} index={play.PlayIndex} advanced={step.Advanced} before={step.Before} after={step.After} used={step.Used} kind={step.Kind} reward={reward} paimonDouble={paimonDoubles}");
        return new(step.Kind, actionId, paimonDoubles);
    }

    public static async Task Resolve(CardModel card, PlayerChoiceContext context, CardPlay play, Reservation reservation)
    {
        if (reservation.Kind == ComboKind.None) return;
        var creature = card.Owner.Creature;
        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || !creature.IsAlive
            || creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) != true)
        {
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} skipped=combat_ended_or_dead");
            return;
        }
        var echoCopies = ComboPresentation.EchoCopies(card.Owner);
        var bonus = ComboPresentation.Reward(reservation.Kind, reservation.PaimonDoubles, echoCopies);
        var draws = ComboPresentation.Reward(ComboKind.LightningWind, reservation.PaimonDoubles, echoCopies);
        var awarded = false;
        if (reservation.Kind == ComboKind.RockLightning)
        {
            var target = play.Target;
            // Self/untargeted defensive skills use the native seeded combat-target RNG.
            // A targeted attack whose victim died still must not jump to another enemy.
            var randomTarget = reservation.Effect || (card.Type != CardType.Attack && card.TargetType != TargetType.AnyEnemy);
            if (!randomTarget && (target is null || !target.IsAlive || target.Side == creature.Side
                || !creature.CombatState!.Creatures.Contains(target)))
            {
                GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} skipped=target_dead_or_removed kind=RockLightning");
            }
            else
            {
                var attack = DamageCmd.Attack(bonus).FromCard(card);
                if (randomTarget) attack.TargetingRandomOpponents(creature.CombatState!);
                else attack.Targeting(target!);
                using (Presentation.CombatPresentation.Reward(reservation.Kind, reservation.PaimonDoubles))
                    await attack.WithHitFx("vfx/vfx_attack_slash").Execute(context);
                var results = attack.Results.SelectMany(results => results).ToArray();
                var receiver = results.FirstOrDefault()?.Receiver;
                if (receiver is not null && results.Any(result => result.TotalDamage > 0)) ComboFeedback.ShowReward(creature, receiver, reservation);
                awarded = results.Any(result => result.TotalDamage > 0);
                if (awarded && receiver is not null)
                    foreach (var nail in card.Owner.Relics.OfType<WallPiercingNail>())
                        await nail.OnHit(context, receiver);
                GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} kind=RockLightning target_mode={(randomTarget ? "random" : "selected")} target={receiver?.CombatId} bonus={bonus}");
            }
        }
        else if (reservation.Kind == ComboKind.LightningWind)
        {
            var handBefore = PileType.Hand.GetPile(card.Owner).Cards.Count;
            var drawn = (await CardPileCmd.Draw(context, draws, card.Owner)).Count();
            ComboFeedback.ShowReward(creature, creature, reservation, drawn);
            awarded = drawn > 0;
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} kind=LightningWind hand_before={handBefore} hand_after={PileType.Hand.GetPile(card.Owner).Cards.Count} bonus_draw={draws}");
        }
        else
        {
            var before = creature.Block;
            creature.GetPower<ComboPower>()?.FlashReward();
            using (Presentation.CombatPresentation.Reward(reservation.Kind, reservation.PaimonDoubles))
                await CreatureCmd.GainBlock(creature, bonus, ValueProp.Unpowered, play);
            if (creature.Block > before) ComboFeedback.ShowReward(creature, creature, reservation);
            awarded = creature.Block > before;
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} block_before={before} block_after={creature.Block} bonus={bonus}");
        }
        if (awarded && creature.IsAlive)
            foreach (var relic in card.Owner.Relics.OfType<IEventComboRelic>().ToArray())
                await relic.OnReward(context, reservation.Kind);
        if (awarded && creature.IsAlive && CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding
            && creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) == true)
        {
            var actualRewardNumber = creature.GetPower<ComboPower>()!.State.MarkRewardGranted();
            foreach (var ability in creature.Powers.OfType<TravelerAbilityPower>().ToArray())
                await ability.OnComboReward(context, reservation.Kind, actualRewardNumber);
            foreach (var compass in card.Owner.Relics.OfType<IComboRewardRelic>().ToArray())
                await compass.OnComboReward(context);
        }
    }
}
