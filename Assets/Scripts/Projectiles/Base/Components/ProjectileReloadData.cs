using Unity.Entities;

public struct ProjectileReloadData : IComponentData
{
    public float ReloadTime;
    public float CurrentReloadTime;
}
