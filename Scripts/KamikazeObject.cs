using System.Collections;
using DG.Tweening;
using UnityEngine;
public class KamikazeObject : MonoBehaviour
{
    [SerializeField] private float _timeToDestroy;

    private void OnEnable()
    {
        StartCoroutine(Destroy());
    }

    private IEnumerator Destroy()
    {
        transform.DOPunchScale(Vector3.zero, _timeToDestroy);

        for (float i = 0; i < _timeToDestroy; i += 1.0f)
        {
            yield return new WaitForSeconds(1.0f);
        }
        
        Destroy(gameObject);
    }
}