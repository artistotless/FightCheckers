using System.Collections;
using UnityEngine;

public class WayPointMover : MonoBehaviour
{
    [SerializeField] private RMCharacterController _controller;
    [SerializeField] private Transform _wayPointsParent;

    private IEnumerator Start()
    {
        for (int i = 0; i < _wayPointsParent.childCount; i++)
            if (_wayPointsParent.GetChild(i).gameObject.activeInHierarchy)
                yield return _controller.MoveToTarget(_wayPointsParent.GetChild(i).position);
    }
}
