using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class MachineGunTurrel : EnemyBase
{
    private float _radius = -1;

    public override float GetRadius()
    {
        if (_radius < 0)
            _radius = GetRadiusFromCollider();

        return _radius;
    }

    private CharacterBase _currentTarget;

    public float ShootRadius;
    public float ReloadTime;
    public float RotateSpeed;

    public int Damage;

    public Transform RotatableTower;

    public GameObject SfxShoot;
    public GameObject SfxHitTarget;

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
        if (_currentTarget != null)
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
        if (_currentTarget == null)
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
                Shoot();
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

    private void Shoot()
    {
        _reloading = true;
        _reloadTimer = ReloadTime;

        SoundManager.Play3DSound("machine_gun", transform.position, 4);

        SfxFactory.CreateSfx(SfxHitTarget, _currentTarget.transform.position + Vector3.up*0.5f, 1f);
        _currentTarget.CurrentHealth -= Damage;
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

        Gizmos.color = oldColor;
    }
}
