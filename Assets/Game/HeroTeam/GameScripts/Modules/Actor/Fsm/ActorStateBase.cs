using Spine.Unity;
using UniFramework.Machine;

namespace GameScripts.HeroTeam
{
    public class ActorStateBase : IStateNode
    {
        protected SkeletonAnimation m_Anim;
        protected cfg_ActorAnimConfig m_ActorAnimConfig;
        protected StateMachine m_StateMachine;
        public virtual void OnCreate( StateMachine machine )
        {
            m_Anim = ( ( IActor ) machine.Owner ).GetSkeleton( );
            m_StateMachine = machine;
        }

        public virtual void OnEnter()
        {
            m_ActorAnimConfig = ( ( IActor ) m_StateMachine.Owner ).GetAnimConfig( );
        }

        public virtual void OnExit( )
        {
        }

        public virtual void OnUpdate( )
        {
        }
    }

}