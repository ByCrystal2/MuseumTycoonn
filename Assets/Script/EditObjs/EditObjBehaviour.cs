using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class EditObjBehaviour : MonoBehaviour, IPointerClickHandler
{
    public EditObjData data;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!data.isClickable) return;
        RoomEditingPanelController.instance.ClickedEditObjBehaviour = this;
        RoomEditingPanelController.instance.ClickedEditObjBehaviour.data = this.data;
        RoomEditingPanelController.instance.SetStatueBuyingPanelUIs(this.data);
        RoomEditingPanelController.instance.BuyEditObjPanel.SetActive(true);
    }
}
