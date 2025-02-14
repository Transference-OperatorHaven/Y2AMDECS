using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;



public class BlobleConverter : MonoBehaviour
{
    public struct TypeBlob : IComponentData
    {
        public int id;
        public BlobString typeName;
        public float health;
        public float speed;
        public float damage;
    }
    public struct WaveBlob
    {
        public struct WaveContent : IComponentData
        {
            public bool isCluster;
            public int id;
            public float spawnTime;
            public int pop;
        }
        public int ID;
        public BlobString Name;

        public BlobArray<WaveContent> waveContent;
    }
    public struct ClusterBlob : IComponentData
    {
        public struct ClusterContent
        {
            public int type;
            public int amount;
        }
        public int id;
        public BlobString clusterName;
        public BlobArray<ClusterContent> clusterContent;
    }

    public TypeSO[] lTypeSO;
    public WaveSO[] lWaveSO;
    public ClusterSO[] lClusterSO;

    public TypeBlob[] lTypeBlob;
    public WaveBlob[] lWaveBlob;
    public ClusterBlob[] lClusterBlob;

    void ConvertSOtoBlob()
    {
        ConvertTypeSOtoBlob();
        ConvertWaveSOtoBlob();
        ConvertClusterSOtoBlob();
    }

    void ConvertTypeSOtoBlob()
    {
        lTypeBlob = new TypeBlob[lTypeSO.Length];
        for(int i = 0; i < lTypeSO.Length; i++)
        {
            lTypeBlob[i].id = lTypeSO[i].id;
            lTypeBlob[i].typeName = lTypeSO[i].typeName.ConvertTo<BlobString>();
            lTypeBlob[i].health = lTypeSO[i].health;
            lTypeBlob[i].speed = lTypeSO[i].speed;
            lTypeBlob[i].damage = lTypeSO[i].damage;
        }

    }

    void ConvertWaveSOtoBlob()
    {
        lWaveBlob = new WaveBlob[lWaveSO.Length];
        for (int i = 0; i < lWaveSO.Length; i++)
        {
            lWaveBlob[i].ID = lWaveSO[i].ID;
            lWaveBlob[i].Name = lWaveSO[i].Name.ConvertTo<BlobString>(); ;
            lWaveBlob[i].waveContent = new BlobArray<WaveBlob.WaveContent>();
            for(int j = 0; j < lWaveSO[i].waveContent.Count; j++)
            {
                lWaveBlob[i].waveContent[j].isCluster = lWaveSO[i].waveContent[j].isCluster;
                lWaveBlob[i].waveContent[j].id = lWaveSO[i].waveContent[j].id;
                lWaveBlob[i].waveContent[j].spawnTime = lWaveSO[i].waveContent[j].spawnTime;
                lWaveBlob[i].waveContent[j].pop = lWaveSO[i].waveContent[j].pop;
            }
        }
    }

    void ConvertClusterSOtoBlob()
    {
        lClusterBlob = new ClusterBlob[lClusterSO.Length];
        for (int i = 0; i < lClusterSO.Length; i++)
        {
            lClusterBlob[i].id = lClusterSO[i].id;
            lClusterBlob[i].clusterName = lClusterSO[i].clusterName.ConvertTo<BlobString>();;
            lClusterBlob[i].clusterContent = new BlobArray<ClusterBlob.ClusterContent>();
            for (int j = 0; j < lClusterSO[i].clusterContent.Count; j++)
            {
                lClusterBlob[i].clusterContent[j].type = lClusterSO[i].clusterContent[j].type;
                lClusterBlob[i].clusterContent[j].amount = lClusterSO[i].clusterContent[j].amount;
            }
        }
    }

    BlobleConverter()
    {
        lTypeSO = Resources.LoadAll<TypeSO>("");
        lWaveSO = Resources.LoadAll<WaveSO>("");
        lClusterSO = Resources.LoadAll<ClusterSO>("");

        ConvertSOtoBlob();
    }

    

    private class BlobleConverterBaker : Baker<BlobleConverter>
    {
        public override void Bake(BlobleConverter authoring)
        {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
        }
    }
}
