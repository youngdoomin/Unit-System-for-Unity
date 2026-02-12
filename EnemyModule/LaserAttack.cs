using DefaultSetting;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class LaserAttack : BaseWeaponSensor
{
    [Header("Laser pieces")]
    [SerializeField] private GameObject _laserStart;
    [SerializeField] private GameObject _laserMiddle;
    [SerializeField] private GameObject _laserEnd;
    private GameObject _start;
    private GameObject _middle;
    private GameObject _end;
    private SpriteRenderer _startSpriteRenderer;
    private SpriteRenderer _middleSpriteRenderer;
    private SpriteRenderer _endSpriteRenderer;
    private float fadeInTime = 0.2f;
    private float fadeOutTime = 1f;
    [SerializeField] private float _damage;
    [SerializeField] private AudioClip _shootReadySfx;
    [SerializeField] private AudioClip _shootSfx;

    public IEnumerator Shoot(Transform target, float beforeShootDelay = 0, Action shootAction = null)
    {

        Managers.Sound.Play(_shootReadySfx);
        yield return new WaitForSeconds(beforeShootDelay);
        // Create the laser start from the prefab
        if (_start == null)
        {
            _start = Instantiate(_laserStart) as GameObject;
            _start.transform.parent = this.transform;
            _start.transform.localPosition = Vector2.zero;
            _startSpriteRenderer = _start.GetComponentInChildren<SpriteRenderer>();
        }

        // Laser middle
        if (_middle == null)
        {
            _middle = Instantiate(_laserMiddle) as GameObject;
            _middle.transform.parent = this.transform;
            _middle.transform.localPosition = Vector2.zero;
            _middleSpriteRenderer = _middle.GetComponentInChildren<SpriteRenderer>();
        }

        // Define an "infinite" size, not too big but enough to go off screen
        float maxLaserSize = float.PositiveInfinity;

        Vector2 laserDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

        float rotAngle = Mathf.Atan2(laserDirection.y, laserDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);
        transform.rotation = rotation;

        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, transform.right, maxLaserSize, CheckLayerMask);
        print(hit.collider.gameObject.name);

        float currentLaserSize = Vector2.Distance(hit.collider.transform.position, this.transform.position) / 2;
        // Adjust the laser middle size and position
        Vector2 middleScale = _middle.transform.localScale;
        middleScale.x = currentLaserSize; // Stretch the middle piece to the length of the laser
        _middle.transform.localScale = middleScale;
        _middle.transform.localPosition = new Vector2(currentLaserSize / 2, 0); // Place the middle part in between

        // Create or update the laser end part
        if (_end == null)
        {
            _end = Instantiate(_laserEnd, this.transform);
            _endSpriteRenderer = _end.GetComponentInChildren<SpriteRenderer>();
        }

        _startSpriteRenderer.DOFade(1, fadeInTime);
        _middleSpriteRenderer.DOFade(1, fadeInTime);
        _endSpriteRenderer.DOFade(1, fadeInTime);
        yield return new WaitForSeconds(fadeInTime);

        Managers.Sound.Play(_shootSfx);
        // Place the laser end at the end of the laser
        _end.transform.localPosition = new Vector2(currentLaserSize, 0);

        shootAction?.Invoke();
        hit.collider?.GetComponent<Health>()?.Damage(_damage);
        hit.collider?.GetComponent<UnitController>()?.OnHitUnit(_damage);

        _startSpriteRenderer.DOFade(0, fadeOutTime);
        _middleSpriteRenderer.DOFade(0, fadeOutTime);
        _endSpriteRenderer.DOFade(0, fadeOutTime);
        yield return new WaitForSeconds(fadeOutTime);
    }

}
