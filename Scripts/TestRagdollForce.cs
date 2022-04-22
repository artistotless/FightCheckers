using System.Collections.Generic;
using UnityEngine;


public class TestRagdollForce : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> _rigids;
    [SerializeField] private Vector3 _force;
    [SerializeField] private bool _applyAutomatically;
    [SerializeField] private Animator _animator;

    private void Start()
    {
        if (_applyAutomatically)
            ApplyForce();
    }

    public void ApplyForce()
    {
        _animator.enabled = false;
        foreach (Rigidbody rigid in _rigids)
            rigid.AddRelativeForce(_force, ForceMode.Impulse);
    }
}

