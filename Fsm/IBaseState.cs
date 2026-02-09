public interface IBaseState
{
    public void Initialized();
    public void Disabled();
    public bool IsActionabled();
    public void Fsm_ActionState(Fsm fsm, Fsm.Step step, Fsm.State state);
}