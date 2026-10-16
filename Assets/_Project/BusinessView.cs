using System;
using UnityEngine;
using UnityEngine.UI;

public class BusinessView : MonoBehaviour
{
    [SerializeField] private Text _businessName;

    [SerializeField] private Button _purchaseLevelButton;
    [SerializeField] private Button _purchaseFirstUpgradeButton;
    [SerializeField] private Button _purchaseSecondUpgradeButton;
    [SerializeField] private Text _firstUpgradeInfo;
    [SerializeField] private Text _secondUpgradeInfo;
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _levelCost;
    [SerializeField] private Text _incomeText;
    [SerializeField] private RectTransform _incomeProgressBar;


    public event Action PurchaseButtonPressed;
    public event Action FirstUpgradeButtonPressed;
    public event Action SecondUpgradeButtonPressed;

    public void SetBusinessName(string businessName)
    {
        _businessName.text = businessName;
    }

    public void SetSecondUpgradeInfo(string name, bool isBought, int cost, int boostInt)
    {
        if (isBought)
        {
            _secondUpgradeInfo.text = $"У{name}Ф\r\nƒоход: +{boostInt}%\r\n уплено";
        }
        else
        {
            _secondUpgradeInfo.text = $"У{name}Ф\r\nƒоход: +{boostInt}%\r\n÷ена: {cost}$";
        }
    }

    public void SetFirstUpgradeInfo(string name, bool isBought, int cost, int boostInt)
    {
        if (isBought)
        {
            _firstUpgradeInfo.text = $"У{name}Ф\r\nƒоход: +{boostInt}%\r\n уплено";
        }
        else
        {
            _firstUpgradeInfo.text = $"У{name}Ф\r\nƒоход: +{boostInt}%\r\n÷ена: {cost}$";
        }
    }

    public void SetIncomeProgress(float progress)
    {
        _incomeProgressBar.anchorMax = new Vector2 (progress, 1f);
    }

    public void SetNextLevelCost(int cost)
    {
        _levelCost.text = $"Cost: {cost}$";
    }

    public void SetIncome(float income)
    {
        _incomeText.text = $"{income}$";
    }

    public void SetLevel(int level)
    {
        _levelText.text = level.ToString();
    }

    private void OnEnable()
    {
        _purchaseLevelButton.onClick.AddListener(OnLevelButtonClicked);
        _purchaseFirstUpgradeButton.onClick.AddListener(OnFirstUpgradeButtonClicked);
        _purchaseSecondUpgradeButton.onClick.AddListener(OnSecondUpgradeButtonClicked);
    }

    private void OnDisable()
    {
        _purchaseLevelButton.onClick.RemoveListener(OnLevelButtonClicked);
        _purchaseFirstUpgradeButton.onClick.RemoveListener(OnFirstUpgradeButtonClicked);
        _purchaseSecondUpgradeButton.onClick.RemoveListener(OnSecondUpgradeButtonClicked);
    }

    private void OnSecondUpgradeButtonClicked()
    {
        SecondUpgradeButtonPressed?.Invoke();
    }

    private void OnFirstUpgradeButtonClicked()
    {
        FirstUpgradeButtonPressed?.Invoke();
    }

    private void OnLevelButtonClicked()
    {
        PurchaseButtonPressed?.Invoke();
    }

}
