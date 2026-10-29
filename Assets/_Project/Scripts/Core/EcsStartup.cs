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


    private EcsPool<BalanceChanged> _balanceChangedPool;
    private EcsPool<BusinessChanged> _businessChangedPool;


    private EcsWorld _world;
    private EcsSystems _systems;
    private int _balanceId;

    private void Awake()
    {
        if (!GameConfigValidator.IsConfigurationValid(
        _businessConfig, _businessNames, _balanceView, _views) ||
    !GameConfigValidator.IsViewValid(_views) ||
    !GameConfigValidator.IsDataValid(_businessConfig) ||
    !GameConfigValidator.IsNameValid(_businessNames))
        {
            return;
        }

        Dictionary<int, BusinessView> viewsById = new();

        Dictionary<int, BusinessData> businessDataById = new();

        Dictionary<int, BusinessNames> businessNamesById = new();

        foreach (BusinessView view in _views)
        {
            viewsById.Add(view.BusinessId, view);
        }

        foreach (BusinessData data in _businessConfig.Businesses)
        {
            businessDataById.Add(data.BusinessId, data);
        }

        foreach (BusinessNames name in _businessNames.BusinessNames)
        {
            businessNamesById.Add(name.BusinessId, name);
        }

        if (!GameConfigValidator.AreIdsMatching(viewsById,businessDataById,businessNamesById))
        {
            return; 
        }

        string path = Path.Combine(Application.persistentDataPath, "save.json");

        _saveService = new DataSaveService(path);

        GameSaveData loadedData = _saveService.Load();

        if (loadedData != null &&
            !SaveDataValidator.IsValid(loadedData, businessDataById))
        {
            Debug.LogWarning("Invalid save data. Starting a new game.");
            loadedData = null;
        }

        _world = new EcsWorld();
        _systems = new EcsSystems(_world);

        EcsPool<BusinessComponent> businessPool = _world.GetPool<BusinessComponent>();
        EcsPool<BalanceComponent> balancePool = _world.GetPool<BalanceComponent>();
        EcsPool<PurchaseRequest> requestPool = _world.GetPool<PurchaseRequest>();
        _balanceChangedPool = _world.GetPool<BalanceChanged>();
        _businessChangedPool = _world.GetPool<BusinessChanged>();

        _balanceId = _world.NewEntity();
        balancePool.Add(_balanceId);

        Dictionary<int, BusinessSaveData> savedBusinessesById = new();

        if (loadedData != null)
        {
            balancePool.Get(_balanceId).Value = loadedData.Balance;

            foreach (BusinessSaveData savedBusiness in loadedData.BusinessDataSaves)
            {
                savedBusinessesById.Add(savedBusiness.BusinessId, savedBusiness);
            }
        }

        foreach (BusinessData data in _businessConfig.Businesses)
        {
            int businessId = data.BusinessId;

            int entity = _world.NewEntity();

            ref BusinessComponent business = ref businessPool.Add(entity);
            business.BusinessId = businessId;

            BusinessPresenter presenter = new(
                viewsById[businessId],
                entity,
                requestPool,
                businessPool,
                businessDataById[businessId],
                businessNamesById[businessId]
            );

            _presenters.Add(presenter);
            presenter.Subscribe();

            if (loadedData == null)
            {
                if (businessId == 1)
                {
                    business.Level = 1;
                }
            }
            else
            {
                BusinessSaveData savedData = savedBusinessesById[businessId];

                business.Level = savedData.BusinessLevel;
                business.IncomeProgress = savedData.IncomeProgress;
                business.FirstUpgradeBought = savedData.FirstUpgradeBought;
                business.SecondUpgradeBought = savedData.SecondUpgradeBought;
            }
        }

        _balancePresenter = new BalancePresenter(_balanceView, _balanceId, balancePool);


        IncomeSystem incomeSystem = new(businessDataById, _balanceId);
        PurchaseSystem purchaseSystem = new(businessDataById, _balanceId);

        _systems.Add(incomeSystem);
        _systems.Add(purchaseSystem);
        
        _systems.Init();

        _balancePresenter.UpdateBalance();

        foreach (BusinessPresenter presenter in _presenters)
        {
            presenter.InitializeView();
        }
    }

    private void Update()
    {
        if (_systems == null)
        {
            return;
        }

        _systems.Run();

        if (_balanceChangedPool.Has(_balanceId))
        {
            _balancePresenter.UpdateBalance();
            _balanceChangedPool.Del(_balanceId);
        }

        foreach (BusinessPresenter presenter in _presenters)
        {
            if (_businessChangedPool.Has(presenter.Entity))
            {
                presenter.UpdateBusinessInfo();
                _businessChangedPool.Del(presenter.Entity);
            }

            presenter.UpdateProgress();
        }
    }

    private void OnDestroy()
    {
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

        List<BusinessSaveData> savedBusinesses = new();

        EcsPool<BusinessComponent> businessPool = _world.GetPool<BusinessComponent>();
        EcsPool<BalanceComponent> balancePool = _world.GetPool<BalanceComponent>();
        EcsFilter filter = _world.Filter<BusinessComponent>().End();

        data.Balance = balancePool.Get(_balanceId).Value;

        foreach (int entity in filter)
        {
            BusinessSaveData businessData = new();
            BusinessComponent component = businessPool.Get(entity);

            businessData.BusinessId = component.BusinessId;
            businessData.BusinessLevel = component.Level;
            businessData.IncomeProgress = component.IncomeProgress;
            businessData.FirstUpgradeBought = component.FirstUpgradeBought;
            businessData.SecondUpgradeBought = component.SecondUpgradeBought;

            savedBusinesses.Add(businessData);
        }

        data.BusinessDataSaves = savedBusinesses.ToArray();

        return data;
    }
}
