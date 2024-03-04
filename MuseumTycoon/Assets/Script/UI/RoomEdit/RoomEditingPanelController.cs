using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RoomEditingPanelController : MonoBehaviour
{
    
    [SerializeField] Transform EditObjsContent;
    [SerializeField] GameObject EditObj_StatueVariant;
    [SerializeField] GameObject EditObj_DecorationVariant;

    //UI
    [SerializeField] public GameObject BuyEditObjPanel;
    [SerializeField] public Image ClickedEditObjImage;
    public EditObjData ClickedEditObjData;
    List<EditObjData> editObjs = new List<EditObjData>(); // Databaseden cekilmeli
    public static RoomEditingPanelController instance { get; private set; }
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
        AddEditObjs();
    }
    private void AddEditObjs()
    {
        Texture2D gemTexture = Resources.Load<Texture2D>("ItemPictures/ItemGold10000x");
        Texture2D goldTexture = Resources.Load<Texture2D>("ItemPictures/ItemGold50000x");

        List<(EditObjBonusType _bonusses, int _value)> bonuss =  new List<(EditObjBonusType _bonusses, int _value)>();
        bonuss.Add((EditObjBonusType.IncreaseNPCHappiness, 10)); // EXAMPLE!!!
        EditObjData objData = new EditObjData(1, "Statue1", gemTexture, EditObjType.Statue, bonuss);
        EditObjData objData1 = new EditObjData(2, "Statue2", gemTexture, EditObjType.Statue, bonuss);
        EditObjData objData2 = new EditObjData(3, "Statue3", gemTexture, EditObjType.Statue, bonuss);
        EditObjData objData3 = new EditObjData(4, "Statue4", gemTexture, EditObjType.Statue, bonuss);
        EditObjData objData4 = new EditObjData(5, "Statue5", gemTexture, EditObjType.Statue, bonuss);

        EditObjData objData5 = new EditObjData(6, "Decoration1", goldTexture, EditObjType.Decoration, new List<(EditObjBonusType _bonusses, int _value)>());
        EditObjData objData6 = new EditObjData(7, "Decoration2", goldTexture, EditObjType.Decoration, new List<(EditObjBonusType _bonusses, int _value)>());
        EditObjData objData7 = new EditObjData(8, "Decoration3", goldTexture, EditObjType.Decoration, new List<(EditObjBonusType _bonusses, int _value)>());
        EditObjData objData8 = new EditObjData(9, "Decoration4", goldTexture, EditObjType.Decoration, new List<(EditObjBonusType _bonusses, int _value)>());
        EditObjData objData9 = new EditObjData(10, "Decoration5", goldTexture, EditObjType.Decoration, new List<(EditObjBonusType _bonusses, int _value)>());

        editObjs.Clear();
        //Statues
        editObjs.Add(objData);
        editObjs.Add(objData1);
        editObjs.Add(objData2);
        editObjs.Add(objData3);
        editObjs.Add(objData4);
        //Decorations
        editObjs.Add(objData5);
        editObjs.Add(objData6);
        editObjs.Add(objData7);
        editObjs.Add(objData8);
        editObjs.Add(objData9);
    }
    private void ClearEditObjContent()
    {
        int length = EditObjsContent.childCount;
        if (length <= 0) return;
        for (int i = 0; i < length; i++)
        {
            Destroy(EditObjsContent.GetChild(i).gameObject);
        }
    }
    public void AddAndBonusCalculator(EditObjData _addingBonusEditObj)
    {
        List<(EditObjBonusType _Enum, int _Value)> Bonusses = new List<(EditObjBonusType _Enum, int _Value)>();
        Debug.Log(" _addingBonusEditObj.BonusEnums[0] Before =>" + _addingBonusEditObj.BonusEnums[0]._bonusses + _addingBonusEditObj.BonusEnums[0]._value);

        Bonusses =_addingBonusEditObj.BonusEnums;
        Debug.Log(" _addingBonusEditObj.BonusEnums[0] After =>" + Bonusses[0]._Enum + Bonusses[0]._Value);

        _addingBonusEditObj._currentRoom = RoomManager.instance.CurrentEditedRoom;
        _addingBonusEditObj._currentRoom.SetMyStatue(_addingBonusEditObj);
        List<NPCBehaviour> _npcsInTheRoom = _addingBonusEditObj._currentRoom.GetNpcsInTheMyRoom();
        foreach (var currentBonus in Bonusses)
        {
            Debug.Log("_npcsInTheRoom.Count => " + _npcsInTheRoom.Count + " CurrentBonus => " +  currentBonus._Enum);
            foreach (var npc in _npcsInTheRoom)
            {
                Debug.Log("CurrentNpc => " + npc.name);
                switch (currentBonus._Enum)
                {
                    case EditObjBonusType.None:
                        break;
                    case EditObjBonusType.IncreaseNPCHappiness:
                        Debug.Log("Before => npc.Happiness += currentBonus._Value => "  + npc.Happiness +"+="+ currentBonus._Value);
                        npc.Happiness += currentBonus._Value;
                        Debug.Log("After => npc.Happiness += currentBonus._Value => " + npc.Happiness);
                        break;
                    case EditObjBonusType.ReduceNpcFightRate:
                        // npc.FightingRate += 10;
                        break;
                    case EditObjBonusType.ReduceNpcUrinationRate:
                        // npc.UrinationgRate += 10;
                        break;
                    case EditObjBonusType.NPCsMustToPay:
                        MuseumManager.instance.AddGold(currentBonus._Value);
                        break;
                    case EditObjBonusType.DancingNPCsEmerge:
                        // Dans Eden Npcler Ortaya Cikiyor
                        break;
                    case EditObjBonusType.CowGivingMilkToNPCs:
                        // Inek heykel her 10 dkda bir sut veriyor
                        break;
                    case EditObjBonusType.IfArtistPaintingInRoom:
                        // Eger ressam olan heykelin cizimleri odada varsa npclere mutluluk ver
                        break;
                    default:
                        break;
                }
            }        
        }
    }
    public void AddBonusEnteredInTheRoom(RoomData _currentRoom, NPCBehaviour _currentNpc)
    {
        List<(EditObjBonusType _Enum, int _Value)> Bonusses = _currentRoom.GetMyStatueInTheMyRoom().BonusEnums;
        foreach (var currentBonus in Bonusses)
        {            
                switch (currentBonus._Enum)
                {
                    case EditObjBonusType.None:
                        break;
                    case EditObjBonusType.IncreaseNPCHappiness:
                    Debug.Log("Before => Current Room Bonus => " + EditObjBonusType.IncreaseNPCHappiness + "Current NPC Happiness => " + _currentNpc.Happiness);
                    _currentNpc.Happiness += currentBonus._Value;
                    Debug.Log("After => Current Room Bonus => " + EditObjBonusType.IncreaseNPCHappiness + "Current NPC Happiness => " + _currentNpc.Happiness);
                        break;
                    case EditObjBonusType.ReduceNpcFightRate:
                        // npc.FightingRate += 10;
                        break;
                    case EditObjBonusType.ReduceNpcUrinationRate:
                        // npc.UrinationgRate += 10;
                        break;
                    case EditObjBonusType.NPCsMustToPay:
                        MuseumManager.instance.AddGold(currentBonus._Value);
                        break;
                    case EditObjBonusType.DancingNPCsEmerge:
                        // Dans Eden Npcler Ortaya Cikiyor
                        break;
                    case EditObjBonusType.CowGivingMilkToNPCs:
                        // Inek heykel her 10 dkda bir sut veriyor
                        break;
                    case EditObjBonusType.IfArtistPaintingInRoom:
                        // Eger ressam olan heykelin cizimleri odada varsa npclere mutluluk ver
                        break;
                    default:
                        break;
                }
            Debug.Log($" {_currentRoom.availableRoomCell.CellLetter} {_currentRoom.availableRoomCell.CellNumber} + adli Oda'nin, {currentBonus._Value} degerinde ki buffu, mevcut {_currentNpc.name} adli npc'ye/odaya eklendi...");
        }
    }
    public void RemoveBonusExitedInTheRoom(RoomData _currentRoom, NPCBehaviour _currentNpc)
    {
        List<(EditObjBonusType _Enum, int _Value)> Bonusses = _currentRoom.GetMyStatueInTheMyRoom().BonusEnums;
        foreach (var currentBonus in Bonusses)
        {

            switch (currentBonus._Enum)
            {
                case EditObjBonusType.None:
                    break;
                case EditObjBonusType.IncreaseNPCHappiness:
                    _currentNpc.Happiness -= currentBonus._Value;
                    break;
                case EditObjBonusType.ReduceNpcFightRate:
                    // npc.FightingRate -= 10;
                    break;
                case EditObjBonusType.ReduceNpcUrinationRate:
                    // npc.UrinationgRate -= 10;
                    break;
                case EditObjBonusType.NPCsMustToPay:
                    break;
                case EditObjBonusType.DancingNPCsEmerge:
                    break;
                case EditObjBonusType.CowGivingMilkToNPCs:
                    // Inek heykel her 10 dkda bir sut veriyor
                    break;
                case EditObjBonusType.IfArtistPaintingInRoom:
                    // Eger ressam olan heykelin cizimleri odada varsa npclere mutluluk ver (-)
                    break;
                default:
                    break;
            }

        Debug.Log($" {_currentRoom.availableRoomCell.CellLetter} {_currentRoom.availableRoomCell.CellNumber} + adli Oda'nin, {currentBonus._Value} degerinde ki buffu, mevcut {_currentNpc.name} adli npc'den kaldirildi...");
        }
    }
    public void AddStatuesInContent() // pnlRoomEditing / pnlRoomObjs / btnStatue
    {
        BuyEditObjPanel.SetActive(false);
        ClearEditObjContent();
        List<EditObjData> _statues = new List<EditObjData>();
        _statues = editObjs.Where(x=> x.EditType == EditObjType.Statue).ToList();
        int length = _statues.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject _newStatue = Instantiate(EditObj_StatueVariant, EditObjsContent);
            EditObjData MyEditObj = _newStatue.AddComponent<EditObjData>();
            EditObjData _newData = _statues[i];
            MyEditObj.ID = _newData.ID;
            MyEditObj.Name = _newData.Name;
            MyEditObj.EditType = _newData.EditType;
            MyEditObj.ImageSprite = _newData.ImageSprite;
            MyEditObj._currentRoom = _newData._currentRoom;
            MyEditObj.BonusEnums = new List<(EditObjBonusType _bonusses, int _value)>();
            MyEditObj.BonusEnums.Clear();
            int length1 = _newData.BonusEnums.Count;
            for (int j = 0; j < length1; j++)
            {
                MyEditObj.BonusEnums.Add(_newData.BonusEnums[j]);
            }
            _newStatue.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = _newData.ImageSprite;
        }
    }
    public void AddDecorationInContent() // pnlRoomEditing / pnlRoomObjs / btnDecoration
    {
        BuyEditObjPanel.SetActive(false);
        ClearEditObjContent();
        List<EditObjData> _decoration = new List<EditObjData>();
        _decoration = editObjs.Where(x => x.EditType == EditObjType.Decoration).ToList();
        int length = _decoration.Count;
        for (int i = 0; i < length; i++)
        {
            GameObject _newStatue = Instantiate(EditObj_DecorationVariant, EditObjsContent);
            EditObjData MyEditObj = _newStatue.AddComponent<EditObjData>();
            EditObjData _newData = _decoration[i];
            MyEditObj.ID = _newData.ID;
            MyEditObj.Name = _newData.Name;
            MyEditObj.EditType = _newData.EditType;
            MyEditObj.ImageSprite = _newData.ImageSprite;
            MyEditObj._currentRoom = _newData._currentRoom;

            MyEditObj.BonusEnums = new List<(EditObjBonusType _bonusses, int _value)>();
            MyEditObj.BonusEnums.Clear();
            int length1 = _newData.BonusEnums.Count;
            for (int j = 0; j < length1; j++)
            {
                MyEditObj.BonusEnums.Add(_newData.BonusEnums[j]);
            }

            _newStatue.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = _newData.ImageSprite;

        }
    }
    public void AddBonusActivation() // pnlRoomEditing / pnlBuyTheEditObj / btnBuyEditObj ~onClick
    {
        if (ClickedEditObjData.GetIsPurchased())
        {
            Debug.Log("Obje Satilmis. ");
            return;
        }
        ClickedEditObjData._currentRoom = RoomManager.instance.CurrentEditedRoom;
        ClickedEditObjData.SetIsPurchased();
    }
}

public enum EditObjType
{
    None,
    Statue,
    Decoration
}