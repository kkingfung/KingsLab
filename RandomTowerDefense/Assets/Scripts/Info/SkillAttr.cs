using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System.IO;

namespace RandomTowerDefense.Info
{
    /// <summary>
    /// スキル属性情報クラス - 魔法スキルのパラメーター情報
    /// </summary>
    public class SkillAttr
    {
        /// <summary>
        /// 範囲
        /// </summary>
        public float Radius;

        /// <summary>
        /// ダメージ
        /// </summary>
        public float Damage;

        /// <summary>
        /// クールタイム
        /// </summary>
        public float CycleTime;

        /// <summary>
        /// 持続時間
        /// </summary>
        public float WaitTime;

        /// <summary>
        /// 全体のスキル持続時間
        /// </summary>
        public float LifeTime;

        /// <summary>
        /// 減速率
        /// </summary>
        public float SlowRate;

        /// <summary>
        /// デバフ時間
        /// </summary>
        public float DebuffTime;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="radius">範囲</param>
        /// <param name="damage">ダメージ</param>
        /// <param name="cycleTime">クールタイム</param>
        /// <param name="waitTime">持続時間</param>
        /// <param name="lifeTime">全体のスキル持続時間</param>
        /// <param name="slowRate">減速率</param>
        /// <param name="debuffTime">デバフ時間</param>
        /// </summary>
        public SkillAttr(float radius, float damage, float cycleTime, float waitTime, float lifeTime, float slowRate = 0, float debuffTime = 0)
        {
            Radius = radius;
            Damage = damage;
            WaitTime = waitTime;
            LifeTime = lifeTime;
            CycleTime = cycleTime;
            SlowRate = slowRate;
            DebuffTime = debuffTime;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="attr">コピー元のSkillAttrオブジェクト</param>
        public SkillAttr(SkillAttr attr)
        {
            Radius = attr.Radius;
            Damage = attr.Damage;
            WaitTime = attr.WaitTime;
            LifeTime = attr.LifeTime;
            CycleTime = attr.CycleTime;
            SlowRate = attr.SlowRate;
            DebuffTime = attr.DebuffTime;
        }
    }
}
