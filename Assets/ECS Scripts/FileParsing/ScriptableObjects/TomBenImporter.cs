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
    public struct ParsedBlock
    {
        public BlockType type;
        public string name;
        public int id;
        public string content;

        public override string ToString() =>
            $"ParsedBlock(type={type}, id={id}, name=={name}, content={content})";
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

    List<ParsedBlock> blocks;

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
    }
}

