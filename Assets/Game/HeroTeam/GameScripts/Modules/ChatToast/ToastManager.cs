using script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;
using XClient.Common;
using XGame;
using XGame.Asset;
using XGame.Poolable;
using XGame.UnityObjPool;
using XGame.Utils;

public class ToastManager : Singleton<ToastManager>
{

    public ToastManager( ) => Setup( );
    private List<Toast> m_arrToastPool = new List<Toast>( );
    private GameObject m_objPrefab = null;
    public void Setup( )
    {
        if ( null == m_objPrefab )
        {
            uint handle = 0;
            var loader = XGameComs.Get<IGAssetLoader>( );
            m_objPrefab = ( GameObject ) loader.LoadResSync<GameObject>( "Game/HeroTeam/GameResources/Prefabs/UI/Toast.prefab", out handle );
            loader.UnloadRes( handle );
        }
    }

    public void UnloadAll( )
    {
        var loader = XGameComs.Get<IGAssetLoader>( );
        m_arrToastPool.ForEach( toast => GameObject.DestroyImmediate( toast.gameObject ) );
        m_arrToastPool.Clear( );
    }

    public Toast Get( )
    {
        if ( m_arrToastPool.Count > 0 )
        {
            var r = m_arrToastPool[ m_arrToastPool.Count - 1 ];
            m_arrToastPool.RemoveAt( m_arrToastPool.Count - 1 );
            Init( r );
            return r;
        }

        var ins = GameObject.Instantiate( m_objPrefab, null );
        var toast = ins.GetComponent<Toast>( );
        Init( toast );
        return toast;
    }

    public void Release( Toast toast )
    {
        if ( !m_arrToastPool.Contains( toast ) )
        {
            m_arrToastPool.Add( toast );
            Reset( toast );
        }
    }


    private void Init( Toast toast ) => Reset( toast );

    private void Reset( Toast toast )
    {
        toast.gameObject.SetActive( false );
        toast.transform.BetterSetParent( null );
    }

}
