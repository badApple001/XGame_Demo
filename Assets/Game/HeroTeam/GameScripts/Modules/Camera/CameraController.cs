using DG.Tweening;
using UnityEngine;
using XClient.Common;
using XGame.EventEngine;

namespace GameScripts.HeroTeam
{

    public class CameraController : MonoBehaviour, IEventExecuteSink
    {
        private Transform m_trCamera = null;
        private Vector3 m_vec3Original;
        private Tween m_tweenShake;


        public void OnExecute( ushort wEventID, byte bSrcType, uint dwSrcID, object pContext )
        {

            if ( wEventID == DHeroTeamEvent.EVENT_CAMERA_SHAKE )
            {
                // ��ֹ����𶯵���
                if ( m_tweenShake != null && m_tweenShake.IsActive( ) )
                    m_tweenShake.Kill( );

                transform.localPosition = m_vec3Original;
                if ( pContext is CameraShakeEventContext ctx )
                {
                    m_tweenShake = transform.DOShakePosition(
                        duration: ctx.duration,
                        strength: ctx.intensity,
                        vibrato: ctx.vibrato,
                        randomness: ctx.randomness,
                        snapping: false,
                        fadeOut: ctx.fadeOut
                    ).OnComplete( ( ) =>
                    {
                        transform.localPosition = m_vec3Original;
                    } );
                }
            }
            else if ( wEventID == DHeroTeamEvent.EVENT_START_GAME )
            {
                m_trCamera.DOMoveY( 15f, 2f ).OnComplete( ( ) =>
                {
                    m_vec3Original = transform.localPosition;
                } );
            }
        }

        // Start is called before the first frame update
        void Start( )
        {
            m_trCamera = transform;
            m_vec3Original = transform.localPosition;

            GameGlobal.EventEgnine.Subscibe( this, DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "CameraController:Start" );
            GameGlobal.EventEgnine.Subscibe( this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "CameraController:Start" );
        }


        private void OnDestroy( )
        {
            GameGlobal.EventEgnine.UnSubscibe( this, DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0 );
            GameGlobal.EventEgnine.UnSubscibe( this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0 );
        }

    }
}