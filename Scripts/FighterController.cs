using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FighterController : MonoBehaviour
{
    [SerializeField] private PunchIK _punchIK;
    private Animator _personAnimator;


    private void Start()
    {
        _personAnimator = GetComponent<Animator>();
        MouseEventService.Instance.enemyClicked += Attack;
    }

    public void Attack(Transform enemy)
    {
        StartCoroutine(_punchIK.Attack(enemy));
    }

    private void OnDestroy()
    {
        MouseEventService.Instance.enemyClicked -= Attack;
 
    }
}
