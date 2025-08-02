using UnityEngine;

public interface IHitable
{
    public void GetHit(int damage);
}

public interface IHitBlocker
{
    bool ShouldBlockHit();
}
