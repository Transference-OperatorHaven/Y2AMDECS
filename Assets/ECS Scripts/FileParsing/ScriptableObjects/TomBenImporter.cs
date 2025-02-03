using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;

[ScriptedImporter(100,"TomBen")]
public class TomBenImporter : ScriptedImporter
{

    public override void OnImportAsset(AssetImportContext ctx)
    {
        string fileText = File.ReadAllText(ctx.assetPath);



        ctx.AddObjectToAsset("TomBenSO", fileObj);
        ctx.SetMainObject(fileObj);
    }
}
