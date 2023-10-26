using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private bool uiControl;
    public bool UIControl { get { return uiControl; } set { uiControl = value; } }

    
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        Init();        
    }

    public void Init()
    {
        SceneManager.LoadScene("Menu");
        Painter.instance.AddBasePainters();
        TableCommentEvaluationManager.instance.AddAllNPCComments();
        SkillTreeManager.instance.AddSkillsForSkillTree();
        AudioMenuManager.instance.PlayMusicOfMenu();
    }

    
}
