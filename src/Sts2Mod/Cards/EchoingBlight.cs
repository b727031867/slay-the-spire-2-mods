using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C28"), Pool(typeof(TravelerCardPool))]
public sealed class EchoingBlight() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/drafts/TC_C28.svg";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        RemoveKeyword(CardKeyword.Ethereal);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        var effects = play.Target.Powers
            .Where(power => power.TypeForCurrentAmount == PowerType.Debuff && power.StackType == PowerStackType.Counter)
            .Select(power => (Power: power, Amount: power.Amount)).ToArray();
        foreach (var (power, amount) in effects)
        {
            if (!play.Target.IsAlive) break;
            if (play.Target.Powers.Contains(power))
                await PowerCmd.ModifyAmount(context, power, amount, Owner.Creature, this);
        }
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
