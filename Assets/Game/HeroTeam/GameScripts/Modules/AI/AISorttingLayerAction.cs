using Spine.Unity;
using UnityEngine;
using XClient.Entity;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;

public class AISorttingLayerAction : IAIAction, IEventExecuteSink
{
    //行为的优先级
    private int m_nPriority = 0;

    private ICreatureEntity m_entityMaster;

    private SkeletonAnimation m_skelModel;

    private Renderer m_renderModel;

    private int m_nSortingLayer = int.MinValue;


    public void SetMaster( ICreatureEntity master )
    {
        m_entityMaster = master;
    }

    public bool Create( )
    {
        return true;
    }

    public int GetPriority( )
    {
        return m_nPriority;
    }

    public void Init( object context = null )
    {
        cfg_AI cfg = context as cfg_AI;
        m_nPriority = cfg.iPriority;
    }

    public void OnExecute( ushort wEventID, byte bSrcType, uint dwSrcID, object pContext )
    {
    }

    public bool OnExeUpdate( )
    {
        if ( null != m_renderModel )
        {

            Vector3 pos = m_entityMaster.GetPos( );
            int currentLayer = ( int ) ( ( ( -pos.y + 25 ) * 2 + ( pos.x + 25 ) ) * 100 );
            if ( currentLayer != m_nSortingLayer )
            {
                m_nSortingLayer = currentLayer;
                m_renderModel.sortingOrder = m_nSortingLayer;
            }
        }

        return false;
    }

    public void OnReceiveEntityMessage( uint id, object data = null )
    {
        if ( id == EntityMessageID.ResLoaded )
        {
            PrefabPart visiblePart = m_entityMaster.GetPart<PrefabPart>( );
            if ( null != visiblePart && visiblePart.transform )
            {
                m_skelModel = visiblePart.transform.GetComponentInChildren<SkeletonAnimation>( );
                m_renderModel = m_skelModel?.GetComponent<Renderer>( );
            }
        }
    }

    public void Release( )
    {
        Stop( );
        //if ( m_forwardMovement )
        //{
        //    m_forwardMovement.StopMove( );
        //}
        //m_forwardMovement = null;
        m_entityMaster = null;
        m_nSortingLayer = int.MinValue;
    }

    public void Reset( )
    {

    }


    public void Start( )
    {
    }

    public void Stop( )
    {
    }
}
