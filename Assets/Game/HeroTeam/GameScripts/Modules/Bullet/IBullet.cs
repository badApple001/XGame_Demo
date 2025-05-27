using UnityEngine;
using XClient.Entity;


namespace GameScripts.HeroTeam
{
    public interface IBullet
    {
        void Init(GameObject ojbRef);
        bool CheckCollision();
        void Fly();
        void ClearState();
        Transform GetTr();
        void SetTarget(IMonster target);
        void SetConfig(cfg_HeroTeamBullet cfg);
        void SetHarm(int harm);
        void SetSender(ulong entityId);
    }
}