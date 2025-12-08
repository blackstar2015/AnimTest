using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Actions;
using RPGCharacterAnims.Actions;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Capabilities
{
    public class EatCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            CapabilityBuilder builder = new CapabilityBuilder("EatCapability");

            builder.AddGoal<EatGoal>()
                .AddCondition<Hunger>(Comparison.SmallerThanOrEqual, 0);


            builder.AddAction<EatAction>()
                .AddEffect<Hunger>(EffectType.Decrease)
                .AddCondition<PearCount>(Comparison.GreaterThanOrEqual, 1)
                .SetRequiresTarget(false);

            return builder.Build();
        }
    }
}

