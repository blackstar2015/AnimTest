using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Behaviours;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Actions
{
    [GoapId("Eat-12c134be-fbc2-4e0f-abfa-c23aad716e17")]
    public class EatAction : GoapActionBase<EatAction.Data>
    {
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.WaitThenComplete(.5f);
        }
        public override void Complete(IMonoAgent agent, Data data)
        {
            data.DataBehaviour.PearCount--;
            data.DataBehaviour.Hunger = 0f;
        }
        public class Data : IActionData
        {
            public ITarget Target { get; set; }

            [GetComponent]
            public DataBehaviour DataBehaviour { get; set; }
        }
    }
}