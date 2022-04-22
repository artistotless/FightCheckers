using DG.Tweening;
using System.Collections;
using UnityEngine;

public enum FigureColor { Empty = 0, White = 1, Black = 2 }
public class Figure : MonoBehaviour
{
    public bool isKing;
    public FigureColor color;
    public Material material;

    private Animator _personAnimator;
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    private const float _maxRaycastDistance = 100f;
    private Vector3 target;

    private void Awake()
    {
        _personAnimator = GetComponent<Animator>();
        _meshRenderer = _meshRenderer == null ? GetComponent<SkinnedMeshRenderer>() : _meshRenderer;
        material = _meshRenderer.sharedMaterial;
    }


    public IEnumerator Move(Vector3 pointPosition)
    {
        Debug.Log($"Position : {pointPosition}");
        target = pointPosition;
        bool startMoveWasEnd = false;
        float distance = Vector3.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pointPosition.x, pointPosition.z));

        if (distance > 0.3)
        {
            _personAnimator.Play("New State");
            startMoveWasEnd = true;
        }

        do
        {
            transform.DOLookAt(pointPosition, 0.5f);

            Vector3 targetDir = new Vector3(pointPosition.x, 0, pointPosition.z) - transform.position;
            distance = Vector3.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pointPosition.x, pointPosition.z));
            float angle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);
            float maxSpeed = (distance > 1.8f) ? 1f : (distance <= 1.8f) && startMoveWasEnd ? 1f : 0.9f;
            float speed = Mathf.Lerp(0, maxSpeed, Mathf.Clamp01(distance));

            distance = distance <= 0.1f ? 0 : distance;
            angle = distance <= 0.3 ? 0 : angle;
            speed = distance == 0f || speed <= 0.1 ? 0 : speed;
            _personAnimator.SetFloat("Speed", speed);

            if (distance == 0)
                break;

            yield return null;
        }

        while (distance > 0);
    }

    public void SetColor(FigureColor color, Material colorMaterial)
    {
        this.color = color;
        this.material = colorMaterial;
        this._meshRenderer.sharedMaterial = colorMaterial;
    }

    public void Attack()
    {

    }

    public void Die()
    {

    }
}
