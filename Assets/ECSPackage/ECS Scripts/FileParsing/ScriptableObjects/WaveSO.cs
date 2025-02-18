using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSO", menuName = "Scriptable Objects/WaveSO")]
public class WaveSO : ScriptableObject
{
    [System.Serializable]
    public struct WaveContent
    {
        public bool isCluster;
        public int id;
        public float spawnTime;
        public int pop;
    }
    public int id;
    public string name;

    public List<WaveContent> waveContent;




    //list of structs, structs of structs


    public void AddData(TomBenImporter.Wave.WaveContent data)
    {

        if (waveContent == null)
        {
            waveContent = new List<WaveContent>();
        }

        WaveContent waveData = new WaveContent()
        {
            id = data.id,
            isCluster = data.isCluster,
            spawnTime = (float)data.spawnTime,
            pop = (int)data.pop
        };
        waveContent.Add(waveData);
    }
}
