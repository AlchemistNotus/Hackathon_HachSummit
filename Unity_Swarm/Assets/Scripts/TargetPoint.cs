using UnityEngine;
using System.Collections;

public class TargetPoint : MonoBehaviour
{
    public float Radius;
    private float _radiusSQR;

    public void Awake()
    {
        _radiusSQR = Radius * Radius;
    }

    void Update()
    {
        if (GameController.Instance.IsGameFinished())
            return;

        foreach (var character in GameController.GetAllControledCharacters())
        {
            if ((character.transform.position - transform.position).sqrMagnitude < _radiusSQR)
            {
                GameController.Instance.Victory();
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(transform.position, Radius);

        Gizmos.color = oldColor;
    }

}
