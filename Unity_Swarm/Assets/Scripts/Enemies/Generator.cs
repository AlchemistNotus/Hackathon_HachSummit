using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class Generator : EnemyBase
{
    public GameObject[] ObjectsToDestroy;

    private float _radius = -1;

    public override float GetRadius()
    {
        if (_radius < 0)
            _radius = GetRadiusFromCollider();

        return _radius;
    }

    protected override void OnDie()
    {
        base.OnDie();

        foreach (GameObject o in ObjectsToDestroy)
        {
            if (o == null)
                continue;
            EnemyBase enemy = o.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.CurrentHealth = 0;
            }
            else
            {
                Destroy(o);
            }
        }
    }
}
