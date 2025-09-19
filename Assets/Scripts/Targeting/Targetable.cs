using UnityEngine;

public class Targetable : MonoBehaviour
{
    [field: SerializeField] public int Team { get; private set; } = 1;  
    [field: SerializeField] public bool IsTargetable { get; set; } = true;
    [field: SerializeField] public Transform ViewPosition { get; private set;}
}
