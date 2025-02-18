using UnityEngine;

[CreateAssetMenu(fileName = "DataSO", menuName = "Scriptable Objects/DataSO")]
public class DataSO : ScriptableObject
{
    public enum DataState
    {
        type,
        cluster,
        wave
    }

    public DataState state;

    public int id;

    public string enemyName;

    public string content;
}
