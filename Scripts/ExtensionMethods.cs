public static class ExtensionMethods
{
    public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }

    public static void SetFloatSmooth(this UnityEngine.Animator animator, string name, float endValue, float duration)
    {
        DG.Tweening.DOTween.To(() => animator.GetFloat(name), x => animator.SetFloat(name, x), endValue, duration);
    }
}
