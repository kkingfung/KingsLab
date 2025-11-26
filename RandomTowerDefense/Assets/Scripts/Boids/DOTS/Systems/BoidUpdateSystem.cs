using Unity;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 群れモデル（Boids）を使ったエンティティの移動と行動を更新するシステム
/// 衝突回避、群れ行動、速度制御を処理
/// </summary>
public class BoidUpdateSystem : JobComponentSystem
{
    /// <summary>
    /// Boidsエンティティの行動と移動を更新
    /// </summary>
    /// <param name="inputDeps">入力依存関係</param>
    /// <returns>ジョブハンドル</returns>
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<BoidTag>().ForEach((Entity entity, ref OriPos oripos,
            ref BoidData dataType, ref BoidDataAvg dataAvgType, ref Velocity vecType, ref BoidRotation rotType,
            ref BoidSettingDots settingType) => {
                dataAvgType.avgFlockHeading = dataType.flockHeading;
                dataAvgType.avgAvoidanceHeading = dataType.flockCentre;
                dataAvgType.centreOfFlockmates = dataType.avoidanceHeading;
                dataAvgType.numPerceivedFlockmates = dataType.numFlockmates;

                Vector3 acceleration = Vector3.zero;

                //if (dataAvgType.numPerceivedFlockmates != 0)
                //{
                //    dataAvgType.centreOfFlockmates /= dataAvgType.numPerceivedFlockmates;

                //    Vector3 offsetToFlockmatesCentre = (dataAvgType.centreOfFlockmates - dataType.position);

                //    var alignmentForce = SteerTowards(dataAvgType.avgFlockHeading,
                //    new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z),
                //    settingType) * settingType.alignWeight;
                //    var cohesionForce = SteerTowards(offsetToFlockmatesCentre,
                //    new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z),
                //    settingType) * settingType.cohesionWeight;
                //    var seperationForce = SteerTowards(dataAvgType.avgAvoidanceHeading,
                //    new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z),
                //    settingType) * settingType.seperateWeight;

                //    acceleration += alignmentForce;
                //    acceleration += cohesionForce;
                //    acceleration += seperationForce;
                //}

                if (IsHeadingForCollision(dataType.position,
                    settingType,oripos.Value, oripos.BoundingRadius))
                {
                    Vector3 collisionAvoidDir = ObstacleRays(dataType.position, dataType.direction,
                       settingType,rotType.rotation, oripos.Value, oripos.BoundingRadius);
                    Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir,
                        new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z),
                        settingType) * settingType.avoidCollisionWeight;
                    acceleration += collisionAvoidForce;
                }

                vecType.Value += new float3(acceleration.x, acceleration.y, acceleration.z) * deltaTime;
                float speed = new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z).magnitude;
                Vector3 dir = new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z).normalized;
                speed = Mathf.Clamp(speed, settingType.minSpeed, settingType.maxSpeed);
                vecType.Value = dir * speed;

                dataType.position += vecType.Value * deltaTime;
                dataType.direction = dir;
                Debug.DrawLine(dataType.position, dataType.position+ dataType.direction, Color.blue);
                Debug.DrawLine(dataType.position, dataType.position + vecType.Value, Color.red);
            }).WithoutBurst().Schedule(inputDeps);
    }

    /// <summary>
    /// 目標ベクトルに向かった操舟力を計算
    /// </summary>
    /// <param name="vector">目標ベクトル</param>
    /// <param name="velocity">現在の速度</param>
    /// <param name="settings">Boids設定</param>
    /// <returns>操舟力ベクトル</returns>
    private static Vector3 SteerTowards(Vector3 vector, Vector3 velocity, BoidSettingDots settings)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

    /// <summary>
    /// エンティティが境界と衝突しそうかどうかを判定
    /// </summary>
    /// <param name="currPos">現在位置</param>
    /// <param name="setting">Boids設定</param>
    /// <param name="oriPos">原点位置</param>
    /// <param name="boundRadius">境界半径</param>
    /// <returns>衝突する場合true</returns>
    private static bool IsHeadingForCollision(Vector3 currPos,　BoidSettingDots setting,
        Vector3 oriPos, float boundRadius)
    {
        float dist = (currPos - oriPos).magnitude;
        if (dist + setting.boundsRadius + setting.collisionAvoidDst > boundRadius)
        {
            return true;
        }
        else { }
        return false;
    }

    /// <summary>
    /// クォータニオンとベクトルの乗算を実行
    /// </summary>
    /// <param name="rotation">回転クォータニオン</param>
    /// <param name="vec">ベクトル</param>
    /// <returns>回転適用後のベクトル</returns>
    private static Vector3 QuaternionMultiplyVector(Quaternion rotation, Vector3 vec)
    {
        LinearAlgebra.Quaternion3d quaternion = new LinearAlgebra.Quaternion3d(
            rotation.x, rotation.y, rotation.z, rotation.w);

        return quaternion * vec;
    }

    /// <summary>
    /// 障害物回避のためのレイキャストで最適な方向を求める
    /// </summary>
    /// <param name="currPos">現在位置</param>
    /// <param name="forward">前方向</param>
    /// <param name="setting">Boids設定</param>
    /// <param name="rotation">回転</param>
    /// <param name="oriPos">原点位置</param>
    /// <param name="boundRadius">境界半径</param>
    /// <returns>回避方向ベクトル</returns>
    private static Vector3 ObstacleRays(Vector3 currPos, Vector3 forward, BoidSettingDots setting,
        Quaternion rotation, Vector3 oriPos, float boundRadius)
    {
        Vector3[] rayDirections = BoidHelper.directions;
        float finalDist = (currPos + forward - oriPos).sqrMagnitude;
        for (int i = 0; i < rayDirections.Length; ++i)
        {
            Vector3 dir = QuaternionMultiplyVector(rotation, rayDirections[i]);
            float Dist = (currPos + dir - oriPos).sqrMagnitude;
            if (Dist < finalDist)
                return dir;
        }
                //float distSq = ((currPos + dir) - oriPos).sqrMagnitude;
                //if (Dist + setting.boundsRadius + setting.collisionAvoidDst < boundRadius)
                //{
                //    return dir;
                //}

        return forward;
    }
}
