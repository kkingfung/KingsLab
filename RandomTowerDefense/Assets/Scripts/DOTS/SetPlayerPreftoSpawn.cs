using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SetPlayerPreftoSpawn
{
	static float healthBase = 10;
	public static void InitCastle(float health)
	{
		PlayerPrefs.SetFloat("EntityHealth", health);
	}

	public static void InitSkill(float area, float damage, float cycleTime, float frameWait, float activeTime)
	{
		PlayerPrefs.SetFloat("EntityArea", area);
		PlayerPrefs.SetFloat("EntityDamage", damage);
		PlayerPrefs.SetFloat("EntityWaitTime", frameWait);
		PlayerPrefs.SetFloat("EntityActiveTime", activeTime);
		PlayerPrefs.SetFloat("EntityCycleTime", cycleTime);
	}

	public static void InitEnm(int money, float health, float speed, float damage = 1, float frameWait = 0)
	{
		PlayerPrefs.SetFloat("EntityHealth", healthBase * health);
		PlayerPrefs.SetFloat("EntityDamage", damage);
		PlayerPrefs.SetFloat("EntityWaitTime", frameWait);
		PlayerPrefs.SetFloat("EntitySpeed", speed);
		PlayerPrefs.SetInt("EntityMoney", money);
	}

	public static void InitTower(float areaSq, int rank=1,float damage = 1, float frameWait = 0)
	{
		PlayerPrefs.SetInt("EntityRank", rank);
		PlayerPrefs.SetFloat("EntityWaitTime", frameWait);
		PlayerPrefs.SetFloat("EntityDamage", damage);
		PlayerPrefs.SetFloat("EntityAreaSq", areaSq);
	}

	public static void InitAttack(float areaSq, int damage, float frameWait, float activeTime)
	{
		PlayerPrefs.SetFloat("EntityWaitTime", frameWait);
		PlayerPrefs.SetFloat("EntityDamage", damage);
		PlayerPrefs.SetFloat("EntityActiveTime", activeTime);
		PlayerPrefs.SetFloat("EntityAreaSq", areaSq);
	}
}
