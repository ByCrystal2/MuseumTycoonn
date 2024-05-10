using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatuesHandler
{
    public GameObject[] Statues; // 0 1 2 3 4 5 6 7 

    public List<EditObjData> editObjs = new List<EditObjData>();
    public List<EditObjData> currentEditObjs = new List<EditObjData>();
    public List<EditObjData> activeEditObjs = new List<EditObjData>(); // Databaseden cekilmeli

    private const string statuesResourcesPath = "Statues";

    private const string statueSpritesResourcesPath = "StatueSprites";
    private const string decorationSpritesResourcesPath = "DecorationSprites";
    public StatuesHandler()
    {
    }
    public void AddEditObjs()
    {
        Statues = Resources.LoadAll<GameObject>(statuesResourcesPath);
        Object[] statueResults = Resources.LoadAll(statueSpritesResourcesPath, typeof(Texture2D));
        Object[] decorationResults = Resources.LoadAll(decorationSpritesResourcesPath, typeof(Texture2D));


        List<Bonus> bonuss = new List<Bonus>();
        Bonus bonus1 = new Bonus(EditObjBonusType.IncreaseNPCHappiness, 10);
        bonuss.Add(bonus1); // EXAMPLE!!!
        EditObjData objData = new EditObjData(1, "Statue1", 1000, (Texture2D)statueResults[0], EditObjType.Statue, bonuss, 0, 0);
        EditObjData objData1 = new EditObjData(2, "Statue2", 2000, (Texture2D)statueResults[1], EditObjType.Statue, bonuss, 5, 1);
        EditObjData objData2 = new EditObjData(3, "Statue3", 3000, (Texture2D)statueResults[2], EditObjType.Statue, bonuss, 10, 2);
        EditObjData objData3 = new EditObjData(4, "Statue4", 4000, (Texture2D)statueResults[3], EditObjType.Statue, bonuss, 15, 3);
        EditObjData objData4 = new EditObjData(5, "Statue5", 5000, (Texture2D)statueResults[4], EditObjType.Statue, bonuss, 20, 4);

        EditObjData objData5 = new EditObjData(6, "Decoration1", 100, (Texture2D)decorationResults[0], EditObjType.Decoration, new List<Bonus>(), 5);
        EditObjData objData6 = new EditObjData(7, "Decoration2", 200, (Texture2D)decorationResults[1], EditObjType.Decoration, new List<Bonus>(), 5);
        EditObjData objData7 = new EditObjData(8, "Decoration3", 300, (Texture2D)decorationResults[2], EditObjType.Decoration, new List<Bonus>(), 5);
        EditObjData objData8 = new EditObjData(9, "Decoration4", 400, (Texture2D)decorationResults[3], EditObjType.Decoration, new List<Bonus>(), 5);
        EditObjData objData9 = new EditObjData(10, "Decoration5", 500, (Texture2D)decorationResults[4], EditObjType.Decoration, new List<Bonus>(), 5);

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


        currentEditObjs = editObjs;
        foreach (var statue in activeEditObjs)
        {
            if (editObjs.Contains(statue))
            {
                currentEditObjs.Remove(statue);
            }            
        }
    }

    public void AddAndBonusCalculator(EditObjData _addingBonusEditObj)
    {
        List<Bonus> Bonusses = new List<Bonus>();
        Debug.Log(" _addingBonusEditObj.Bonusses[0] Before =>" + _addingBonusEditObj.Bonusses[0].BonusType + _addingBonusEditObj.Bonusses[0].Value);

        Bonusses = _addingBonusEditObj.Bonusses;
        Debug.Log(" _addingBonusEditObj.Bonusses[0] After =>" + Bonusses[0].BonusType + Bonusses[0].Value);

        _addingBonusEditObj._currentRoomCell = RoomManager.instance.CurrentEditedRoom.availableRoomCell;

        RoomData bonusStatueRoom = RoomManager.instance.GetRoomWithRoomCell(_addingBonusEditObj._currentRoomCell);
        bonusStatueRoom.SetMyStatue(_addingBonusEditObj);
        List<NPCBehaviour> _npcsInTheRoom = bonusStatueRoom.GetNpcsInTheMyRoom();
        foreach (var currentBonus in Bonusses)
        {
            Debug.Log("_npcsInTheRoom.Count => " + _npcsInTheRoom.Count + " CurrentBonus => " + currentBonus.BonusType);
            foreach (var npc in _npcsInTheRoom)
            {
                Debug.Log("CurrentNpc => " + npc.name);
                switch (currentBonus.BonusType)
                {
                    case EditObjBonusType.None:
                        break;
                    case EditObjBonusType.IncreaseNPCHappiness:
                        Debug.Log("Before => npc.Happiness += currentBonus._Value => " + npc.Happiness + "+=" + currentBonus.Value);
                        npc.Happiness += currentBonus.Value;
                        Debug.Log("After => npc.Happiness += currentBonus._Value => " + npc.Happiness);
                        break;
                    case EditObjBonusType.ReduceNpcFightRate:
                        // npc.FightingRate += 10;
                        break;
                    case EditObjBonusType.ReduceNpcUrinationRate:
                        // npc.UrinationgRate += 10;
                        break;
                    case EditObjBonusType.NPCsMustToPay:
                        MuseumManager.instance.AddGold(currentBonus.Value);
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
        List<Bonus> Bonusses = _currentRoom.GetMyStatueInTheMyRoom().Bonusses;
        Debug.Log("Bonusses.Count => " + Bonusses.Count);
        foreach (var currentBonus in Bonusses)
        {
            switch (currentBonus.BonusType)
            {
                case EditObjBonusType.None:
                    break;
                case EditObjBonusType.IncreaseNPCHappiness:
                    Debug.Log("Before => Current Room Bonus => " + EditObjBonusType.IncreaseNPCHappiness + "Current NPC Happiness => " + _currentNpc.Happiness);
                    _currentNpc.Happiness += currentBonus.Value;
                    Debug.Log("After => Current Room Bonus => " + EditObjBonusType.IncreaseNPCHappiness + "Current NPC Happiness => " + _currentNpc.Happiness);
                    break;
                case EditObjBonusType.ReduceNpcFightRate:
                    // npc.FightingRate += 10;
                    break;
                case EditObjBonusType.ReduceNpcUrinationRate:
                    // npc.UrinationgRate += 10;
                    break;
                case EditObjBonusType.NPCsMustToPay:
                    MuseumManager.instance.AddGold(currentBonus.Value);
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
            Debug.Log($" {_currentRoom.availableRoomCell.CellLetter} {_currentRoom.availableRoomCell.CellNumber} + adli Oda'nin, {currentBonus.Value} degerinde ki buffu, mevcut {_currentNpc.name} adli npc'ye/odaya eklendi...");
        }
    }
    public void RemoveBonusExitedInTheRoom(RoomData _currentRoom, NPCBehaviour _currentNpc)
    {
        List<Bonus> Bonusses = _currentRoom.GetMyStatueInTheMyRoom().Bonusses;
        foreach (var currentBonus in Bonusses)
        {

            switch (currentBonus.BonusType)
            {
                case EditObjBonusType.None:
                    break;
                case EditObjBonusType.IncreaseNPCHappiness:
                    _currentNpc.Happiness -= currentBonus.Value;
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

            Debug.Log($" {_currentRoom.availableRoomCell.CellLetter} {_currentRoom.availableRoomCell.CellNumber} + adli Oda'nin, {currentBonus.Value} degerinde ki buffu, mevcut {_currentNpc.name} adli npc'den kaldirildi...");
        }
    }

    
}
