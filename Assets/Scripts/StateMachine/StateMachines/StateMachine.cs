using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected State _currentState {  get; set; }
    public string CurrentState => _currentState.ToString();

    public virtual void Update()
    {
        _currentState?.Tick(Time.deltaTime);
    }

    public void SwitchState(State newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }
}
