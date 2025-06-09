using UnityEngine;
using XGame.Entity;
using System;

namespace GameScripts.HeroTeam
{
    public interface IActor : IVisibleEntity
    {
        Transform GetTr();

        cfg_Actor GetActorCig();


        Transform GetLockTr();

        Vector3 GetPos();

        void SetPos(Vector3 pos);

        void SetPos(float[] float3Pos);

        void SetRotation(Quaternion rotate);

        Quaternion GetRotation();

        ActorState GetState();

        void SetState(ActorState actorState);

        void SetIntAttr(int key, int value);

        int GetIntAttr(int key);

        void SetResLoadedCallback(Action callback);

        void ClearTimes();
    }
}
