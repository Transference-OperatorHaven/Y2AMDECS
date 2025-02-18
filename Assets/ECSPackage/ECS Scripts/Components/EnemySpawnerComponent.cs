using Unity.Entities;
using Unity.Mathematics;

public struct EnemySpawnerComponent : IComponentData
{
	public Entity PrefabToSpawn;

	public float health;

    public float damage;

    public float speed;

    public float amount;

	public float Timer;

	public float SpawnDelay;

	public float3 SpawnPosition;
}
