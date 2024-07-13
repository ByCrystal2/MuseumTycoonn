using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardsPanelController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txtTime;
    [SerializeField] GameObject pnlIsReceivedPrefab;
    [SerializeField] GameObject pnlIsLockedPrefab;

    public static DailyRewardsPanelController instance { get; private set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    private void Start()
    {
        InvokeRepeating(nameof(SetTimeText), 0, 1);
    }

    void SetTimeText()
    {
        System.TimeSpan _currentTime = (MuseumManager.instance.lastDailyRewardTime + GameManager.instance.rewardManager.dailyRewardInterval - GameManager.instance.rewardManager.currentTime);
        txtTime.text = $"{_currentTime.Hours:D2}:{_currentTime.Minutes:D2}:{_currentTime.Seconds:D2}";
    }
    public void CreatePnlReceived(Transform _content)
    {
        GameObject receivedObj = Instantiate(pnlIsReceivedPrefab, _content);
    }
    public void CreatePnlLocked(Transform _content)
    {
        Instantiate(pnlIsLockedPrefab, _content);
    }
    public void WinReward(DailyRewardItemOptions _rewardItemOption)
    {
        ItemData _currentRewardItem = ItemManager.instance.CurrentDailyRewardItems.Where(x => x.ID == _rewardItemOption.MyItemID).FirstOrDefault();
        if (_currentRewardItem.CurrentShoppingType != ShoppingType.DailyReward)
        {
            Debug.Log("Mevcut Item Gunluk odul itemi degil => " + _currentRewardItem.CurrentShoppingType);
            return;
        }
        
        if (_currentRewardItem.CurrentItemType == ItemType.Gem)
        {
            _rewardItemOption.SetClickable(true,false);
            _rewardItemOption.GetComponent<Button>().enabled = false;
            int index = ItemManager.instance.CurrentDailyRewardItems.FindIndex(y => y.ID == _currentRewardItem.ID);

            // Eðer bulunduysa
            if (index != -1)
            {
                // Orijinal listedeki öðeyi al
                var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

                // Orijinal öðenin bir kopyasýný oluþtur
                var updatedItem = originalItem;

                // Kopyanýn üzerinde deðiþiklik yap
                updatedItem.IsPurchased = true;
                updatedItem.IsLocked = false;

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            MuseumManager.instance.AddGem(_currentRewardItem.Amount);
        }
        else if (_currentRewardItem.CurrentItemType == ItemType.Gold)
        {
            _rewardItemOption.SetClickable(true, false);
            _rewardItemOption.GetComponent<Button>().enabled = false;
            int index = ItemManager.instance.CurrentDailyRewardItems.FindIndex(y => y.ID == _currentRewardItem.ID);

            // Eðer bulunduysa
            if (index != -1)
            {
                // Orijinal listedeki öðeyi al
                var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

                // Orijinal öðenin bir kopyasýný oluþtur
                var updatedItem = originalItem;

                // Kopyanýn üzerinde deðiþiklik yap
                updatedItem.IsPurchased = true;
                updatedItem.IsLocked = false;

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            MuseumManager.instance.AddGold(_currentRewardItem.Amount);
        }
        else if (_currentRewardItem.CurrentItemType == ItemType.Table)
        {
            _rewardItemOption.SetClickable(true, false);
            _rewardItemOption.GetComponent<Button>().enabled = false;
            int index = ItemManager.instance.CurrentDailyRewardItems.FindIndex(y => y.ID == _currentRewardItem.ID);

            // Eðer bulunduysa
            if (index != -1)
            {
                // Orijinal listedeki öðeyi al
                var originalItem = ItemManager.instance.CurrentDailyRewardItems[index];

                // Orijinal öðenin bir kopyasýný oluþtur
                var updatedItem = originalItem;

                // Kopyanýn üzerinde deðiþiklik yap
                updatedItem.IsPurchased = true;
                updatedItem.IsLocked = false;

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            PictureData newInventoryItem = new PictureData();
            newInventoryItem.TextureID = _currentRewardItem.textureID;
            newInventoryItem.RequiredGold = GameManager.instance.PictureChangeRequiredAmount; ;
            newInventoryItem.painterData = new PainterData(_currentRewardItem.ID, _currentRewardItem.Description, _currentRewardItem.Name, _currentRewardItem.StarCount);
            MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
#if UNITY_EDITOR
             FirestoreManager.instance.UpdateGameData("ahmet123");
#else
             FirestoreManager.instance.UpdateGameData(FirebaseAuthManager.instance.GetCurrentUser().UserId);
#endif
        }
        //GameManager.instance.Save();
    }
    public void ForTutorialUnityEvent()
    {
        DailyRewardItemOptions giftReward = UIController.instance.DailyRewardContents[0].GetChild(0).GetComponent<DailyRewardItemOptions>();
        WinReward(giftReward);
    }
}
