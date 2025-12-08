using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Behaviours;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Actions
{
    [GoapId("PickupPear-4162d8c8-45a4-4b28-8382-a6a5f6f3d75b")]
    public class PickupPearAction : GoapActionBase<PickupPearAction.Data>
    {
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.WaitThenComplete(.5f);
        }

        public override void Complete(IMonoAgent agent, Data data)
        {
            if (data.Target is not TransformTarget transformTarget) return;

            data.DataBehaviour.PearCount++;
            GameObject.Destroy(transformTarget.Transform.gameObject);
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            [GetComponent]
            public DataBehaviour DataBehaviour { get; set; }
        }
    }
}