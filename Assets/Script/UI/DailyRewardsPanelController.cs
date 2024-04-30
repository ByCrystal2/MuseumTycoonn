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
        InvokeRepeating(nameof(StartSetTimeTextIE), 0f, 1f);
    }
    private void Update()
    {
        if (RewardManager.instance.CheckTheInRewardControl()) RewardManager.instance.CheckRewards();
    }
    public void StartSetTimeTextIE()
    {
            StartCoroutine(SetTimeText()); // DailyRewards paneli aktifse calisacak kod. (if kontrolu ona gore saglanmali.)

    }
    IEnumerator SetTimeText()
    {
        System.DateTime currentTime = RewardManager.instance.GetTimeRemaining();
        txtTime.text = currentTime.ToString("HH:mm:ss");
        yield return new WaitForSecondsRealtime(1f);
    }
    public void CreatePnlReceived(Transform _content)
    {
        Instantiate(pnlIsReceivedPrefab, _content);
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
            _rewardItemOption.SetClickable(false);
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

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            MuseumManager.instance.AddGem(_currentRewardItem.Amount);
        }
        else if (_currentRewardItem.CurrentItemType == ItemType.Gold)
        {
            _rewardItemOption.SetClickable(false);
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

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            MuseumManager.instance.AddGold(_currentRewardItem.Amount);
        }
        else if (_currentRewardItem.CurrentItemType == ItemType.Table)
        {
            _rewardItemOption.SetClickable(false);
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

                // Kopyayý orijinal listeye geri yerleþtir
                ItemManager.instance.CurrentDailyRewardItems[index] = updatedItem;
            }
            PictureData newInventoryItem = new PictureData();
            newInventoryItem.TextureID = _currentRewardItem.textureID;
            newInventoryItem.RequiredGold = PicturesMenuController.instance.PictureChangeRequiredAmount;
            newInventoryItem.painterData = new PainterData(_currentRewardItem.ID, _currentRewardItem.Description, _currentRewardItem.Name, _currentRewardItem.StarCount);
            MuseumManager.instance.AddNewItemToInventory(newInventoryItem);
        }
        GameManager.instance.Save();
    }
}
