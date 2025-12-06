using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using RandomTowerDefense.DOTS.Tags;
using RandomTowerDefense.DOTS.Components;

namespace RandomTowerDefense.DOTS.Systems.Skills
{
    /// <summary>
    /// 攻撃エンティティの移動と位置更新を処理するシステム
    /// </summary>
    public class AttackUpdateSystem : ComponentSystem
    {
        /// <summary>
        /// 攻撃エンティティの更新処理
        /// </summary>
        protected override void OnUpdate()
        {
            EntityManager entityManager = World.EntityManager;
            Entities.WithAll<AttackTag>().ForEach((Entity unitEntity, ref Translation transform, ref ActionTime action, ref WaitingTime wait, ref Velocity velocity, ref Radius radius) =>
            {
                transform.Value += velocity.Value * Time.DeltaTime;
                //if (action.Value > 0 && wait.Value <= 0)
                //{
                //    Debug.DrawLine(transform.Value, transform.Value + new float3(0, 1, 0), Color.magenta);

                //    Debug.DrawLine(transform.Value, transform.Value + new float3(radius.Value, 0, 0), Color.black);
                //    Debug.DrawLine(transform.Value, transform.Value + new float3(-radius.Value, 0, 0), Color.black);
                //    Debug.DrawLine(transform.Value, transform.Value + new float3(0, 0, radius.Value), Color.black);
                //    Debug.DrawLine(transform.Value, transform.Value + new float3(0, 0, -radius.Value), Color.black);
                //}
            });
        }
    }
}
