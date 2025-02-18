using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Entities.UniversalDelegates;
using UnityEditor.Overlays;

[ScriptedImporter(100, "TomBen")]
public class TomBenImporter : ScriptedImporter
{
    public enum BlockType
    {
        type,
        cluster,
        wave
    }

    public enum BlockDataState
    {
        type,
        cluster,
        wave
    }

    public struct ParsedBlock
    {
        public BlockType type;
        public string name;
        public int id;
        public string content;

        public override string ToString() =>
            $"ParsedBlock(type={type}, id={id}, name=={name}, content={content})";
    }

    public struct Type
    {
        public int id;
        public string typeName;
        public float? health;
        public float? speed;
        public float? damage;
    }

    public struct Wave
    {
        public struct WaveContent
        {
            public bool isCluster;
            public int id;
            public float? spawnTime;
            public float? pop;
        }
        public int id;
        public string waveName;
        public List<WaveContent> waveContent;

        
    }

    public struct Cluster
    {
        public struct ClusterContent
        {
            public int type;
            public int amount;
        }
        public int id;
        public string clusterName;
        public List<ClusterContent> clusterContent;
    }

    enum ParserState
    {
        OutsideBlock,
        BlockHeader,
        BlockBody
    }

    [SerializeField] string filePath;


    string fileContent;

    int charIndex = 0;

    string charBuffer = "";

    ParserState state;

    BlockDataState blockDataState;

    List<ParsedBlock> blocks;
    
    Dictionary<int, Type> typeDictionary = new Dictionary<int, Type>();
    Dictionary<int, Wave> waveDictionary = new Dictionary<int, Wave>();
    Dictionary<int, Cluster> clusterDictionary = new Dictionary<int, Cluster>();

    List<ScriptableObject> lTypeSO;
    List<ScriptableObject> lWaveSO;
    List<ScriptableObject> lClusterSO;

    TomBenSO tomBenSO;

    AssetImportContext context;


    private void ClearBuffer() => charBuffer = "";

    private bool ReachedEnd() => charIndex >= fileContent.Length;

    private char NextChar()
    {
        charBuffer += fileContent[charIndex];
        return fileContent[charIndex++];
    }

    private void ChangeState(ParserState state)
    {
        this.state = state;
        ClearBuffer();
    }
    private bool BufferHas(string token) => charBuffer.EndsWith(token);

    private bool BufferHasAny(params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (BufferHas(token)) return true;
        }

