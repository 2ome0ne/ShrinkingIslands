using System;
using UnityEngine;

public class TutorialDetecter : MonoBehaviour
{
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private string Text;
    [SerializeField] private bool HasSteppedIn;
    private void OnTriggerStay(Collider other)
    {
        if (other.transform == tutorialManager.controller.transform && !HasSteppedIn)
        {
            HasSteppedIn = true;
            tutorialManager.SetTutorialText(Text);
        }
    }
}
