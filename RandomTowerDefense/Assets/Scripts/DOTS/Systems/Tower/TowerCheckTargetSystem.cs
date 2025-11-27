using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

namespace RandomTowerDefense.DOTS.Systems.Tower
{
    /// <summary>
    /// タワーターゲット確認システム - ターゲットの有効性と範囲内チェック
    ///
    /// 主な機能:
    /// - ターゲットエンティティの存在確認
    /// - 攻撃範囲内ターゲット検証
    /// - 無効ターゲットの自動解除
    /// - ターゲット情報のリアルタイム更新
    /// </summary>
    public class TowerCheckTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityManager entityManager = World.EntityManager;
        Entities.WithAll<PlayerTag>().ForEach((Entity unitEntity, ref Target target, ref Translation transform, ref WaitingTime wait, ref Radius radius) => 
        {

            if (entityManager.Exists(target.targetEntity) && target.targetEntity != Entity.Null)
            {
                if (entityManager.HasComponent<EnemyTag>(target.targetEntity))
                {
                    Translation targetpos = entityManager.GetComponentData<Translation>(target.targetEntity);
                    Health targetHealth = entityManager.GetComponentData<Health>(target.targetEntity);
                    target.targetPos = targetpos.Value;
                    targetpos.Value.y = transform.Value.y;
                    target.targetHealth = targetHealth.Value;
                    if (CheckCollision(transform.Value, targetpos.Value, radius.Value * radius.Value)==false)
                    {
                        // far to target, destroy it
                        //PostUpdateCommands.DestroyEntity(hasTarget.targetEntity);
                        //PostUpdateCommands.RemoveComponent(unitEntity, typeof(Target));
                        target.targetEntity = Entity.Null;
                        target.targetHealth = 0;
                        entityManager.RemoveComponent<Target>(unitEntity);
                    }
                }
                else
                {
                    target.targetEntity = Entity.Null;
                    target.targetHealth = 0;
                    target.targetPos = new Vector3();
                    entityManager.RemoveComponent<Target>(unitEntity);
                }
            }
        });
    }

        #region Private Methods
        /// <summary>
        /// 2点間の距離の2乗を計算
        /// </summary>
        /// <param name="posA">位置A</param>
        /// <param name="posB">位置B</param>
        /// <returns>距離の2乗</returns>
        private static float GetDistance(float3 posA, float3 posB)
        {
            float3 delta = posA - posB;
            return delta.x * delta.x + delta.z * delta.z;
        }

        /// <summary>
        /// 範囲内衝突チェック
        /// </summary>
        /// <param name="posA">位置A</param>
        /// <param name="posB">位置B</param>
        /// <param name="radiusSqr">半径の2乗</param>
        /// <returns>衝突しているか</returns>
        private static bool CheckCollision(float3 posA, float3 posB, float radiusSqr)
        {
            return GetDistance(posA, posB) <= radiusSqr;
        }
        #endregion
    }
}
