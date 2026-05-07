using System;
using UnityEngine;

public class DEV_LOCKED_ANIMATION : MonoBehaviour
{
    public AnimationClip animationClip; 
    [Range(0, 1)] public float normalizedTime = 0f;
    
    public Animator animator;
    [HideInInspector] public int selectedClipIndex = 0; 
    private string[] clipNames;

    [Button]
    private void LockAnimation()
    {
        if (animationClip != null)
        {
            float time = normalizedTime * animationClip.length;
            
            animationClip.SampleAnimation(gameObject, time);
            Debug.Log($"Animation locked at time: {time}s (normalized: {normalizedTime})");
        }
        else
        {
            Debug.LogWarning("No AnimationClip assigned!");
        }
    }

    private void OnValidate()
    {
        LockAnimation();
        
         if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            
            clipNames = new string[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                clipNames[i] = clips[i].name;
            }
        }
        else
        {
            clipNames = new string[0];
        }

        animationClip = GetSelectedClip();
    }
    
    public AnimationClip GetSelectedClip()
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

            if (selectedClipIndex >= 0 && selectedClipIndex < clips.Length)
            {
                return clips[selectedClipIndex]; 
            }
        }

        return null; 
    }
}
