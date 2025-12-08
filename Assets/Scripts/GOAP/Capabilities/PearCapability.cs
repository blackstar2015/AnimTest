using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Actions;
using CrashKonijn.Scripts.GOAP.Sensors;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Capabilities
{
    public class PearCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            CapabilityBuilder builder = new CapabilityBuilder("PearCapability");

            builder.AddGoal<PickupPearGoal>()
                .AddCondition<PearCount>(Comparison.GreaterThanOrEqual, 3);

            builder.AddAction<PickupPearAction>()
                .AddEffect<PearCount>(EffectType.Increase)
                .SetTarget<ClosestPear>();

            builder.AddMultiSensor<PearSensor>();
            
            return builder.Build();
        }
    }
}
