using UnityEngine;

// Place this on the Animator object (Kangaroo_Character)
public class AnimationEventForwarder : MonoBehaviour
{
    public PlayerController controller;

    public void TriggerHopBounce()
    {
        if (controller != null)
            controller.TriggerHopBounce();
    }
}