using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class RMCharacterController : MonoBehaviour
{

    [SerializeField] private AnimationCurve[] _animTransitionCurve;
    [SerializeField] private float minDistanceForAttraction = 0.1f;

    private Transform _person;
    private Animator _personAnimator;
    private Vector3 _targetPosition;

    private const string _TURN_DIRECTION = "TurnDirection";
    private const string _SPEED = "Speed";

    [Header("Debug")]
    [SerializeField] private bool _gizmosEnable;
    [SerializeField] private float _distance, _currentDistance;
    [SerializeField] private KamikazeObject _debugPoint;

    private void Start()
    {
        _person = gameObject.transform;
        _personAnimator = _person.GetComponent<Animator>();
    }

    public void TryMoveToTarget(Vector3 targetPosition, UnityAction callback = null)
    {
        StartCoroutine(MoveToTarget(targetPosition, callback));
    }

    public IEnumerator MoveToTarget(Vector3 targetPosition, UnityAction callback = null)
    {
        _targetPosition = targetPosition;
        _distance = Vector3.Distance(new Vector3(_personAnimator.transform.position.x, 0, _personAnimator.transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        _currentDistance = _distance;
        Instantiate(_debugPoint, targetPosition, Quaternion.identity);
        yield return Rotate();
        yield return Move();
        Debug.Log($"Coroutine is done");
        callback?.Invoke();
    }

    private IEnumerator Rotate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(_targetPosition.x, 0, _targetPosition.z) - new Vector3(_personAnimator.transform.position.x, 0, _personAnimator.transform.position.z));
        float angle = GetSignedAngle(_targetPosition);
        float turnDirection = 1 / Mathf.Round(Mathf.Clamp(180 / angle, -2.0f, 2.0f));
        _personAnimator.SetFloatSmooth(_TURN_DIRECTION, turnDirection, 0.1f);
        yield return new WaitWhile(() => Mathf.Abs(GetSignedAngle(_targetPosition)) > 7);
        _personAnimator.SetFloatSmooth(_TURN_DIRECTION, 0, 0.2f);
        yield return new WaitWhile(() => _personAnimator.GetFloat(_TURN_DIRECTION) != 0.0f);
    }

    private float GetSignedAngle(Vector3 target)
    {
        return Vector3.SignedAngle(_personAnimator.transform.forward, (target - _personAnimator.transform.position), Vector3.up);
    }

    private IEnumerator Move()
    {
        AnimationCurve speedCurve = _distance > 2.5f ? _animTransitionCurve[1] : _animTransitionCurve[0];
        _personAnimator.SetFloatSmooth(_SPEED, speedCurve.Evaluate(0), 0.3f);
        while (_currentDistance > minDistanceForAttraction)
        {
            yield return new WaitWhile(() => _personAnimator.GetFloat(_SPEED) < speedCurve.Evaluate(0));
            float step = 1 - Mathf.Clamp01(_currentDistance.Remap(0.1f, _distance, 0, 1));

            if (step < 0.9)
                _personAnimator.transform.DOLookAt(_targetPosition, 0.3f);
            _personAnimator.SetFloat(_SPEED, speedCurve.Evaluate(step));
            _currentDistance = Vector3.Distance(
                new Vector3(_personAnimator.transform.position.x, 0, _personAnimator.transform.position.z),
                new Vector3(_targetPosition.x, 0, _targetPosition.z));
            yield return new WaitForFixedUpdate();
        }
        _personAnimator.SetFloatSmooth(_SPEED, 0, 0.2f);
        _personAnimator.transform.DOMove(_targetPosition, 0.2f);
        yield return new WaitWhile(() => _personAnimator.GetFloat(_SPEED) > 0 || _personAnimator.transform.position != _targetPosition);
    }

    private void OnDrawGizmos()
    {
        if (!_gizmosEnable || _targetPosition == null) return;

        DrawLine(transform.position + transform.forward, new Vector3(_targetPosition.x, 0, _targetPosition.z), Color.yellow);
        DrawLine(transform.position, new Vector3(transform.position.x, 0, transform.position.z) + new Vector3(transform.forward.x * 3, 0, transform.forward.z * 3), Color.green);
        DrawSphere(transform.position, 0.15f, Color.red);
    }

    private void DrawLine(Vector3 from, Vector3 to, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(from, to);
    }

    private void DrawSphere(Vector3 center, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(center, radius);
    }
}

