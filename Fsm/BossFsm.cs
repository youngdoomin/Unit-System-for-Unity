using UnityEngine;

public class BossFsm : BaseFsm
{
    // 전환 시 딜레이
    [SerializeField] private float _transStateDelay;
    private BossPhaseControl _phaseControl;

    public float StateTransDelay
    {
        set => _transStateDelay = value;
    }

    protected override void InitFsm()
    {
        base.InitFsm();
        _phaseControl = GetComponent<BossPhaseControl>();
    }

    protected override void Fsm_IdleState(Fsm fsm, Fsm.Step step, Fsm.State state)
    {
        base.Fsm_IdleState(fsm, step, state);
        if (step == Fsm.Step.Enter)
        {
            // _bodyRenderer.enabled = false;
        }
        else if (step == Fsm.Step.Update)
        {
            var actionStates = GetActionableStates();


            if (!_phaseControl)
                BossBasicTransState(fsm);
            else
                BossPhaseState(fsm);

        }
        else if (step == Fsm.Step.Exit)
        {
            // _bodyRenderer.enabled = true;
        }
    }

    private void BossBasicTransState(Fsm fsm)
    {
        var actionStates = GetActionableStates();
        if (actionStates.states == null || actionStates.states.Count == 0 || actionStates.baseStates == null || actionStates.baseStates.Count == 0)
            return;

        int random = Random.Range(0, actionStates.states.Count);
        InitializeStateConfig(actionStates.baseStates[random]);

        var actionState = actionStates.states[random];
        fsm.TransitionTo(actionState);
    }

    private void BossPhaseState(Fsm fsm)
    {
        var state = _phaseControl.TriggerRandomState();
        if (state == null)
            return;

        fsm.TransitionTo(state.Fsm_ActionState);
    }

    protected override void InitializeStateConfig(BaseState baseState)
    {
        /*FlyState flyState = baseState as FlyState;
        flyState?.InitializedStateValue(_flyConfig);*/

        // 관련 Config 처리 추가 필요
    }
}
