using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class TeslaTurrel : EnemyBase
{
    private float _radius = -1;

    public override float GetRadius()
    {
        if (_radius < 0)
            _radius = GetRadiusFromCollider();

        return _radius;
    }

    private GameObject _currentMark;

    public float ShootRadius;
    public float ReloadTime;
    public float DamageRadius;

    public float ShootDelay;

    public int Damage;

    public GameObject TargetPrefab;
    public GameObject SfxShoot;

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
    }

    void LookTargetsAndFire()
    {
        if (!IsReloaded())
            return;

        // Look for target
        CharacterBase character = FindClosestCharacter();
        if (character != null && IsInShootDistance(character.transform, ShootRadius))
        {
            StartCoroutine("Shoot");
        }
    }

    private bool IsReloaded()
    {
        return !_reloading;
    }

    private IEnumerator Shoot()
    {
        _reloading = true;
        _reloadTimer = ReloadTime;

        _currentMark = Instantiate(TargetPrefab);
        _currentMark.transform.localScale = Vector3.one;
        _currentMark.transform.position = transform.position;

        SoundManager.Play3DSound("tesla", transform.position, 4);
        yield return new WaitForSeconds(ShootDelay-0.5f);
        SfxFactory.CreateSfx(SfxShoot, transform.position, 1.5f);
        yield return new WaitForSeconds(0.5f);
        

        DamageCharactersInRadius(transform.position, DamageRadius, Damage, true);
        Destroy(_currentMark);
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
        Gizmos.DrawWireSphere(transform.position, DamageRadius);

        Gizmos.color = oldColor;
    }
}
