using Spine;
using XGame.Entity;
using XGame.Entity.Part;

namespace GameScripts.HeroTeam
{
    public class ActorSkelForwardPart : BasePart
    {
        private IActor m_actor;

        private Skeleton m_skel;

        private int m_oldScale = 1;
        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);
            if (id == EntityMessageID.ResLoaded)
            {
                m_actor = master as IActor;
                m_skel = m_actor.GetSkeleton().skeleton;
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (null != m_skel)
            {
                // Vector3 forward = (targetPos - curPos).normalized;
                int currentScale = m_actor.GetPart<ActorDataPart>().m_forward.Value.x > 0 ? 1 : -1;
                if (currentScale != m_oldScale)
                {
                    m_oldScale = currentScale;
                    m_skel.ScaleX = currentScale;
                }
            }
        }

    }

}