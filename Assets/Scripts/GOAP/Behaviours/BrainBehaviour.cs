using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using System;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Behaviours
{
    public class BrainBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private DataBehaviour data;
        private GoapBehaviour _goap;

        private void Awake()
        {
            _goap = FindFirstObjectByType<GoapBehaviour>();
            _agent = GetComponent<AgentBehaviour>();
            _provider = GetComponent<GoapActionProvider>();
            data = GetComponent<DataBehaviour>();

            if (_provider.AgentTypeBehaviour == null)
                _provider.AgentType = _goap.GetAgentType("ScriptDemoAgent");
        }

        private void Start()
        {
            _provider.RequestGoal<IdleGoal, PickupPearGoal>();
        }

        private void OnEnable()
        {
            _agent.Events.OnActionEnd += OnActionEnd;
        }

        private void OnDisable()
        {
            _agent.Events.OnActionEnd -= OnActionEnd;
        }

        private void OnActionEnd(IAction action)
        {
            if(data.Hunger > 50)
            {
                _provider.RequestGoal<EatGoal>();
                return;
            }
            _provider.RequestGoal<IdleGoal, PickupPearGoal>();
        }
    }
}
