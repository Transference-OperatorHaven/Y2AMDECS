using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour
{
	public GameObject PrefabToSpawn;
    public float health;
    public float damage;
    public float speed;
    public float amount;
    public float SpawnDelay;

	private class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
	{
		public override void Bake(EnemySpawnerAuthoring authoring)
		{
			Entity e = GetEntity(TransformUsageFlags.None);

			AddComponent(e, new EnemySpawnerComponent
			{
				PrefabToSpawn = GetEntity(authoring.PrefabToSpawn, TransformUsageFlags.Dynamic),
                health = 0,
                damage = 0,
                speed = 0,
                amount = 0,
                Timer = 0.0f,
				SpawnDelay = authoring.SpawnDelay,
				SpawnPosition = authoring.transform.position
			});
		}
	}
}
