using RootMotion.FinalIK;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFightSettings", menuName = "ScriptableObjects/FightSettings", order = 1)]
public class FightSettings : ScriptableObject
{
    public FullBodyBipedEffector effector;
    public AnimationCurve weightCurve;
    public AnimationClip motion;


}
