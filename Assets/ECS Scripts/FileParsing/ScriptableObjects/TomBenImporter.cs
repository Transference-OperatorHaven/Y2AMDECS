using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Unity.Entities.UniversalDelegates;

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
        [CanBeNull] public string typeName;
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
            public float? SpawnTime;
            public float? Pop;
        }
        public int id;
        [CanBeNull] public string waveName;
        public WaveContent[] waveContent;

        
    }

    public struct Cluster
    {
        public struct ClusterContent
        {
            public int id;
            public int amount;
        }
        public int type;
        public string clusterName;
        public ClusterContent[] clusterContent;
    }

    enum ParserState
    {
        OutsideBlock,
        BlockHeader,
        BlockBody
    }

    [SerializeField] string filePath;


    string fileContent;

    int charIndex;

    string charBuffer = "";

    ParserState state;

    BlockDataState blockDataState;

    List<ParsedBlock> blocks;

    Dictionary<int, Type> typeDictionary = new Dictionary<int, Type>();
    Dictionary<int, Wave> waveDictionary = new Dictionary<int, Wave>();
    Dictionary<int, Cluster> clusterDictionary = new Dictionary<int, Cluster>();

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
        blocks = new List<ParsedBlock>();
        ChangeState(ParserState.OutsideBlock);
        WriteState();
    }


    void WriteState()
    {
        while (!ReachedEnd())
        {

            switch (this.state)
            {
                case ParserState.OutsideBlock:
                    while (!BufferHas("_Tom"))
                    {
                        NextChar();
                    }
                    charIndex = 0;
                    ChangeState(ParserState.BlockHeader);
                    break;
                case ParserState.BlockHeader:
                    Regex headerRegex = new Regex(@"(type|wave|cluster)\\s*-\\s*(\\d+)\\s*(\(([\\w\\s]+)\))?\\s*");
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
    }

    void ParseBodyContent(int id, string name, string content)
    {
        charBuffer = content;
        switch(blockDataState)
        {
            case BlockDataState.type:
                string[] contentBlocks = charBuffer.Split("!?");
                Regex contentRegex = new Regex(@"(health|speed|damage)=>(\\d+)");
                float typeHealth = 0;
                float typeSpeed = 0;
                float typeDamage = 0;

                for(int i = 0;i<contentBlocks.Length; i++)
                {
                    Match contentMatch = contentRegex.Match(contentBlocks[i]);
                    if (contentMatch.Success)
                    {
                        string chunkType = contentMatch.Groups[1].Value;
                        if(chunkType == "health")
                        {
                            if (float.TryParse(contentMatch.Groups[2].Value, out float hOut))
                            {
                                typeHealth = hOut;
                            }
                            
                        }
                        else if (chunkType == "speed")
                        {
                            if (float.TryParse(contentMatch.Groups[2].Value, out float sOut))
                            {
                                typeSpeed = sOut;
                            }
                        }
                        else if(chunkType == "damage")
                        {
                            if (float.TryParse(contentMatch.Groups[2].Value, out float dOut))
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

                }

                break;
        }
    }
}

