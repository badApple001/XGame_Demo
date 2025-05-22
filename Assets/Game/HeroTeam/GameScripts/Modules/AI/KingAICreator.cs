using XClient.Entity;
using XGame.Poolable;
using XGame.Utils;

namespace GameScripts.HeroTeam
{

    public class KingAICreator : Singleton<KingAICreator>, IAIActionCreator
    {


        //构造函数
        public void Setup( )
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>( );
            if ( itemPoolMgr == null )
                return;

            //if ( itemPoolMgr.GetPool<AIMoveAction>( ) == null )
            //    itemPoolMgr.Register<AIMoveAction>( );

            if ( itemPoolMgr.GetPool<AISkillAction>( ) == null )
                itemPoolMgr.Register<AISkillAction>( );

            //if ( itemPoolMgr.GetPool<AICollisionExplosionAction>( ) == null )
            //    itemPoolMgr.Register<AICollisionExplosionAction>( );

            //   if (itemPoolMgr.GetPool<AIRouteMoveAction>() == null)
            //        itemPoolMgr.Register<AIRouteMoveAction>();

            if( itemPoolMgr.GetPool<AISorttingLayerAction>( ) == null )
                itemPoolMgr.Register<AISorttingLayerAction>( );

            MonsterSystem.Instance.SetAICreator( this );
        }

        //创建一个AI
        public IAIAction CreateAIAction( int id, object context )
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>( );
            AI_ACTION_TYPE type = ( AI_ACTION_TYPE ) id;
            switch ( type )
            {
                //case AI_ACTION_TYPE.AI_ACTION_MOVE_FORWARD:
                //    return itemPoolMgr.Pop<AIMoveAction>( context );
                case AI_ACTION_TYPE.AI_ACTION_CIRCLE_SKILL:
                    return itemPoolMgr.Pop<AISkillAction>( context );
                    //return null;

                //case AI_ACTION_TYPE.AI_ACTION_COLLISION_EXPLOSION:
                //    return itemPoolMgr.Pop<AICollisionExplosionAction>( context );
                // case AI_ACTION_TYPE.AI_ACTION_ROUTE_MOVE:
                //  return itemPoolMgr.Pop<AIRouteMoveAction>(context);
                case AI_ACTION_TYPE.AI_ACTION_SORTING_LAYER:
                    return itemPoolMgr.Pop<AISorttingLayerAction>( context );
                default:
                    break;
            }
            return null;
        }

        //回收到对象池
        public void ReleaseAIAction( IAIAction action )
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>( );
            itemPoolMgr.Push( action );
        }
    }
}
