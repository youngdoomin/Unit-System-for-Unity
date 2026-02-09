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
    private bool _isinitalized = false;

    private Animator _animator; // Unity Animator
    private List<AniInfo> _animationInfos; // 애니메이션 정보를 담은 리스트
    private AniInfo _currentAniInfo;

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
        _animationInfos?.Clear();
        _isinitalized = false;
    }

    // 애니메이션 정보를 초기화하여 리스트에 저장
    private void InitializeAnimationInfos()
    {
        if (_isinitalized) return;
        if (_animator == null)
        {
            Debug.LogError("초기화 되지 않았니다.");
            _isinitalized = false;
            return;
        }

        _animationInfos = new List<AniInfo>();
        RuntimeAnimatorController rac = _animator.runtimeAnimatorController;

        foreach (AnimationClip clip in rac.animationClips)
        {
            string clipName = clip.name;
            int clipHash = Animator.StringToHash(clipName);
            float clipLength = clip.length;

            AniInfo animationInfo = new AniInfo(clipName, clipHash, clipLength);
            _animationInfos.Add(animationInfo);
        }

        _isinitalized = true;
        Debug.Log("모든 애니메이션 정보가 성공적으로 저장되었습니다.");
    }

    // 애니메이션 이름으로 AnimationInfo를 검색하여 반환
    public AniInfo? GetAnimationInfoByName(string name)
    {
        InitializeAnimationInfos();
        if (!_isinitalized) return null;

        foreach (AniInfo info in _animationInfos)
        {
            if (info.Name == name)
                return info;
        }

        Debug.LogWarning($"'{name}' 애니메이션 정보를 찾을 수 없습니다.");
        return null;
    }

    // 해시 값으로 AnimationInfo를 검색하여 반환
    public AniInfo? GetAnimationInfoByHash(int hash)
    {
        InitializeAnimationInfos();
        if (!_isinitalized) return null;

        foreach (AniInfo info in _animationInfos)
        {
            if (info.Hash == hash)
                return info;
        }

        Debug.LogWarning($"해시 '{hash}' 값에 해당하는 애니메이션 정보를 찾을 수 없습니다.");
        return null;
    }

    // 애니메이션 해시 값을 통해 강제 전환하는 메서드
    public void Play(int animationHash)
    {
        InitializeAnimationInfos();
        if (!_isinitalized) return;

        var aniInfo = GetAnimationInfoByHash(animationHash).GetValueOrDefault();
        if (aniInfo.Hash == 0)
        {
            if (_animationInfos.Count == 0) return;
            aniInfo = _animationInfos[0];
        }
        _currentAniInfo = aniInfo;
        _animator?.Play(_currentAniInfo.Hash);
    }
    public void CrossFade(int animationHash, float normalizedTransitionDuration = 0.0f)
    {
        InitializeAnimationInfos();
        if (!_isinitalized) return;

        var aniInfo = GetAnimationInfoByHash(animationHash).GetValueOrDefault();
        if (aniInfo.Hash == 0)
        {
            if (_animationInfos.Count == 0) return;
            aniInfo = _animationInfos[0];
        }
        _currentAniInfo = aniInfo;
        _animator?.CrossFade(_currentAniInfo.Hash, normalizedTransitionDuration);
    }
}