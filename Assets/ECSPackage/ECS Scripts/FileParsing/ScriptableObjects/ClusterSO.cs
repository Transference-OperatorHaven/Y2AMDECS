using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClusterSO", menuName = "Scriptable Objects/ClusterSO")]
public class ClusterSO : ScriptableObject
{
    [System.Serializable]
    public struct ClusterContent
    {
        public int type;
        public int amount;
    }
    public int id;
    public string clusterName;
    public List<ClusterContent> clusterContent;

   

    public void AddData(TomBenImporter.Cluster.ClusterContent data)
    {
        if (clusterContent == null)
        {
            clusterContent = new List<ClusterContent>();
        }

        ClusterContent clusterData = new ClusterContent()
        {
            amount = data.amount,
            type = data.type,
        };

        clusterContent.Add(clusterData);
    }
}
