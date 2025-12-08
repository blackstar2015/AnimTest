using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Behaviours;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Actions
{
    [GoapId("Idle-b2701a24-db90-4162-b97d-e69b9f90d01c")]
    public class IdleAction : GoapActionBase<IdleAction.Data>
    {
        // This method is called when the action is created
        // This method is optional and can be removed
        public override void Created() { }

        // This method is called every frame before the action is performed
        // If this method returns false, the action will be stopped
        // This method is optional and can be removed
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            return true;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = Random.Range(.5f, 1.5f);
        }

        // This method is called once before the action is performed
        // This method is optional and can be removed
        public override void BeforePerform(IMonoAgent agent, Data data) {}

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Timer <= 0f) return ActionRunState.Completed;               
            
            data.Timer -= context.DeltaTime;
            return ActionRunState.Continue;
        }

        // This method is called when the action is completed
        // This method is optional and can be removed
        public override void Complete(IMonoAgent agent, Data data) 
        {
            data.DataBehaviour.Hunger++;
        }

        // This method is called when the action is stopped
        // This method is optional and can be removed
        public override void Stop(IMonoAgent agent, Data data) {}

        // This method is called when the action is completed or stopped
        // This method is optional and can be removed
        public override void End(IMonoAgent agent, Data data) { }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer { get; set; }
            [GetComponent]
            public DataBehaviour DataBehaviour {  get; set; }
        }
    }
}