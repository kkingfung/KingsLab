using Unity.Entities;
using UnityEngine;

public class Attackbehaviour : MonoBehaviour, IConvertGameObjectToEntity
{
	void Start()
	{
	}

	public void Convert(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
	{
		manager.AddComponent(entity, typeof(AttackTag));
		manager.AddComponent(entity, typeof(Hybrid));
		Area earea = new Area { Value = PlayerPrefs.GetFloat("EntityArea") };
		manager.AddComponentData(entity, earea);
		Damage edamage = new Damage { Value = PlayerPrefs.GetInt("EntityDamage") };
		manager.AddComponentData(entity, edamage);
		WaitingFrame ewait = new WaitingFrame { Value = PlayerPrefs.GetFloat("EntityWaitTime") };
		manager.AddComponentData(entity, ewait);
		ActiveTime eactive = new ActiveTime { Value = PlayerPrefs.GetFloat("EntityActiveTime") };
		manager.AddComponentData(entity, eactive);
		ActionTime eaction = new ActionTime { Value = PlayerPrefs.GetFloat("EntityActionTime") };
		manager.AddComponentData(entity, eactive);
	}

}
