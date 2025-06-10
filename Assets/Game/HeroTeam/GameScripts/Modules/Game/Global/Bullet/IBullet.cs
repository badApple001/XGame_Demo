using UnityEngine;
using XClient.Entity;


namespace GameScripts.HeroTeam
{
    public interface IBullet
    {
        void Init(GameObject ojbRef);
        bool IsExpired();
        void Fly();
        void ClearState();
        Transform GetTr();
        void Active(Vector3 newPos);
        void SetTarget(ISpineCreature target);
        void SetConfig(cfg_HeroTeamBullet cfg);
        void SetHarm(int harm);
        void SetSender(ulong entityId);
        int GetPoolId();
    }
}