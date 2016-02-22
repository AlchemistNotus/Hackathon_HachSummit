using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class RocketTurrel : EnemyBase
{
    private float _radius = -1;

    public override float GetRadius()
    {
        if (_radius < 0)
            _radius = GetRadiusFromCollider();

        return _radius;
    }

    private CharacterBase _currentTarget;

    private GameObject _currentMark;

    public float ShootRadius;
    public float ReloadTime;
    public float RotateSpeed;
    public float ShootDelay;
    public float RocketSpeed;
    public float RocketSplashRadius;
    public int Damage;

    public GameObject RocketTargetPrefab;
    public GameObject BulletPrefab;
    public Transform BulletSpawnPoint;

    public Transform RotatableTower;


    private bool _stopRotate;
    private bool _reloading;
    private float _reloadTimer;

    void Update()
    {
        if (CurrentEnemyState == EnemyState.Death)
            return;

        if (_reloading)
        {
            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0)
            {
                _reloading = false;
            }
        }

        LookTargetsAndFire();
        RotateTowerToTarget();
    }

    private void RotateTowerToTarget()
    {
        if (!_stopRotate && _currentTarget != null)
        {
            float rotationDirection = GetRotateToTarget(_currentTarget.transform, RotatableTower);
            Vector3 currentRotateVector = RotatableTower.rotation.eulerAngles;

            float rotate = RotateSpeed * Time.deltaTime * Mathf.Sign(rotationDirection);

            if (Mathf.Abs(rotate) > Mathf.Abs(rotationDirection))
                rotate = rotationDirection;

            currentRotateVector.y += rotate;
            RotatableTower.rotation = Quaternion.Euler(currentRotateVector);
        }
    }


    void LookTargetsAndFire()
    {
        if (_currentTarget != null && _currentTarget.CurrentState != CharacterBase.CharacterState.Possessed)
            _currentTarget = null;

        // Look for target
        while (_currentTarget == null)
        {
            CharacterBase character = FindClosestCharacter();
            if (character != null && IsInShootDistance(character.transform, ShootRadius))
            {
                _currentTarget = character;
            }
            else
            {
                return;
            }
        }

        // Shoot target
        if (CanShootInCurrentTarget())
        {
            if (IsReloaded() && IsRotated())
            {
                StartCoroutine("Shoot");
            }
        }
        else
        {
            _currentTarget = null;
        }

    }

    private bool IsRotated()
    {
        if (_currentTarget == null)
            return false;

        float neededRotation = GetRotateToTarget(_currentTarget.transform, RotatableTower);
        return Mathf.Abs(neededRotation) < 1;
    }

    private bool IsReloaded()
    {
        return !_reloading;
    }

    private IEnumerator Shoot()
    {
        _stopRotate = true;
        _reloading = true;
        _reloadTimer = ReloadTime;

        Vector3 targetPosition = _currentTarget.transform.position;
        _currentMark = Instantiate(RocketTargetPrefab);
        _currentMark.transform.localScale = Vector3.one;
        _currentMark.transform.position = targetPosition;

        yield return new WaitForSeconds(ShootDelay);

        _stopRotate = false;

        GameObject rocket = Instantiate(BulletPrefab);
        rocket.transform.localScale = Vector3.one;
        if (BulletSpawnPoint != null)
        {
            rocket.transform.position = BulletSpawnPoint.position;
        }
        else
        {
            rocket.transform.position = transform.position;
        }
        rocket.transform.LookAt(targetPosition);

        Vector3 middlePosition = (targetPosition + transform.position)/2f;
        middlePosition.y += middlePosition.magnitude / 10;

        SoundManager.Play3DSound("rocket_launch", rocket.transform.position, 4);
        rocket.transform.DOPath(new []{ middlePosition , targetPosition }, RocketSpeed, PathType.CatmullRom)
            .SetEase(Ease.InQuad)
            .SetSpeedBased().OnComplete(() => BulletHittedEnemy(rocket));
    }

    void BulletHittedEnemy(GameObject bullet)
    {
        SoundManager.Play3DSound("rocket_explosion", bullet.transform.position, 4);
        DamageCharactersInRadius(bullet.transform.position, RocketSplashRadius, Damage, true);
        if (_currentMark != null)
            Destroy(_currentMark);
        Destroy(bullet);
    }

    protected override void OnDie()
    {
        if (_currentMark != null)
            Destroy(_currentMark);
        StopCoroutine("Shoot");
    }

    bool CanShootInCurrentTarget()
    {
        return (_currentTarget != null && IsInShootDistance(_currentTarget.transform, ShootRadius));
    }


    private void OnDrawGizmosSelected()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, ShootRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.left*3, RocketSplashRadius);

        Gizmos.color = oldColor;
    }
}
