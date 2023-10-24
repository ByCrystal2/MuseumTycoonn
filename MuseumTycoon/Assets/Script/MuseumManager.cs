using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager instance { get; private set; }
    public List<PictureElementData> MyPictures = new List<PictureElementData>();
    public List<NPCBehaviour> CurrentNpcs = new List<NPCBehaviour>();

    public Sprite EmptyPictureSprite;

    protected float Gold, Culture, Gem, currentGameTime;
    protected int CurrentCultureLevel;
    protected float SmootherCultureExp;
    public int InMuseumCurrentNPCCount;
    public int TotalVisitorCommentCount;
    public float TotalVisitorHappiness;
    public float DailyEarning;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        CatchTheColorForAll();

        CurrentCultureLevel = 1;
    }

    private void FixedUpdate()
    {
        if (SmootherCultureExp < Culture)
        {
            SmoothTheExp(true);
        }
        else if (SmootherCultureExp > Culture)
        {
            SmoothTheExp(false);
        }
    }

    void SmoothTheExp(bool _isIncrease)
    {
        if (_isIncrease)
        {
            SmootherCultureExp += Time.deltaTime * (0.05f * GetRequiredCultureExp());
            if (SmootherCultureExp > Culture)
                SmootherCultureExp = Culture;
        }
        else
        {
            SmootherCultureExp -= Time.deltaTime * (0.05f * GetRequiredCultureExp());
            if (SmootherCultureExp < Culture)
                SmootherCultureExp = Culture;
        }

        UIController.instance.CultureFillBar.fillAmount = SmootherCultureExp / GetRequiredCultureExp();
    }

    void CatchTheColorForAll()
    {
        int length = MyPictures.Count;
        for (int i = 0; i < length; i++)
        {
            MyPictures[i].id = (i + 1);
            MyPictures[i].MostCommonColors = CatchTheColors.instance.FindMostUsedColors(MyPictures[i].texture);
        }
    }

    public void OnNpcEnteredMuseum(NPCBehaviour _newNpc)
    {
        if(!CurrentNpcs.Contains(_newNpc))
            CurrentNpcs.Add(_newNpc);
        UIController.instance.InMuseumCurrentNPCCountChanged(GetInMuseumVisitorCount());
        //Kac adet npc var buraya guncellenicek.
    }

    public void OnNpcPaid()
    {
        Gold += GetTicketPrice();
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log("An npc entered Museum. New gold: " + Gold);
        DailyEarning = Gold;
        UIController.instance.InMuseumDailyEarningChanged(DailyEarning);
    }

    public void OnNpcExitedMuseum(NPCBehaviour _oldNpc)
    {
        if (CurrentNpcs.Contains(_oldNpc))
            CurrentNpcs.Remove(_oldNpc);
        UIController.instance.InMuseumCurrentNPCCountChanged(GetInMuseumVisitorCount());
        Debug.Log("An npc left Museum.");
        
    }
    public void OnNpcCommentedPicture(NPCBehaviour _npc, PictureElement _pictureElement, float starCount)
    {
        List<TableCommentEvaluationData> randomDatas = TableCommentEvaluationManager.instance.GetComment(starCount);
        TableCommentEvaluationData randomData = randomDatas[Random.Range(0, randomDatas.Count)];
        UIController.instance.AddCommentInGlobalTab(EmptyPictureSprite, _npc.name, randomData.Message, currentGameTime.ToString());
    }
    public void AddTotalVisitorCommentCount(int count)
    {
        TotalVisitorCommentCount += count;
        UIController.instance.TotalVisitorsCommentCountChanged(TotalVisitorCommentCount);
    }
    public bool IsMuseumFull()
    {
        return (CurrentNpcs.Count == GetMuseumCurrentCapacity());
    }

    public void AddCultureExp(float _xp)
    {
        Culture += _xp;
        if (Culture > GetRequiredCultureExp())
        {
            Culture -= GetRequiredCultureExp();
            CurrentCultureLevel++;
            CultureLevelUP();
        }
    }
    public int GetInMuseumVisitorCount()
    {
        return CurrentNpcs.Count;        
    }
    public void CultureLevelUP()
    {
        UIController.instance.CultureLevelText.text = "" + CurrentCultureLevel;
        UIController.instance.CultureLevelCountChanged(CurrentCultureLevel);
        Debug.Log("Yeni levelde kazanilan bonuslari buraya islicez.");
    }

    public int GetRequiredCultureExp()
    {
        return CultureLevel[CurrentCultureLevel];
    }

    public int GetMuseumCurrentCapacity()
    {
        return MaxVisitorPerCultureLevel[CurrentCultureLevel];
    }

    public int GetTicketPrice()
    {
        return TicketPricePerCultureLevel[CurrentCultureLevel];
    }

    public List<int> CultureLevel = new List<int>() 
    { 0, 100, 225, 450, 600, 800, 1050, 1350, 1700, 2100, 2650,
        3200, 3750, 4600, 5300, 6000, 999999
    };

    public List<int> MaxVisitorPerCultureLevel = new List<int>()
    {
        0, 5,  6,  7,  8,  9,  10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
    };

    public List<int> TicketPricePerCultureLevel = new List<int>()
    {
        0, 300,  305,  310,  315,  320,  325, 330, 335, 340, 345, 350, 358, 366, 374, 382, 390
    };
}
