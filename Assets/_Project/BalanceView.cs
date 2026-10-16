using UnityEngine;
using UnityEngine.UI;

public class BalanceView : MonoBehaviour
{
    [SerializeField] private Text _balanceText;

    public void SetBalance(float balance)
    {
        _balanceText.text = $"Balance: {balance}$";
    }
}
