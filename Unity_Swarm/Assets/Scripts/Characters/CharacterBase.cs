using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public abstract class CharacterBase : MonoBehaviour
{
    public static List<CharacterBase> AllCharacter = new List<CharacterBase>();

    public enum CharacterState
    {
        None,
        Prisoned,
        Possessed,
        Death
    }

    public CharacterState CurrentState = CharacterState.None;


    public enum AnimationState
    {
        Idle,
        Attack,
        Run
    }

    public AnimationState CurrentAnimationState = AnimationState.Idle;

    protected float _maxHealth = 10;
    private float _currentHealth = 10;
    private Vector3 _destination;

    public EnemyBase _enemy;

    public GameObject SfxDeath;

    public EnemyBase Prison;


    public Image HealthSlider;
    public RectTransform HealthBar;
    public GameObject PrefabUi;

    public float CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
            if (HealthSlider != null)
                HealthSlider.fillAmount = _currentHealth / _maxHealth;
            if (_currentHealth <= 0 && CurrentState != CharacterState.Death)
            {
                SoundManager.Play3DSound("bug_death", transform.position, 8);
                CurrentState = CharacterState.Death;
                AllCharacter.Remove(this);
                Destroy(gameObject);
                Destroy(HealthBar.gameObject);
                SfxFactory.CreateSfx(SfxDeath, transform.position + Vector3.up * 0.5f, 1.5f);
            }
            if (HealthSlider.fillAmount < 0.99)
            {
                HealthBar.gameObject.SetActive(true);
            }
        }
    }

    public NavMeshAgent NavMeshAgent;

    public Vector3 Destination
    {
        get { return _destination; }
        set
        {

            Vector3 random = Random.insideUnitSphere * 1.5f;
            random.y = 0;
            _destination = value + random;
            NavMeshAgent.SetDestination(_destination);
            _enemy = null;
            PlayAnimation(AnimationState.Run);
        }
    }

    public EnemyBase Enemy
    {
        get { return _enemy; }
        set
        {
            _enemy = value;
            Vector3 random = Random.insideUnitSphere * 1;
            random.y = 0;
            NavMeshAgent.SetDestination(_enemy.transform.position);
            NavMeshAgent.radius = 0.1f;
        }
    }

    public Animator Animator;
    public float MyDamage;
    private float _storeRadius;

    public virtual void OnEnable()
    {
        if (Prison != null && Prison.CurrentEnemyState != EnemyBase.EnemyState.Death)
        {
            CurrentState = CharacterState.Prisoned;
            NavMeshAgent.enabled = false;
        }
        AllCharacter.Add(this);
        _storeRadius = NavMeshAgent.radius;

        var go = Instantiate(PrefabUi);
        go.transform.SetParent(GameCanvas.Instance.transform,false);
        HealthBar = (RectTransform) go.transform;
        HealthSlider = HealthBar.GetChild(0).GetComponent<Image>();
        go.SetActive(false);
    }

    public void OnDestroy()
    {
        AllCharacter.Remove(this);
    }

    public void LateUpdate()
    {
        if (HealthBar != null)
        {
            HealthBar.anchoredPosition3D = RectTransformUtility.WorldToScreenPoint(GameCamera.Instance.Camera, transform.position) + Vector2.up * 30;
        }
    }

    public void Update()
    {
        

        if ((Prison == null || Prison.CurrentEnemyState == EnemyBase.EnemyState.Death) &&
            CurrentState == CharacterState.Prisoned)
        {
            CurrentState = CharacterState.None;
            NavMeshAgent.enabled = true;
        }

        if (Enemy != null && Enemy.CurrentEnemyState != EnemyBase.EnemyState.Death && CurrentAnimationState != AnimationState.Attack)
        {
            var minDis = NavMeshAgent.radius * transform.lossyScale.x + Enemy.GetRadius();
            var curDis = Vector3.Distance(transform.position, Enemy.transform.position);
            if (curDis <= (minDis + 1f))
            {
                PlayAnimation(AnimationState.Attack);
            }
        }

        if (Enemy == null && CurrentAnimationState == AnimationState.Run &&
            Vector3.Distance(transform.position, Destination) <= NavMeshAgent.stoppingDistance)
        {
            ReachedDestination();
        }

        //if (CurrentAnimationState == AnimationState.Run)
        //{
        //    Vector3 targetPosition = Vector3.zero;
        //    var allControlledCharacters = GameController.GetAllControledCharacters();
        //    if (allControlledCharacters.Count > 0)
        //    {
        //        foreach (var controledCharacter in allControlledCharacters)
        //        {
        //            targetPosition += controledCharacter.transform.position;
        //        }

        //        targetPosition /= allControlledCharacters.Count;
        //    }

        //    if ( Vector3.Distance(targetPosition, Destination) < 3 && 
        //        Vector3.Distance(transform.position, Destination)<4)
        //        ReachedDestination();
        //}


        if (CurrentAnimationState == AnimationState.Attack && Enemy != null)
        {
            transform.LookAt(Enemy.transform.position);
        }

        if (CurrentAnimationState == AnimationState.Attack && (Enemy == null || Enemy.CurrentEnemyState == EnemyBase.EnemyState.Death))
        {
            ReachedDestination();
        }
    }

    public void ReachedDestination()
    {
        EnemyBase enemy = null;
        float minDis = Settings.Instance.FindEnemyDistance;
        foreach (var enemyBase in EnemyBase.AllEnemy)
        {
            var dis = Vector3.Distance(enemyBase.transform.position, transform.position);
            if (dis < minDis)
            {
                enemy = enemyBase;
                minDis = dis;
            }
        }

        if (enemy != null)
        {
            Enemy = enemy;
        }
        else
        {
            PlayAnimation(AnimationState.Idle);
        }

        var countCurrent = GameController.GetAllControledCharacters();
        var currentAdd = 0;
        foreach (var characterBase in AllCharacter)
        {
            if (characterBase.CurrentState == CharacterState.None && (countCurrent.Count + currentAdd) < Settings.Instance.MaxSquad)
            {
                if (Vector3.Distance(transform.position, characterBase.transform.position) < 2f)
                {
                    characterBase.CurrentState = CharacterState.Possessed;
                    characterBase.Destination = Destination;
                }
            }
        }
    }


    public void PlayAnimation(AnimationState anim)
    {
        if (CurrentAnimationState != anim)
        {
            switch (anim)
            {
                case AnimationState.Idle:
                    NavMeshAgent.Stop();
                    Animator.SetTrigger("Idle");
                    break;
                case AnimationState.Attack:
                    NavMeshAgent.Stop();
                    Animator.SetTrigger("Attack");
                    break;
                case AnimationState.Run:
                    NavMeshAgent.Resume();
                    NavMeshAgent.radius = _storeRadius;
                    Animator.SetTrigger("Run");
                    break;
            }
            CurrentAnimationState = anim;

            if (CurrentAnimationState == AnimationState.Run)
            {
                StartCoroutine("WalkSounds");
            }
            else
            {
                StopCoroutine("WalkSounds");
            }
        }
    }

    IEnumerator WalkSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0, 1));
            SoundManager.Play3DSound("bug_walk", transform.position, 4);
            yield return new WaitForSeconds(1f);
        }
    }

    public abstract void CallbackAnimation(string command);
}