using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorUserSignCanvasController : MonoBehaviour
{
    [SerializeField] Toggle rememberMe_Toggle;
    [SerializeField] Transform content;
    [SerializeField] EditorUserHandler editUserUIPrefab;
    public static EditorUserSignCanvasController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    private void OnEnable()
    {
        ClearContent();
        List<DatabaseUser> users = FirebaseAuthManager.instance.editorUsers;
        foreach (DatabaseUser user in users)
        {
            EditorUserHandler userHandler = Instantiate(editUserUIPrefab, content);
            userHandler.InitEditorUser(user);
        }
    }
    void ClearContent()
    {
        int length = content.childCount;
        for (int i = 0; i < length; i++)
            if (content.GetChild(i).TryGetComponent(out EditorUserHandler handler))
                Destroy(handler.gameObject);
    }
    public bool IsRememberMe()
    {
        return rememberMe_Toggle.isOn;
    }
}
