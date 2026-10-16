using UnityEngine;

[CreateAssetMenu(fileName = "BusinessConfig", menuName = "Scriptable Objects/BusinessConfig")]
public class BusinessConfig : ScriptableObject
{
    public BusinessData[] Businesses;
}
