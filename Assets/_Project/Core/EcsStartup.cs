using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;
using System.IO;

public class EcsStartup : MonoBehaviour
{
    [SerializeField] private BusinessConfig _businessConfig;

    [SerializeField] private BusinessNamesConfig _businessNames;

    [SerializeField] private BalanceView _balanceView;

    [SerializeField] private BusinessView[] _views;

    private readonly List<BusinessPresenter> _presenters = new();
    private BalancePresenter _balancePresenter;

    private DataSaveService _saveService;

    private EcsWorld _world;
    private EcsSystems _systems;
    private int _balanceId;

    private void Awake()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");

        _saveService = new DataSaveService(path);

        GameSaveData loadedData = _saveService.Load();

        if (loadedData != null)
        {
            BusinessSaveData[] savedBusinesses = loadedData.BusinessDataSaves;

            bool isValid = savedBusinesses != null
                && _businessConfig != null
                && _businessConfig.Businesses != null
                && savedBusinesses.Length == _businessConfig.Businesses.Length;

            if (isValid)
            {
                for (int i = 0; i < savedBusinesses.Length; i++)
                {
                    if (savedBusinesses[i] == null ||
                        savedBusinesses[i].BusinessId != i ||
                        savedBusinesses[i].BusinessLevel < 0 ||
                        savedBusinesses[i].IncomeProgress < 0 ||
                        savedBusinesses[i].IncomeProgress >= _businessConfig.Businesses[i].IncomeLatency)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (!isValid || loadedData.Balance < 0)
            {
                Debug.LogWarning("Invalid save data. Starting a new game.");
                loadedData = null;
            }
        }

        if (_views == null || _views.Length != _businessConfig.Businesses.Length)
        {
            Debug.LogError("Business views count does not match configuration.");
            return;
        }
        else if (_businessNames == null || _views.Length != _businessNames.BusinessNames.Length)
        {
            Debug.LogError("Business names count does not match configuration.");
            return;
        }

        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

        EcsPool<BusinessComponent> businessPool = _world.GetPool<BusinessComponent>();
        EcsPool<BalanceComponent> balancePool = _world.GetPool<BalanceComponent>();
        EcsPool<PurchaseRequest> requestPool = _world.GetPool<PurchaseRequest>();

        _balanceId = _world.NewEntity();
        balancePool.Add(_balanceId);

        if (loadedData != null)
        {
            balancePool.Get(_balanceId).Value = loadedData.Balance;
        }

        for (int i = 0; i< _businessConfig.Businesses.Length; i++)
        {
            int entity = _world.NewEntity();
            ref BusinessComponent addedEntityBusiness = ref businessPool.Add(entity);
            addedEntityBusiness.BusinessId = i;
            BusinessPresenter presenter = new(_views[i],entity, requestPool,businessPool, _businessConfig.Businesses[i], _businessNames.BusinessNames[i]);
            _presenters.Add(presenter);
            presenter.Subscribe();

            if (loadedData == null)
            {
                if (i == 0)
                {
                    addedEntityBusiness.Level = 1;
                }
            }
            else
            {
                BusinessSaveData savedData = loadedData.BusinessDataSaves[i];

                addedEntityBusiness.Level = savedData.BusinessLevel;
                addedEntityBusiness.IncomeProgress = savedData.IncomeProgress;
                addedEntityBusiness.FirstUpgradeBought = savedData.FirstUpgradeBought;
                addedEntityBusiness.SecondUpgradeBought = savedData.SecondUpgradeBought;
            }
        }
        BalancePresenter balancePresenter = new(_balanceView, _balanceId, balancePool);
        _balancePresenter = balancePresenter;


        IncomeSystem incomeSystem = new(_businessConfig, _balanceId);
        PurchaseSystem purchaseSystem = new(_businessConfig, _balanceId);

        _systems.Add(incomeSystem);
        _systems.Add(purchaseSystem);
        
        _systems.Init();
    }

    private void Update()
    {
        if (_systems == null)
        {
            return;
        }

        _systems.Run();

        _balancePresenter.UpdateBalance();

        foreach (BusinessPresenter presenter in _presenters)
        {
            presenter.UpdateView();
        }
    }

    private void OnDestroy()
    {
        Debug.Log(Application.persistentDataPath);
        if (_systems == null)
        {
            return;
        }

        _saveService.Save(CreateSaveData());

        foreach (BusinessPresenter presenter in _presenters)
        {
            presenter.Unsubscribe();
        }

        _systems.Destroy();
        _world.Destroy();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && _systems != null)
        {
            _saveService.Save(CreateSaveData());
        }
    }

    private GameSaveData CreateSaveData()
    {
        GameSaveData data = new GameSaveData();

        EcsPool<BusinessComponent> businessPool = _world.GetPool<BusinessComponent>();
        EcsPool<BalanceComponent> balancePool = _world.GetPool<BalanceComponent>();
        EcsFilter filter = _world.Filter<BusinessComponent>().End();

        data.Balance = balancePool.Get(_balanceId).Value;

        data.BusinessDataSaves = new BusinessSaveData[_businessConfig.Businesses.Length];

        foreach (int entity in filter)
        {
            BusinessSaveData businessData = new();
            BusinessComponent component = businessPool.Get(entity);

            data.BusinessDataSaves[component.BusinessId] = businessData;
            data.BusinessDataSaves[component.BusinessId].BusinessId = component.BusinessId;
            data.BusinessDataSaves[component.BusinessId].BusinessLevel = component.Level;
            data.BusinessDataSaves[component.BusinessId].IncomeProgress= component.IncomeProgress;
            data.BusinessDataSaves[component.BusinessId].FirstUpgradeBought= component.FirstUpgradeBought;
            data.BusinessDataSaves[component.BusinessId].SecondUpgradeBought= component.SecondUpgradeBought;
        }

        return data;
    }
}
