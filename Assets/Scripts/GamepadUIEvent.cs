using UnityEngine;
using UnityEngine.EventSystems;

public class GamepadUIEvent : MonoBehaviour
{

    [SerializeField]
    GameObject DefaultUIObject;

    public void SetEventObject()
    {
        EventSystem.current.SetSelectedGameObject(DefaultUIObject);
    }

    public static void ClearSelectedUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
