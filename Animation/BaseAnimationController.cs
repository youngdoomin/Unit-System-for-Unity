using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AniInfo
{
    private string _name;     // 애니메이션 이름
    private int _hash;        // 애니메이션 해시 값
    private float _length;    // 애니메이션 길이

    public AniInfo(string name, int hash, float length)
    {
        _name = name;
        _hash = hash;
        _length = length;
    }

    public string Name => _name;
    public int Hash => _hash;
    public float Length => _length;
}

public class BaseAnimationController : MonoBehaviour
{
    private bool _isInitialized = false;

    private Animator _animator;

    private Dictionary<string, AniInfo> _animationInfosByName;
    private Dictionary<int, AniInfo> _animationInfosByHash;

    private AniInfo _currentAniInfo;
    private AniInfo _defaultAniInfo;

    public AniInfo CurrentAniInfo => _currentAniInfo;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        InitializeAnimationInfos(); // 시작 시 모든 애니메이션 정보를 초기화
    }

    private void OnDisable()
    {
        ClearAnimationData();
    }

    // 애니메이션 정보를 초기화하여 리스트에 저장
    private void InitializeAnimationInfos()
    {
        if (_isInitialized) return;

        if (_animator == null)
        {
            Debug.LogError($"[{gameObject.name}] Animator not found. Cannot initialize animations.");
            return;
        }

        RuntimeAnimatorController rac = _animator.runtimeAnimatorController;
        if (rac == null)
        {
            Debug.LogError($"[{gameObject.name}] RuntimeAnimatorController is null.");
            return;
        }

        _animationInfosByName = new Dictionary<string, AniInfo>();
        _animationInfosByHash = new Dictionary<int, AniInfo>();

        foreach (AnimationClip clip in rac.animationClips)
        {
            string clipName = clip.name;
            int clipHash = Animator.StringToHash(clipName);
            float clipLength = clip.length;

            AniInfo animationInfo = new AniInfo(clipName, clipHash, clipLength);

            if (!_animationInfosByName.ContainsKey(clipName))
            {
                _animationInfosByName.Add(clipName, animationInfo);
                _animationInfosByHash.Add(clipHash, animationInfo);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Duplicate animation name: {clipName}");
            }
        }

        if (_animationInfosByName.Count > 0)
        {
            foreach (var info in _animationInfosByName.Values)
            {
                _defaultAniInfo = info;
                break;
            }
        }

        _isInitialized = true;
        Debug.Log($"[{gameObject.name}] Successfully initialized {_animationInfosByName.Count} animations.");
    }

    private void ClearAnimationData()
    {
        _animationInfosByName?.Clear();
        _animationInfosByHash?.Clear();
        _isInitialized = false;
    }

    // 애니메이션 이름으로 AnimationInfo를 검색하여 반환
    public AniInfo? GetAnimationInfoByName(string name)
    {
        InitializeAnimationInfos();
        if (!_isInitialized) return null;

        if (_animationInfosByName.TryGetValue(name, out AniInfo info))
        {
            return info;
        }

        Debug.LogWarning($"[{gameObject.name}] Animation '{name}' not found.");
        return null;
    }

    // 해시 값으로 AnimationInfo를 검색하여 반환
    public AniInfo? GetAnimationInfoByHash(int hash)
    {
        InitializeAnimationInfos();
        if (!_isInitialized) return null;

        if (_animationInfosByHash.TryGetValue(hash, out AniInfo info))
        {
            return info;
        }

        Debug.LogWarning($"[{gameObject.name}] Animation hash '{hash}' not found.");
        return null;
    }

    // 애니메이션 해시 값을 통해 강제 전환하는 메서드
    public void Play(int animationHash)
    {
        InitializeAnimationInfos();
        if (!_isInitialized) return;

        AniInfo aniInfo = GetAnimationInfoByHash(animationHash) ?? _defaultAniInfo;

        if (aniInfo.Hash == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Cannot play animation. Invalid hash: {animationHash}");
            return;
        }

        _currentAniInfo = aniInfo;
        _animator?.Play(_currentAniInfo.Hash);
    }

    public void Play(string animationName)
    {
        AniInfo? info = GetAnimationInfoByName(animationName);
        if (info.HasValue)
        {
            Play(info.Value.Hash);
        }
    }

    public void CrossFade(int animationHash, float normalizedTransitionDuration = 0.0f)
    {
        InitializeAnimationInfos();
        if (!_isInitialized) return;

        AniInfo aniInfo = GetAnimationInfoByHash(animationHash) ?? _defaultAniInfo;

        if (aniInfo.Hash == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Cannot crossfade animation. Invalid hash: {animationHash}");
            return;
        }

        _currentAniInfo = aniInfo;
        _animator?.CrossFade(_currentAniInfo.Hash, normalizedTransitionDuration);
    }
}