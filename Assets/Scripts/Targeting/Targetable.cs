using UnityEngine;

public class Targetable : MonoBehaviour
{
    public int Team { get; private set; } 
    public bool IsTargetable { get; set; }
    public Transform ViewPosition { get; private set;}

    private void Awake()
    {
        if (gameObject.TryGetComponent(out StateMachine stateMachine))
        {
            Team = stateMachine.Team;
            IsTargetable = stateMachine.IsTargetable;
            ViewPosition = stateMachine.ViewPosition;
        }
        else Team = 0;
    }
}
