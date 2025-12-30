using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.RemoteConfig;
using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// タワーの戦闘属性情報を格納するクラス
    /// </summary>
    public class TowerAttr
    {
        /// <summary>
        /// ダメージ値
        /// </summary>
        public float Damage { get; }

        /// <summary>
        /// 待機時間
        /// </summary>
        public float WaitTime { get; }

        /// <summary>
        /// 持続時間
        /// </summary>
        public float Lifetime { get; }

        /// <summary>
        /// 攻撃半径
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// 攻撃ダメージ持続時間
        /// </summary>
        public float AttackLifetime { get; }

        /// <summary>
        /// 攻撃待機時間
        /// </summary>
        public float AttackWaittime { get; }

        /// <summary>
        /// 攻撃範囲
        /// </summary>
        public float AttackRadius { get; }

        /// <summary>
        /// 攻撃速度
        /// </summary>
        public float AttackSpeed { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="radius">攻撃半径</param>
        /// <param name="damage">ダメージ値</param>
        /// <param name="waitTime">待機時間</param>
        /// <param name="lifetime">持続時間</param>
        /// <param name="attackWaittime">攻撃待機時間</param>
        /// <param name="attackRadius">攻撃範囲</param>
        /// <param name="attackSpd">攻撃速度</param>
        /// <param name="attackLifetime">攻撃ダメージ持続時間</param>
        public TowerAttr(float radius, float damage, float waitTime,
            float lifetime, float attackWaittime,
            float attackRadius, float attackSpd, float attackLifetime)
        {
            this.Radius = radius;
            this.Damage = damage;
            this.WaitTime = waitTime;
            this.Lifetime = lifetime;
            this.AttackWaittime = attackWaittime;
            this.AttackRadius = attackRadius;
            this.AttackSpeed = attackSpd;
            this.AttackLifetime = attackLifetime;
        }
    }
}
