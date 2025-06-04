using XClient.Entity;
using XClient.Network;

namespace GameScripts.HeroTeam
{

    public class ActorDataPart : NetDataPart
    {
        public NetVarLong m_hp;

        public NetVarLong m_maxHp;

        public NetVarInt m_camp;

        public NetVarVector3 m_pos;

        public NetVarVector3 m_forward;

        public NetVarFloat m_speed;

        public NetVarInt m_visibleType;

        public NetVarInt userData;

        public NetVarBool m_IsBoss;

        public NetVarInt m_Hatred;

        public NetVarInt m_power;

        protected override void OnSetupVars()
        {
            m_hp = SetupVar<NetVarLong>();
            m_maxHp = SetupVar<NetVarLong>();
            m_camp = SetupVar<NetVarInt>();
            m_pos = SetupVar<NetVarVector3>();
            m_forward = SetupVar<NetVarVector3>();
            m_speed = SetupVar<NetVarFloat>();
            m_visibleType = SetupVar<NetVarInt>();
            userData = SetupVar<NetVarInt>();
            m_IsBoss = SetupVar<NetVarBool>();
            m_Hatred = SetupVar<NetVarInt>();
            m_power = SetupVar<NetVarInt>();

            m_IsBoss.Value = false;
            m_Hatred.Value = 0;
        }
    }

}