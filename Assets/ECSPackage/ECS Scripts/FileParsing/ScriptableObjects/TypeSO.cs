using UnityEngine;

[CreateAssetMenu(fileName = "TypeSO", menuName = "Scriptable Objects/TypeSO")]
public class TypeSO : ScriptableObject
{
    public int id;
    public string typeName;
    public float health;
    public float speed;
    public float damage;


}
