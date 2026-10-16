using Leopotam.EcsLite;
using UnityEngine;

public class ConfigDebugSystem : IEcsRunSystem
{
    private BusinessConfig _businessConfig;

    public ConfigDebugSystem(BusinessConfig businessConfig)
    {
        _businessConfig = businessConfig;
    }

    public void Run(IEcsSystems systems)
    {
        Debug.Log(_businessConfig.Businesses[0].BaseCost);
    }
}
