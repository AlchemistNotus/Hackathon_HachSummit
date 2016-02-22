using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LazerTurrel : EnemyBase
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

    public Transform RotatableTower;

    public float ShootRadius;
    public float RotateSpeed;
    public float ReloadTime;
    public float DamageWidth;
    public float DamageDistance;

    public float ShootDelay;

    public int Damage;

    public GameObject TargetPrefab;
    public GameObject SfxShoot;

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

    bool CanShootInCurrentTarget()
    {
        return (_currentTarget != null && IsInShootDistance(_currentTarget.transform, ShootRadius));
    }


    private IEnumerator Shoot()
    {
        _reloading = true;
        _reloadTimer = ReloadTime;
        _stopRotate = true;

        _currentMark = Instantiate(TargetPrefab);
        _currentMark.transform.localScale = Vector3.one;
        _currentMark.transform.position = transform.position;
        _currentMark.transform.rotation = RotatableTower.rotation;

        SoundManager.Play3DSound("tesla", transform.position, 4);

        yield return new WaitForSeconds(ShootDelay);

        GameObject ShootSfx = Instantiate(SfxShoot);
        ShootSfx.transform.localScale = Vector3.one;
        ShootSfx.transform.position = transform.position;
        ShootSfx.transform.rotation = RotatableTower.rotation;
        Destroy(ShootSfx, 1.5f);

        _stopRotate = false;

        DamageCharacters(RotatableTower, DamageWidth, DamageDistance);
        Destroy(_currentMark);
    }

    protected void DamageCharacters(Transform baseTransform, float width, float lenght)
    {
        List<CharacterBase> damageList = new List<CharacterBase>();

        var targets = GameController.GetAllControledCharacters();
        foreach (var character in targets)
        {
            Vector3 relativePosition = baseTransform.InverseTransformPoint(character.transform.position);


            if ((relativePosition.z < lenght && relativePosition.z > 0) &&
                (relativePosition.x < (width/2) && relativePosition.x > (-width / 2)) )
            {
                damageList.Add(character);
            }
        }

        foreach (CharacterBase characterBase in damageList)
        {
            characterBase.CurrentHealth -= Damage;
        }
    }

    protected override void OnDie()
    {
        if (_currentMark != null)
            Destroy(_currentMark);
        StopCoroutine("Shoot");
    }

    private void OnDrawGizmosSelected()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, ShootRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DamageWidth);

        Gizmos.color = oldColor;
    }
}
