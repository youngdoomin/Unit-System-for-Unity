using UnityEngine;

public class Fsm
{
    public enum Step
    {
        Enter,
        Update,
        Exit
    }

    public delegate void State(Fsm fsm, Step step, State state);

    State _currentState;
    private float _enterTime;

    public float EnterTime => _enterTime;

    public void Start(State startState)
    {
        TransitionTo(startState);
    }

    public void OnUpdate()
    {
        _currentState.Invoke(this, Step.Update, null);
        _enterTime += Time.deltaTime;
    }

    public void TransitionTo(State state)
    {
        _currentState?.Invoke(this, Step.Exit, state);
        var oldState = _currentState;
        _currentState = state;
        _currentState.Invoke(this, Step.Enter, oldState);
        _enterTime = 0;
    }
}
