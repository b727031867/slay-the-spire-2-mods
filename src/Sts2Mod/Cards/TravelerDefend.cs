using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_DEFEND"), Pool(typeof(TravelerCardPool))]
public sealed class TravelerDefend : CustomCardModel
{
    public TravelerDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self) { }
    public override bool GainsBlock => true;
    public override string CustomPortraitPath => "res://Sts2Mod/art/defend.svg";
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
}
