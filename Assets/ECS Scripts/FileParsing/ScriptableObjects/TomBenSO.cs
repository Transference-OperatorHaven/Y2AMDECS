using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TomBenSO", menuName = "Scriptable Objects/TomBenSO")]
public class TomBenSO : ScriptableObject
{
    [SerializeField]
    public ScriptableObject[] types;
    [SerializeField]
    public ScriptableObject[] clusters;
    [SerializeField]
    public ScriptableObject[] waves;


    public void AddTypes(List<ScriptableObject> TypesSO)
    {
        types = TypesSO.ToArray();
    }
    public void AddCluster(List<ScriptableObject> ClusterSO)
    {
        clusters = ClusterSO.ToArray();
    }
    public void AddWaves(List<ScriptableObject> WavesSO)
    {
        waves = WavesSO.ToArray();
    }
}
