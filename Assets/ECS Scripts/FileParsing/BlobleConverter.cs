using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;
using static BlobleConverter;
using Hash128 = Unity.Entities.Hash128;



public class BlobleConverter : MonoBehaviour
{
    public struct TypeBlob : IComponentData
    {
        public int id;
        //public BlobString typeName;
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
        public int id;
        //public BlobString Name;

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
        //public BlobString clusterName;
        public BlobArray<ClusterContent> clusterContent;
    }

    public struct TypeDataPool
    {
        public BlobArray<TypeBlob> pool;
    }
    public struct WaveDataPool
    {
        public BlobArray<WaveBlob> pool;
    }
    public struct ClusterDataPool
    {
        public BlobArray<ClusterBlob> pool;
    }

    public struct TypeComp : IComponentData
    {
        public BlobAssetReference<TypeDataPool> typeBlobComp;
    }
    public struct WaveComp : IComponentData
    {
        public BlobAssetReference<WaveDataPool> waveBlobComp;
    }
    public struct ClusterComp : IComponentData
    {
        public BlobAssetReference<ClusterDataPool> clusterBlobComp;
    }


    public TypeSO[] lTypeSO;
    public WaveSO[] lWaveSO;
    public ClusterSO[] lClusterSO;

    public TypeSO currentTypeSO;
    public WaveSO currentWaveSO;
    public ClusterSO currentClusterSO;

    public TypeDataPool typeDataPool;
    public WaveDataPool waveDataPool;
    public ClusterDataPool clusterDataPool;

    public static BlobAssetReference<TypeDataPool> typePool;
    public static BlobAssetReference<WaveDataPool> wavePool;
    public static BlobAssetReference<ClusterDataPool> clusterPool;

    public static TypeComp typeComp;
    public static WaveComp waveComp;
    public static ClusterComp clusterComp;

    GameObject prefabToSpawn;
    public float spawnDelay;


    BlobleConverter()
    {
        lTypeSO = Resources.LoadAll<TypeSO>("");
        lWaveSO = Resources.LoadAll<WaveSO>("");
        lClusterSO = Resources.LoadAll<ClusterSO>("");
        
    }

    

