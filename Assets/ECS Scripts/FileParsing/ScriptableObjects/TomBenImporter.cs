using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Rendering;
using System.Text.RegularExpressions;

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

    ParsedBlock[] blocks;

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
        while(!ReachedEnd())
        {
            fileContent = File.ReadAllText(ctx.assetPath);
            ChangeState(ParserState.OutsideBlock);
            while (!BufferHasAny("type", "cluster", "wave"))
            {
                NextChar();
            }

        }
        

        




        ctx.AddObjectToAsset("TomBenSO", fileObj);
        ctx.SetMainObject(fileObj);
    }


    void WriteState()
    {
        switch (this.state)
        {
            case ParserState.OutsideBlock:
                break;
            case ParserState.BlockHeader:
                Regex regex = new Regex(@"(type|cluster|wave)");
                Match match = regex.Match(charBuffer);
                if(match.Success)
                {
                    
                }
                break;
            case ParserState.BlockBody:
                break;
        }
    
    }
}

