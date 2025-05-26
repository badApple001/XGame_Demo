using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Toast : MonoBehaviour
{
    public Text m_textChat;
    private uint m_handle;
    public void BindResHandle( uint handle ) => m_handle = handle;
    public uint GetResHandle( ) => m_handle;    


    public void Show( string text, float duration = 2f )
    {
        m_textChat.text = text;
        gameObject.SetActive( true );
        StartCoroutine( HideAfterDuration( duration ) );
    }

    private IEnumerator HideAfterDuration( float duration )
    {
        yield return new WaitForSeconds( duration );
        gameObject.SetActive( false );
        ToastManager.Instance.Release( this );
    }

}
