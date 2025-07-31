using UnityEngine;

public class Enemy : MonoBehaviour, IHitable
{
    public void GetHit()
    {
        Debug.Log("Hit Enemy");
    }
}
