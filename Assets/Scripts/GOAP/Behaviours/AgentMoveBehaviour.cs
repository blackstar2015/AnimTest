using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using System;
using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Behaviours
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private AgentBehaviour agent;
        private ITarget _currentTarget;
        private bool _shouldMove;

        private void Awake()
        {
            agent = GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {
            agent.Events.OnTargetInRange += OnTargetInRange;
            agent.Events.OnTargetChanged += OnTargetChanged;
            agent.Events.OnTargetNotInRange += OnTargetNotInRange;
            agent.Events.OnTargetLost += OnTargetLost;
        }

        private void OnDisable()
        {
            agent.Events.OnTargetInRange -= OnTargetInRange;
            agent.Events.OnTargetChanged -= OnTargetChanged;
            agent.Events.OnTargetNotInRange -= OnTargetNotInRange;
            agent.Events.OnTargetLost -= OnTargetLost;
        }

        private void Update()
        {
            if (agent.IsPaused || !_shouldMove || _currentTarget == null) return;
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(_currentTarget.Position.x, _currentTarget.Position.y, _currentTarget.Position.z), 
                Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (_currentTarget == null) return;
             
            Gizmos.DrawLine(transform.position, _currentTarget.Position);
        }

        private void OnTargetLost()
        {
            _currentTarget = null;
            _shouldMove = false;
        }

        private void OnTargetNotInRange(ITarget target)
        {
            _shouldMove = true;
        }

        private void OnTargetChanged(ITarget target, bool inRange)
        {
            _currentTarget = target;
            _shouldMove = !inRange;
        }

        private void OnTargetInRange(ITarget target)
        {
            _shouldMove = false;
        }
    }
}
