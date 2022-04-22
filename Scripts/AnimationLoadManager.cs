using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class AnimationLoadManager
{
    private AnimatorOverrideController _overrideController;
    private static string _currentClipName = "AttackEmpty";
    private Animator _animator;
    private ResourceRequest _request;
    private AnimatorStateInfo[] _layerInfo;

    public AnimationLoadManager(Animator animator)
    {
        _animator = animator;
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);

    }

    public IEnumerator LoadAnimClip(string bundleName, string fightSettingsName, Action<FightSettings> result)
    {
        AssetBundle.UnloadAllAssetBundles(true);
        AssetBundle localAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName));

        if (localAssetBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            yield break;
        }

        FightSettings asset = localAssetBundle.LoadAsset<FightSettings>(fightSettingsName);

        _layerInfo = new AnimatorStateInfo[_animator.layerCount];
        for (int i = 0; i < _animator.layerCount; i++)
            _layerInfo[i] = _animator.GetCurrentAnimatorStateInfo(i);

        _overrideController[_currentClipName] = asset.motion;
        _animator.runtimeAnimatorController = _overrideController;

        for (int i = 0; i < _animator.layerCount; i++)
            _animator.Play(_layerInfo[i].nameHash, i, _layerInfo[i].normalizedTime);

        
        // Force an update
        _animator.Update(0.0f); 
        result(asset);
    }

    public void UnloadPreviousLoadAnimation()
    {
        for (int i = 0; i < _animator.layerCount; i++)
        {
            _layerInfo[i] = _animator.GetCurrentAnimatorStateInfo(i);
        }

        _overrideController[_currentClipName] = null;
        AssetBundle.UnloadAllAssetBundles(true);
        for (int i = 0; i < _animator.layerCount; i++)
        {
            _animator.Play(_layerInfo[i].nameHash, i, _layerInfo[i].normalizedTime);
        }
        _animator.runtimeAnimatorController = _overrideController;
        // Force an update
        _animator.Update(0.0f);
    }
}
