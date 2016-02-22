using UnityEngine;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class WallDestroable : EnemyBase
{
    private float _radius = -1;

    public override float GetRadius()
    {
        if (_radius < 0)
            _radius = GetRadiusFromCollider();

        return _radius;
    }

}
