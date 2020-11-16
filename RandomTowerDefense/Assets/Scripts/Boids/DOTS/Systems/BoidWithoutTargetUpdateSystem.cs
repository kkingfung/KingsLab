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

public class BoidWithoutTargetUpdateSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        return Entities.WithAll<BoidTag>().ForEach((Entity entity, ref BoidDirection forward, ref OriPos oripos,
            ref Translation transformType, ref BoidData dataType, ref Velocity vecType,
            ref BoidSettingDots settingType) => {
                Vector3 acceleration = Vector3.zero;

                if (dataType.numFlockmates != 0)
                {
                    dataType.flockCentre /= dataType.numFlockmates;

                    Vector3 offsetToFlockmatesCentre = (dataType.flockCentre - transformType.Value);

                    var alignmentForce = SteerTowards(dataType.flockHeading,
                    new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z), settingType) * settingType.alignWeight;
                    var cohesionForce = SteerTowards(offsetToFlockmatesCentre,
                    new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z), settingType) * settingType.cohesionWeight;
                    var seperationForce = SteerTowards(dataType.avoidanceHeading,
                    new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z), settingType) * settingType.seperateWeight;

                    acceleration += alignmentForce;
                    acceleration += cohesionForce;
                    acceleration += seperationForce;
                }

                if (IsHeadingForCollision(transformType.Value, oripos.Value,
                    forward.Value, settingType))
                {
                    Vector3 collisionAvoidDir = ObstacleRays(transformType.Value, oripos.Value,
                       forward.Value, settingType);
                    Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir,
                        new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z),
                        settingType) * settingType.avoidCollisionWeight;
                    acceleration += collisionAvoidForce;
                }

                vecType.Value += new float3(acceleration.x, acceleration.y, acceleration.z) * deltaTime;
                float speed = new Vector3(vecType.Value.x, vecType.Value.y, vecType.Value.z).magnitude;
                float3 dir = vecType.Value / speed;
                speed = Mathf.Clamp(speed, settingType.minSpeed, settingType.maxSpeed);
                vecType.Value = dir * speed;

                transformType.Value += vecType.Value * deltaTime;
                forward.Value = dir;
            }).WithoutBurst().Schedule(inputDeps);
    }

    static Vector3 SteerTowards(Vector3 vector, Vector3 velocity, BoidSettingDots settings)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

    public static bool IsHeadingForCollision(Vector3 currPos, Vector3 oriPos, Vector3 forward, BoidSettingDots setting)
    {
        float distSq = (currPos + forward - oriPos).sqrMagnitude;
        RaycastHit hit;
        if (distSq > setting.boundsRadius * setting.boundsRadius)
        {
            return true;
        }
        else { }
        return false;
    }

    static Vector3 ObstacleRays(Vector3 currPos, Vector3 oriPos, Vector3 forward, BoidSettingDots setting)
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; ++i)
        {
            Vector3 dir = rayDirections[i];
            Ray ray = new Ray(currPos, dir);

            float distSq = (currPos - oriPos).sqrMagnitude;
            RaycastHit hit;
            if (distSq > setting.collisionAvoidDst * setting.collisionAvoidDst)
            {
                return dir;
            }
        }

        return forward;
    }

    //public static bool IsHeadingForCollision(Vector3 position, Vector3 forward, BoidSettingDots setting)
    //{
    //    RaycastHit hit;
    //    if (Physics.SphereCast(position, setting.boundsRadius, forward, out hit,
    //        setting.collisionAvoidDst))
    //    {
    //        return true;
    //    }
    //    else { }
    //    return false;
    //}

    //static Vector3 ObstacleRays(Vector3 position, Vector3 forward, BoidSettingDots setting)
    //{
    //    Vector3[] rayDirections = BoidHelper.directions;

    //    for (int i = 0; i < rayDirections.Length; ++i)
    //    {
    //        Vector3 dir = rayDirections[i];
    //        Ray ray = new Ray(position, dir);
    //        if (!Physics.SphereCast(ray, setting.boundsRadius, setting.collisionAvoidDst, LayerMask.GetMask("BoidsWall")))
    //        {
    //            return dir;
    //        }
    //    }

    //    return forward;
    //}
}
