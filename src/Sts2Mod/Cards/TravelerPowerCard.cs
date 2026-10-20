using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Sts2Mod.Cards;

public abstract class TravelerPowerCard<TPower>(int cost, CardRarity rarity, decimal amount)
    : CustomCardModel(cost, CardType.Power, rarity, TargetType.Self) where TPower : PowerModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("PowerAmount", amount)];
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
        => await PowerCmd.Apply<TPower>(context, Owner.Creature, DynamicVars["PowerAmount"].BaseValue, Owner.Creature, this);
}
