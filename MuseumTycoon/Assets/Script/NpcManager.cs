using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public static NpcManager instance { get; private set; }
    public NPCBehaviour CurrentNPC; // npc information panelde gorunen npc! (uzerine tiklanan npc)
    public List<LocationData> Locations = new List<LocationData>();

    public List<Transform> GidisListe = new List<Transform>();
    public List<Transform> GelisListe = new List<Transform>();
    public List<Transform> HorseCartList = new List<Transform>();

    public Transform Enter1Point, Enter2Point;

    public Transform RoomsParent;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);
        GameManager.instance.LoadPictures(RoomsParent, true);
        GameManager.instance.LoadRooms();
        WorkerManager.instance.BaseAllWorkerOptions();
        GameManager.instance.LoadWorkers();
        WorkerManager.instance.CreateWorkersToMarket();
        GameManager.instance.LoadPurchasedItems();     

        //Gaming Services Activation
        RoomManager.instance.AddRooms(); // in app baglantisi kurulmadan once odalar yuklendi.
        BuyingConsumables.instance.InitializePurchasing();
        UnityAdsManager.instance.Initialize();
        UnityAdsManager.instance.CreateBannerView();
        UnityAdsManager.instance.LoadBannerAd();
        UnityAdsManager.instance.ShowBannerAd();
    }
    private void Start()
    {
        AudioManager.instance.PlayMusicOfGame();
        Transform skillsContentTransform = UIController.instance.skillsContent.transform;
        int length = skillsContentTransform.childCount;
        for (int i = 0; i < length; i++)
        {
            if (skillsContentTransform.GetChild(i).TryGetComponent(out BaseSkillOptions component))
            {
                SkillTreeManager.instance.AddSkillObject(skillsContentTransform.GetChild(i).gameObject);

            }            
        }        
        GameManager.instance.LoadSkills();
        ItemManager.instance.SetCalculatedDailyRewardItems();        
        RewardManager.instance.CheckRewards();// Burada gecen sureleri kontrol et ve odul verme durumunu degerlendir.
    }
}
