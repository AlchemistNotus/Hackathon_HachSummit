using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class EnemyBase : MonoBehaviour, IPointerClickHandler
{
    public static List<EnemyBase> AllEnemy = new List<EnemyBase>();

    public enum EnemyState
    {
        None,
        Death
    }

    public EnemyState CurrentEnemyState = EnemyState.None;

    public abstract float GetRadius();

    private float _maxHealth = 200;
    private float _currentHealth = 200;

    public GameObject SfxDeath;
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

            if (_currentHealth <= 0 && CurrentEnemyState != EnemyState.Death)
            {
                SoundManager.Play3DSound("tower_breaking", transform.position, 4);
                Destroy(gameObject);
                CurrentEnemyState = EnemyState.Death;
                OnDie();
                AllEnemy.Remove(this);
                SfxFactory.CreateSfx(SfxDeath, transform.position, 4f);
            }
        }
    }

    protected virtual void OnDie()
    {
        
    }

    void Reset()
    {
        HealthSlider = GetComponentsInChildren<Image>().FirstOrDefault(image => image.gameObject.name == "Fill");
    }

    public void OnEnable()
    {
        AllEnemy.Add(this);

        var go = Instantiate(PrefabUi);
        go.transform.SetParent(GameCanvas.Instance.transform,false);
        HealthBar = (RectTransform)go.transform;
        HealthSlider = HealthBar.GetChild(0).GetComponent<Image>();
    }

    public void LateUpdate()
    {
        if (HealthBar != null)
        {
            HealthBar.anchoredPosition3D = RectTransformUtility.WorldToScreenPoint(GameCamera.Instance.Camera, transform.position)+ Vector2.up*30;
        }
    }

    public void OnDestroy()
    {
        AllEnemy.Remove(this);

        if (HealthBar != null) Destroy(HealthBar.gameObject);
    }

    protected float GetRadiusFromCollider()
    {
        float radius = 0;
        Collider childCollider = GetComponentInChildren<Collider>();
        if (childCollider != null)
        {
            radius = childCollider.bounds.size.x / 2f;
        }
        else
        {
            NavMeshObstacle nav = GetComponentInChildren<NavMeshObstacle>();
            if (nav != null)
                radius = nav.radius * transform.lossyScale.x;
        }
        return radius;
    }


    protected bool IsInShootDistance(Transform target, float shootDistance)
    {
        Vector3 relativePosition = target.position - transform.position;
        return relativePosition.magnitude < shootDistance;
    }

    protected CharacterBase FindClosestCharacter()
    {

        var targets = GameController.GetAllControledCharacters();

        Vector3 currentPosition = transform.position;
        CharacterBase closestEnemy = null;
        float distanceToClosestSqr = Mathf.Infinity;

        foreach (var character in targets)
        {
            Vector3 relativePosition = character.transform.position - currentPosition;
            if (distanceToClosestSqr > relativePosition.sqrMagnitude)
            {
                distanceToClosestSqr = relativePosition.sqrMagnitude;
                closestEnemy = character;
            }
        }

        return closestEnemy;
    }

    protected void DamageCharactersInRadius(Vector3 center, float radius, int damage, bool friendlyFire)
    {
        Vector3 explosionPosition = center;
        var targets = GameController.GetAllControledCharacters();

        var charactersToDamage = targets.FindAll(delegate(CharacterBase character)
        {
            return (character.transform.position - explosionPosition).magnitude <= radius;
        });
        foreach (var character in charactersToDamage)
        {
            character.CurrentHealth -= damage;
        }

        if (friendlyFire)
        {

            var friendsToDamage = EnemyBase.AllEnemy.FindAll(delegate(EnemyBase friend)
            {
                return (friend.transform.position - explosionPosition).magnitude <= radius;
            });


            foreach (var friend in friendsToDamage)
            {
                if (friend.gameObject == gameObject)
                    continue;

                friend.CurrentHealth -= damage;
            }
        }
    }

    protected float GetRotateToTarget(Transform target, Transform tower)
    {
        Vector3 relativeTargetPos = target.transform.position - tower.position;
        relativeTargetPos.y = 0;

        float targetRotate = Mathf.Atan2(relativeTargetPos.x, relativeTargetPos.z) * Mathf.Rad2Deg;
        Vector3 currentRotateVector = tower.rotation.eulerAngles;
        float currentRotation = currentRotateVector.y;

        return GetClosestRotateDirection(currentRotation, targetRotate);
    }

    float GetClosestRotateDirection(float currentRotation, float targetRotation)
    {
        float direction = targetRotation - currentRotation;
        while (direction > 180)
            direction -= 360;

        while (direction < -180)
            direction += 360;

        return direction;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameController.Instance.ClickedOnEnemy(this);
    }
}
