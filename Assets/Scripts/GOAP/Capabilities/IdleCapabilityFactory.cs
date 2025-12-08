using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Actions;
using CrashKonijn.Scripts.GOAP.Sensors;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Capabilities
{
    public class IdleCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            CapabilityBuilder builder = new CapabilityBuilder("IdleCapability");

            builder.AddGoal<IdleGoal>()
                .AddCondition<IsIdle>(Comparison.GreaterThanOrEqual,1)
                .SetBaseCost(2);

            builder.AddAction<IdleAction>()
                .AddEffect<IsIdle>(EffectType.Increase)
                .AddEffect<Hunger>(EffectType.Increase)
                .SetTarget<IdleTarget>();

            builder.AddTargetSensor<IdleTargetSensor>()
                .SetTarget<IdleTarget>();

            return builder.Build();
        }    
    }
}
