
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class TomBenParsing : MonoBehaviour
{
    [System.Serializable]
    public struct ParsedBlock
    {
        public string type;
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


    string fileContent = File.ReadAllText(filePath);

    int charIndex;

    string charBuffer = "";
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
        foreach(var token in tokens)
        {
            if(BufferHas(token)) return true;
        }

        return false;
    }
}
