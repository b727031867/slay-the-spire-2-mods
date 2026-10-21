namespace Sts2Mod.Cards;

public static class TravelerContent
{
    public static IReadOnlyList<Type> RewardCardTypes { get; } = Array.AsReadOnly(new[]
    {
        typeof(WindBlade), typeof(RockGuard), typeof(LightningThrust), typeof(TailwindPrep), typeof(RockAdvance), typeof(ThunderPrep),
        typeof(WindGuidance), typeof(RockHeart), typeof(ThunderEcho), typeof(ElementResonance),
        typeof(GaleSweep), typeof(ShatteringRock), typeof(ThunderMark), typeof(TriphaseFinale),
        typeof(WindShelter), typeof(PatientRampart), typeof(ChargeThunder), typeof(JourneyPlans), typeof(ThunderChain)
    });
}