    private class BlobleConverterBaker : Baker<BlobleConverter>
    {
        public override void Bake(BlobleConverter authoring)
        {
            TypeSO[] typeSO = Resources.LoadAll<TypeSO>("");
            int typeIndex = Mathf.Max(0,Array.FindIndex<TypeSO>(typeSO, 0, typeSO.Length,(TypeSO test) => { return test == authoring.currentTypeSO; }));
            Hash128 typeHash = new Hash128((uint)typeSO.Length,(uint)typeSO[0].id, (uint)typeSO[^1].id, 0);

            if(!TryGetBlobAssetReference<TypeDataPool>(typeHash,out typePool))
            {
                BlobBuilder typeBuilder = new BlobBuilder(Allocator.Temp);
                ref TypeDataPool poolType = ref typeBuilder.ConstructRoot<TypeDataPool>();
                BlobBuilderArray<TypeBlob> typeArrayBuilder = typeBuilder.Allocate(ref poolType.pool, typeSO.Length);
                for (int i = 0; i < typeSO.Length; i++)
                {
                    typeArrayBuilder[i] = new TypeBlob
                    {
                        id = typeSO[i].id,
                        //typeName = new BlobString(typeSO[i].name, Allocator.Temp)
                        damage = typeSO[i].damage,
                        health = typeSO[i].health,
                        speed = typeSO[i].speed
                    };
                }

                typePool = typeBuilder.CreateBlobAssetReference<TypeDataPool>(Allocator.Persistent);
                typeBuilder.Dispose();
                AddBlobAssetWithCustomHash<TypeDataPool>(ref typePool, typeHash);
            }

            WaveSO[] waveSO = Resources.LoadAll<WaveSO>("");
            int waveIndex = Mathf.Max(0,Array.FindIndex<WaveSO>(waveSO, 0, waveSO.Length, (WaveSO test) => { return test == authoring.currentWaveSO; }));
            Hash128 waveHash = new Hash128((uint)waveSO.Length,(uint)waveSO[0].id, (uint)waveSO[^1].id, 0);

            if (!TryGetBlobAssetReference<WaveDataPool>(waveHash, out wavePool))
            {
                BlobBuilder waveBuilder = new BlobBuilder(Allocator.Temp);
                ref WaveDataPool poolWave = ref waveBuilder.ConstructRoot<WaveDataPool>();
                BlobBuilderArray<WaveBlob> waveArrayBuilder = waveBuilder.Allocate(ref poolWave.pool, waveSO.Length);
                for (int i = 0; i < waveSO.Length - 1; i++)
                {
                    waveArrayBuilder[i] = new WaveBlob
                    {
                        id = waveSO[i].id
                    };
                    BlobBuilderArray<WaveBlob.WaveContent> waveBlobContent = waveBuilder.Allocate(ref waveArrayBuilder[i].waveContent, waveSO.Length);
                    for(int j = 0; j < waveSO[i].waveContent.Count -1; j++)
                    {
                        waveBlobContent[i].id = waveSO[i].waveContent[j].id;
                        waveBlobContent[i].pop = waveSO[i].waveContent[j].pop;
                        waveBlobContent[i].spawnTime = waveSO[i].waveContent[j].spawnTime;
                        waveBlobContent[i].isCluster = waveSO[i].waveContent[j].isCluster;
                    }
                }

                wavePool = waveBuilder.CreateBlobAssetReference<WaveDataPool>(Allocator.Persistent);
                waveBuilder.Dispose();
                AddBlobAssetWithCustomHash<WaveDataPool>(ref wavePool, waveHash);
            }

            ClusterSO[] clusterSO = Resources.LoadAll<ClusterSO>("");
            int clusterIndex = Mathf.Max(0, Array.FindIndex<ClusterSO>(clusterSO, 0, clusterSO.Length, (ClusterSO test) => { return test == authoring.currentClusterSO; }));
            Hash128 clusterHash = new Hash128((uint)clusterSO.Length, (uint)clusterSO[0].id, (uint)clusterSO[^1].id, 0);

            if (!TryGetBlobAssetReference<ClusterDataPool>(clusterHash, out clusterPool))
            {
                BlobBuilder clusterBuilder = new BlobBuilder(Allocator.Temp);
                ref ClusterDataPool poolCluster = ref clusterBuilder.ConstructRoot<ClusterDataPool>();
                BlobBuilderArray<ClusterBlob> clusterArrayBuilder = clusterBuilder.Allocate(ref poolCluster.pool, clusterSO.Length);
                for (int i = 0; i < clusterSO.Length - 1; i++)
                {
                    clusterArrayBuilder[i] = new ClusterBlob
                    {
                        id = clusterSO[i].id
                    };
                    BlobBuilderArray<ClusterBlob.ClusterContent> clusterBlobContent = clusterBuilder.Allocate(ref clusterArrayBuilder[i].clusterContent, clusterSO.Length);
                    for (int j = 0; j < clusterSO[i].clusterContent.Count - 1; j++)
                    {
                        clusterBlobContent[i].type = clusterSO[i].clusterContent[j].type;
                        clusterBlobContent[i].amount = clusterSO[i].clusterContent[j].amount;
                    }
                }

                clusterPool = clusterBuilder.CreateBlobAssetReference<ClusterDataPool>(Allocator.Persistent);
                clusterBuilder.Dispose();
                AddBlobAssetWithCustomHash<ClusterDataPool>(ref clusterPool, clusterHash);
            }

            typeComp.typeBlobComp = typePool;
            waveComp.waveBlobComp = wavePool;
            clusterComp.clusterBlobComp = clusterPool;

            Entity e = GetEntity(TransformUsageFlags.None);

            AddComponent(e, new EnemySpawnerComponent
            {
                PrefabToSpawn = GetEntity(authoring.prefabToSpawn, TransformUsageFlags.Dynamic),
                health = 0,
                damage = 0,
                speed = 0,
                amount = 0,
                Timer = 0.0f,
                SpawnDelay = authoring.spawnDelay,
                SpawnPosition = authoring.transform.position

            });

            AddComponent(e, new TypeComp
            {
                typeBlobComp = typePool
            });
            AddComponent(e, new WaveComp
            {
                waveBlobComp = wavePool
            });
            AddComponent(e, new ClusterComp
            {
                clusterBlobComp = clusterPool
            });
                
        }
    }
}
