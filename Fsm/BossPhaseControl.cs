using DefaultSetting;
using SerializedTuples;
using SerializedTuples.Runtime;
using System;
using UnityEngine;
public class BossPhaseControl : MonoBehaviour
{
    [Serializable]
    private struct PhaseInfoData
    {
        [SerializeField] private float _startPhaseHpPercent; // 페이즈가 시작되는 체력 퍼센트
        [SerializeField] private float _stateTranseDelay; // 상태 전환 딜레이

        // 상태, 등장 퍼센트
        [SerializedTupleLabels("PhaseState", "ActionPercent")]
        [SerializeField] SerializedTuple<BaseState, float>[] _phaseInfo;

        [SerializeField] private AudioClip _audioClip; // 해당 페이즈 배경음
        [SerializeField] private float _sFxVolume; // 해당 배경음 크기


        public float StartPhaseHpPercent => _startPhaseHpPercent;
        public float StateTranseDelay => _stateTranseDelay;
        public SerializedTuple<BaseState, float>[] PhaseInfo => _phaseInfo;
        public AudioClip AudioClip => _audioClip;
        public float SfxVolume => _sFxVolume;
    }

    private BossFsm _controlBossFsm; // 컨트롤할 Fsm
    private Health _health; // 현재 체력 컴포넌트

    [SerializeField] private PhaseInfoData[] _phases; // 페이즈들
    private PhaseInfoData _currentPhase; // 현재 페이즈 정보
    private int _phaseStep = 0;

    private bool _isPhaseInitialized = false;

    private void Awake()
    {
        _controlBossFsm = GetComponent<BossFsm>();
        _health = GetComponent<Health>();

    }

    private void Start()
    {
        Array.Sort(_phases, (a, b) => b.StartPhaseHpPercent.CompareTo(a.StartPhaseHpPercent));

        if (_controlBossFsm == null || _health == null)
            return;

        if (_phases.Length > 0)
        {
            UpdatePhase();
        }
    }

    private void Update()
    {
        if (_isPhaseInitialized)
            UpdatePhase();
    }

    private void UpdatePhase()
    {
        for (int i = _phaseStep; i < _phases.Length; i++)
        {
            var phase = _phases[i];

            // 체력 컷에 맞고, 현재 상태와 같은 것이 아닐 때
            if (_health.CurrentHealth <= _health.MaxHealth * phase.StartPhaseHpPercent / 100f && !_currentPhase.Equals(phase))
            {
                _currentPhase = phase;
                _isPhaseInitialized = true;
                _phaseStep = i;

                // 보스의 상태 전환을 막고 현재 단계에 맞는 상태들로 업데이트
                _controlBossFsm.StateTransDelay = _currentPhase.StateTranseDelay;
                _controlBossFsm.TransitionToState(Define.BaseFsmStateType.Idle);

                // 오디오 클립 재생 (페이즈 전환 효과)
                if (_currentPhase.AudioClip != null)
                {
                    Managers.Sound.Play(_currentPhase.AudioClip, Define.Sound.Bgm, _currentPhase.SfxVolume);
                }

                break;
            }
        }
    }

    public IBaseState TriggerRandomState()
    {
        IBaseState state = null;
        if (!_isPhaseInitialized) return state;

        float totalWeight = 0f;
        foreach (var stateInfo in _currentPhase.PhaseInfo)
        {
            totalWeight += stateInfo.v2;
        }

        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var stateInfo in _currentPhase.PhaseInfo)
        {
            cumulativeWeight += stateInfo.v2;
            if (randomValue <= cumulativeWeight)
            {
                var baseState = stateInfo.v1;
                state = (baseState is IBaseState) ? baseState as IBaseState : null;
                break;
            }
        }

        return state;
    }
}
