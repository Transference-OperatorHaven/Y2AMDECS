using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;


[ScriptedImporter(100,"enemy")]
public class EnemyImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string fileText = File.ReadAllText(ctx.assetPath);

        string[] chunks = fileText.Split(',');

        EnemySO enemyInstance = ScriptableObject.CreateInstance<EnemySO>();
        enemyInstance.name = chunks[0];
        enemyInstance.attackDamage = float.Parse(chunks[1]);
        enemyInstance.speed = float.Parse(chunks[2]);

        ctx.AddObjectToAsset("enemyObject", enemyInstance);
        ctx.SetMainObject(enemyInstance);
    }
}
