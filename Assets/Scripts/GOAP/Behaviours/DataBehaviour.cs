using UnityEngine;

namespace CrashKonijn.Scripts.GOAP.Behaviours
{
    public class DataBehaviour : MonoBehaviour
    {
        public int PearCount = 0;
        public float Hunger = 0f;

        private void Update()
        {
            Hunger += Time.deltaTime * 5f;
        }
    }
}
