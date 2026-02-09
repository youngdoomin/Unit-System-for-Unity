using UnityEngine;

public class FixedDirectionProjectile : Projectile
{
    private Vector3 _fixedDirection;
    private float _deactivateTime = 10f; // 발사 후 10초 후 비활성화
    private float _spawnTime;

    private void OnEnable()
    {
        CalculateDirection(); // 발사 전 타겟과의 방향 계산
        _spawnTime = Time.time; // 발사 시각 저장
    }

    // 타겟과의 방향을 계산하여 고정된 방향을 설정하는 메서드
    private void CalculateDirection()
    {
        if (_target != null)
        {
            _fixedDirection = (Vector2)_target.position - (Vector2)transform.position;
            _fixedDirection.Normalize(); // 방향 벡터를 정규화
            float rotAngle = Mathf.Atan2(_fixedDirection.y, _fixedDirection.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);
            transform.rotation = rotation;
        }
        else
        {
            Debug.LogWarning("Target is null, cannot calculate direction.");
        }
    }

    public override void Movement()
    {
        // 10초 이상 지나면 비활성화
        if (Time.time - _spawnTime > _deactivateTime)
        {
            gameObject.SetActive(false);
            return;
        }

        // 기본 속도가 0 이하이면 움직이지 않음
        if (Speed <= 0)
            return;

        // 고정된 방향으로 발사체를 이동시킴
        _movement = _fixedDirection * Speed * Time.deltaTime;
        transform.Translate(_movement, Space.World);

        // 속도 증가 (가속도 적용)
        Speed += Acceleration * Time.deltaTime;
    }
}
