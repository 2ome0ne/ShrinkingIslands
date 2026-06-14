using System;
using UnityEngine;

public class FadeCamera : MonoBehaviour
{
    public Animator FadeCameraAnimator;

    private void Start()
    {
        FadeIn();
    }

    public void FadeOut()
    {
        FadeCameraAnimator.SetTrigger("FadeOut");
    }

    public void FadeIn()
    {
        FadeCameraAnimator.SetTrigger("FadeIn");
    }
}