        return false;
    }

    public override void OnImportAsset(AssetImportContext ctx)
    { 
        context = ctx;
        fileContent = File.ReadAllText(context.assetPath);
        ChangeState(ParserState.OutsideBlock);
        
        blocks = new List<ParsedBlock>();
        lTypeSO = new List<ScriptableObject>();
        lWaveSO = new List<ScriptableObject>();
        lClusterSO = new List<ScriptableObject>();

        tomBenSO = ScriptableObject.CreateInstance<TomBenSO>();
        context.AddObjectToAsset("TomBenSO", tomBenSO);
        context.SetMainObject(tomBenSO);
        WriteState();
    }


    void WriteState()
    {
        Regex removeEscaped = new Regex(@"(\r\n|\r|\n)");
        Match escapedMatch = removeEscaped.Match(fileContent);
        fileContent = fileContent.Replace("\r\n", "");
        while (!ReachedEnd())
        {

            switch (this.state)
            {
                case ParserState.OutsideBlock:
                    charIndex = 0;
                    ChangeState(ParserState.BlockHeader);
                    break;
                case ParserState.BlockHeader:
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }

                    charBuffer = charBuffer[0..^4];
                    
                    Regex headerRegex = new Regex(@"(type|wave|cluster)\s*-\s*(\d+)\s*(\(([\w\s]+)\))?\s*");
                    Match headerMatch = headerRegex.Match(charBuffer);
                    if (headerMatch.Success)
                    {
                        ClearBuffer();
                        while(!BufferHas("_Ben"))
                        {
                            NextChar();
                        }

                        charBuffer = charBuffer[0..^4];

                        ParsedBlock parsedBlock = new ParsedBlock()
                        {
                            type = Enum.Parse<BlockType>(headerMatch.Groups[1].Value),
                            id = int.Parse(headerMatch.Groups[2].Value),
                            name = headerMatch.Groups[4].Value,
                            content = charBuffer
                        };
                        blocks.Add(parsedBlock);
                        ClearBuffer();

                    }
                    break;
                case ParserState.BlockBody:
                    break;
            }

        }
        FirstParseComplete();
    }

    void FirstParseComplete()
    {
        charIndex = 0;
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].type == BlockType.type) 
            { blockDataState = BlockDataState.type; }
            else if (blocks[i].type == BlockType.wave) 
            { blockDataState = BlockDataState.wave; }
            else if (blocks[i].type == BlockType.cluster) 
            { blockDataState = BlockDataState.cluster; }
            ParseBodyContent(blocks[i].id, blocks[i].name, blocks[i].content);

        }

        SOCreation();
    }

    void ParseBodyContent(int id, string name, string content)
    {
        charBuffer = content;
        switch (blockDataState)
        {
            case BlockDataState.type:
                string[] typeBlock = charBuffer.Split("!?");
                Regex typeRegex = new Regex(@"(health|speed|damage)=>(\d+)");
                float typeHealth = 0;
                float typeSpeed = 0;
                float typeDamage = 0;

                for (int i = 0; i < typeBlock.Length; i++)
                {
                    Match typeMatch = typeRegex.Match(typeBlock[i]);
                    if (typeMatch.Success)
                    {
                        string chunkType = typeMatch.Groups[1].Value;
                        if (chunkType == "health")
                        {
                            if (float.TryParse(typeMatch.Groups[2].Value, out float hOut))
                            {
                                typeHealth = hOut;
                            }

                        }
                        else if (chunkType == "speed")
                        {
                            if (float.TryParse(typeMatch.Groups[2].Value, out float sOut))
                            {
                                typeSpeed = sOut;
                            }
                        }
                        else if (chunkType == "damage")
                        {
                            if (float.TryParse(typeMatch.Groups[2].Value, out float dOut))
                            {
                                typeDamage = dOut;
                            }
                        }
                    }
                    Type type = new Type()
                    {
                        typeName = name is "" ? null : name,
                        id = id,
                        health = typeHealth is 0 ? null : typeHealth,
                        speed = typeSpeed is 0 ? null : typeSpeed,
                        damage = typeDamage is 0 ? null : typeDamage

                    };
                    try
                    {
                        typeDictionary.Add(id, type);
                    }
                    catch (ArgumentException)
                    {
                        type.typeName ??= typeDictionary[id].typeName;
                        type.health ??= typeDictionary[id].health;
                        type.speed ??= typeDictionary[id].speed;
                        type.damage ??= typeDictionary[id].damage;

                        typeDictionary[id] = type;
                    }
                }

                ClearBuffer();
                charIndex = 0;
                break;
            case BlockDataState.wave:
                string[] waveBlock = content.Split("!?");
                Regex waveRegex = new Regex(@"([CT])(\d+)\<?(\d+)?\>?\[?(\d+)?\]?");

                Wave waves = new Wave()
                {
                    id = id,
                    waveName = name,
                    waveContent = new List<Wave.WaveContent>()
                };

                Wave.WaveContent waveContent;
                for (int i = 0; i < waveBlock.Length - 1; i++)
                {
                    Match waveMatch = waveRegex.Match(waveBlock[i]);
                    if (waveMatch.Success)
                    {
                        float.TryParse(waveMatch.Groups[3].Value, out float spOut);
                        int.TryParse(waveMatch.Groups[4].Value, out int popOut);
                        if (waveMatch.Groups[1].Value == "C")
                        {
                            waveContent = new Wave.WaveContent()
                            {
                                isCluster = true,
                                id = int.Parse(waveMatch.Groups[2].Value),
                                spawnTime = spOut,
                                pop = popOut,
                            };
                            waves.waveContent.Add(waveContent);
                        }
                        else if (waveMatch.Groups[1].Value == "T")
                        {
                            waveContent = new Wave.WaveContent()
                            {
                                isCluster = false,
                                id = int.Parse(waveMatch.Groups[2].Value),
                                spawnTime = spOut,
                                pop = popOut,
                            };
                            waves.waveContent.Add(waveContent);
                        }
                    }
                }

                try
                {
                    waveDictionary.Add(id, waves);
                }
                catch (ArgumentException)
                {
                    Debug.Log($"parsed waves count = {waves.waveContent.ToList().Count}");
                    foreach (Wave.WaveContent data in waves.waveContent.ToList())
                    {
                        waveDictionary[id].waveContent.Add(data);
                    }
                    Debug.Log($"parsed waves count = {waves.waveContent.ToList().Count}");
                }

                break;
            case BlockDataState.cluster:
                string[] clusterBlock = content.Split("!?");
                Regex clusterRegex = new Regex(@"(\d+):(\d+)");
                Cluster.ClusterContent clustercontent;

                Cluster cluster = new Cluster()
                {
                    clusterName = name,
                    id = id,
                    clusterContent = new List<Cluster.ClusterContent>()
                };
                for(int i = 0; i < clusterBlock.Length - 1; i++)
                {
                    Match clusterMatch = clusterRegex.Match(clusterBlock[i]);
                    if (clusterMatch.Success)
                    {
                        clustercontent = new Cluster.ClusterContent()
                        {
                            type = int.Parse(clusterMatch.Groups[1].Value),
                            amount = int.Parse(clusterMatch.Groups[2].Value),
                        };
                        cluster.clusterContent.Add(clustercontent);
                    }
                }
                try
                {
                    clusterDictionary.Add(id, cluster);
                }
                catch
                {
                    foreach (Cluster.ClusterContent data in cluster.clusterContent.ToList())
                    {
                        clusterDictionary[id].clusterContent.Add(data);
                    }
                }

                break;
        }
    }

    void SOCreation()
    {
        foreach(Type type in typeDictionary.Values)
        {
            TypeSO typeSO = ScriptableObject.CreateInstance<TypeSO>();
            typeSO.id = type.id;
            typeSO.typeName = type.typeName;
            if (type.health != null) { typeSO.health = (float)type.health; }
            if (type.speed != null) { typeSO.speed = (float) type.speed; }
            if (type.damage != null) { typeSO.damage = (float)type.damage; }
            context.AddObjectToAsset($"typeObject {type.id}", typeSO);
            lTypeSO.Add(typeSO);

        }
        foreach(Wave wave in waveDictionary.Values)
        {
            WaveSO waveSO = ScriptableObject.CreateInstance<WaveSO>();
            waveSO.id = wave.id;
            waveSO.name = wave.waveName;
            foreach(Wave.WaveContent waveContent in waveDictionary[wave.id].waveContent)
            {
                waveSO.AddData(waveContent);
            }
            context.AddObjectToAsset($"waveObject {wave.id}", waveSO);
            lWaveSO.Add(waveSO);
        }
        foreach (Cluster cluster in clusterDictionary.Values)
        {
            ClusterSO clusterSO = ScriptableObject.CreateInstance<ClusterSO>();
            clusterSO.id = cluster.id;
            clusterSO.name = cluster.clusterName;
            foreach(Cluster.ClusterContent clusterContent in clusterDictionary[cluster.id].clusterContent)
            {
                clusterSO.AddData(clusterContent);
            }
            context.AddObjectToAsset($"clusterObject {cluster.id}", clusterSO);
            lClusterSO.Add(clusterSO);

            tomBenSO.AddTypes(lTypeSO);
            tomBenSO.AddCluster(lClusterSO);
            tomBenSO.AddWaves(lWaveSO);
        }
    }
}

