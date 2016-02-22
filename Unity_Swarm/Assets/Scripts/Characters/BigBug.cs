using UnityEngine;
using System.Collections;

public class BigBug : CharacterBase
{
    public GameObject SfxBite;
    public GameObject SfxClaw;

    public override void CallbackAnimation(string command)
    {
        switch (command)
        {
            case "damage":
                if (Enemy != null)
                {
                    SoundManager.Play3DSound("bug_eating", transform.position, 2);
                    Enemy.CurrentHealth -= MyDamage;
                    SfxFactory.CreateSfx(Random.Range(0, 1f) > 0.5f ? SfxBite : SfxClaw, Enemy.transform.position + (transform.position - Enemy.transform.position).normalized * Enemy.GetRadius() + Vector3.up * 0.5f, Random.insideUnitSphere * 30, 0.5f);
                }
                break;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _maxHealth = Settings.Instance.BigBugHealth;
        CurrentHealth = _maxHealth;
        MyDamage = Settings.Instance.BigBugDamage;
        NavMeshAgent.speed = Settings.Instance.BigBugSpeed;
    }
}