using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    public MonoBehaviour[] targets;

    public void ForwardEvent(string methodName)
    {
        foreach (var target in targets)
        {
            if (target == null) continue;

            var method = target.GetType().GetMethod(methodName);
            if (method != null)
                method.Invoke(target, null);
        }
    }
}