using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumManager : MonoBehaviour
{
    public static MuseumManager instance { get; private set; }
    public List<PictureElementData> MyPictures = new List<PictureElementData>();
    public List<NPCBehaviour> CurrentNpcs = new List<NPCBehaviour>();

    public Sprite EmptyPictureSprite;

    protected float Gold, Culture, Gem;
    protected int CurrentCultureLevel;
    protected float SmootherCultureExp;

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

        //Kac adet npc var buraya guncellenicek.
    }

    public void OnNpcPaid()
    {
        Gold += GetTicketPrice();
        UIController.instance.GoldText.text = "" + Gold;
        Debug.Log("An npc entered Museum. New gold: " + Gold);
    }

    public void OnNpcExitedMuseum(NPCBehaviour _oldNpc)
    {
        if (CurrentNpcs.Contains(_oldNpc))
            CurrentNpcs.Remove(_oldNpc);

        Debug.Log("An npc left Museum.");
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

    public void CultureLevelUP()
    {
        UIController.instance.CultureLevelText.text = "" + CurrentCultureLevel;
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
