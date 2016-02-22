using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
    public float Radius =2;

    public GameObject PrefabMiniBug;
    public GameObject PrefabMidBug;
    public GameObject PrefabBigBug;

    public void Awake()
    {
        foreach (var savedBug in SessionStats.Instance.SavedBugs)
        {
            var random = Random.insideUnitSphere* Radius;
            random.y = 0;
            CharacterBase ch;
            if (savedBug == typeof(MiniBug))
            {
               GameObject go = (GameObject) Instantiate(PrefabMiniBug, transform.position + random, Quaternion.identity);
                go.GetComponent<CharacterBase>().CurrentState = CharacterBase.CharacterState.Possessed;
            }
            if (savedBug == typeof(MidBug))
            {
                GameObject go = (GameObject)Instantiate(PrefabMidBug, transform.position + random, Quaternion.identity);
                go.GetComponent<CharacterBase>().CurrentState = CharacterBase.CharacterState.Possessed;
            }
            if (savedBug == typeof(BigBug))
            {
                GameObject go = (GameObject)Instantiate(PrefabBigBug, transform.position + random, Quaternion.identity);
                go.GetComponent<CharacterBase>().CurrentState = CharacterBase.CharacterState.Possessed;
            }
        }

        var fgo = (GameObject)Instantiate(PrefabMiniBug, transform.position, Quaternion.identity);
        fgo.GetComponent<CharacterBase>().CurrentState = CharacterBase.CharacterState.Possessed;
    }

    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(transform.position, Radius);

        Gizmos.color = oldColor;
    }
}
