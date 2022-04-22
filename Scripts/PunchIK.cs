using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;

public class PunchIK : MonoBehaviour
{
    [SerializeField] private string _animationBundle;
    [SerializeField] private string _fightSettingsName;
    [SerializeField] private FightSettings _fightSettings;
    public TestRagdollForce trf;

    public float _weight;
    private FullBodyBipedIK _ik;
    private Animator _animator;
    private AnimationLoadManager _animRemoteLoader;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _ik = GetComponent<FullBodyBipedIK>();
        _animRemoteLoader = new AnimationLoadManager(_animator);
    }


    public float GetCurveWeightValue(string animationState)
    {
        if (_animator == null) return 0;

        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        return _fightSettings.weightCurve.Evaluate(Mathf.Repeat(info.normalizedTime, 1f));
    }

    public IEnumerator Attack(Transform target)
    {
        yield return _animRemoteLoader.LoadAnimClip(_animationBundle, _fightSettingsName, (x) => _fightSettings = x);
        _animator.SetTrigger("Attack");
        var info = _animator.GetCurrentAnimatorStateInfo(0);

        yield return new WaitUntil(() => _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == _fightSettings.motion.name);
        while (_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == _fightSettings.motion.name)
        {
            _weight = GetCurveWeightValue(_fightSettings.motion.name);
            if(_weight>=0.98f)
                trf.ApplyForce();
            _ik.solver.GetEffector(_fightSettings.effector).position = target.position;
            _ik.solver.GetEffector(_fightSettings.effector).positionWeight = _weight;
            yield return new WaitForEndOfFrame();
        }

       
        //_animRemoteLoader.UnloadPreviousLoadAnimation();
    }

}