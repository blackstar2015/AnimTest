using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Capabilities;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.AgentTypes
{
    public class DemoAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("ScriptDemoAgent");
            factory.AddCapability<IdleCapabilityFactory>();
            factory.AddCapability<PearCapability>();
            factory.AddCapability<EatCapability>();
            return factory.Build();
        }
    }
}
