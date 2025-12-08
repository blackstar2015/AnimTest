using CrashKonijn.Goap.Runtime;
using CrashKonijn.Scripts.GOAP.Behaviours;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CrashKonijn.Scripts.GOAP.Sensors
{
    public class PearSensor : MultiSensorBase
    {
        private PearBehaviour[] _pears;

        public PearSensor()
        {
            AddLocalWorldSensor<PearCount>((agent, references) =>
            {
                DataBehaviour data = references.GetCachedComponent<DataBehaviour>();

                return data.PearCount;
            });

            AddLocalWorldSensor<Hunger>((agent, references) =>
            {
                DataBehaviour data = references.GetCachedComponent<DataBehaviour>();
                return (int)data.Hunger;
            });

            AddLocalTargetSensor<ClosestPear>((agent, references, target) => 
            {
                var closestPear = Closest(_pears, agent.Transform.position);
                if (closestPear == null) return null;

                if (target is TransformTarget transformTarget) return transformTarget.SetTransform(closestPear.transform);

                return new TransformTarget(closestPear.transform);
                
            });
        }

        public override void Created() {}

        public override void Update() 
        {
            _pears = Object.FindObjectsByType<PearBehaviour>(FindObjectsSortMode.None);
        }

        private T Closest<T>(IEnumerable<T> list, Vector3 position) where T : MonoBehaviour
        {
            T closest = null;
            float closestDistance = float.MaxValue;

            foreach (T item in list)
            {
                float distance = Vector3.Distance(item.gameObject.transform.position, position);

                if (!(distance < closestDistance)) continue;
                closest = item;
                closestDistance = distance;
            }
            return closest;
        }
    }
}
