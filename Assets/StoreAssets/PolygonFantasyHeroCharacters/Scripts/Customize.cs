using System.Collections.Generic;
using UnityEngine;

public class Customize : MonoBehaviour
{
    public void UpdateVisual(PlayerExtraCustomizeData _data)
    {
        PlayerExtraCustomizeData data = _data;

        Dictionary<CustomizeSlot, GameObject> equipped = new();

        foreach (var item in data.CustomizeElements)
        {
            equipped[item.customizeSlot] = null;
            Transform elementParent = null;
            if((int)item.customizeSlot < 100)
            {
                Transform GenderHolder = data.isFemale ? transform.GetChild(0) : transform.GetChild(1);
                elementParent = GenderHolder.GetChild((int)item.customizeSlot);

                for (int i = 0; i <= 1; i++)
                    foreach (Transform other in transform.GetChild(i).GetChild((int)item.customizeSlot))
                        other.gameObject.SetActive(false);
            }
            else
            {
                elementParent = transform.GetChild(((int)item.customizeSlot) - 100);
            }

            int baseID = (int)item.customizeSlot * 1000;
            int index = item.elementID - baseID;

            int length = elementParent.childCount;
            for (int i = 0; i < length; i++)
            {
                if(index == (i + 1))
                {
                    equipped[item.customizeSlot] = elementParent.GetChild(i).gameObject;
                    elementParent.GetChild(i).gameObject.SetActive(true);
                }
                else
                    elementParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        FixOverrides(equipped);
    }

    void FixOverrides(Dictionary<CustomizeSlot, GameObject> _equipped)
    {
        if (_equipped.ContainsKey(CustomizeSlot.Helmet))
        {
            if (_equipped[CustomizeSlot.Helmet] != null)
            {
                if (_equipped.ContainsKey(CustomizeSlot.Hair) && _equipped[CustomizeSlot.Hair] != null)
                    _equipped[CustomizeSlot.Hair].SetActive(false);
                if (_equipped.ContainsKey(CustomizeSlot.Head) && _equipped[CustomizeSlot.Head] != null)
                    _equipped[CustomizeSlot.Head].SetActive(false);
                if (_equipped.ContainsKey(CustomizeSlot.FacialHair) && _equipped[CustomizeSlot.FacialHair] != null)
                    _equipped[CustomizeSlot.FacialHair].SetActive(false);
                if (_equipped.ContainsKey(CustomizeSlot.Eyebrows) && _equipped[CustomizeSlot.Eyebrows] != null)
                    _equipped[CustomizeSlot.Eyebrows].SetActive(false);
                if (_equipped.ContainsKey(CustomizeSlot.Mask) && _equipped[CustomizeSlot.Mask] != null)
                    _equipped[CustomizeSlot.Mask].SetActive(false);
                if (_equipped.ContainsKey(CustomizeSlot.Hat) && _equipped[CustomizeSlot.Hat] != null)
                    _equipped[CustomizeSlot.Hat].SetActive(false);
                if (_equipped.ContainsKey(CustomizeSlot.Elf_Ear) && _equipped[CustomizeSlot.Elf_Ear] != null)
                    _equipped[CustomizeSlot.Elf_Ear].SetActive(false);
            }
        }

        if (_equipped.ContainsKey(CustomizeSlot.Mask) && _equipped[CustomizeSlot.Mask] != null)
        {
            if (_equipped.ContainsKey(CustomizeSlot.FacialHair) && _equipped[CustomizeSlot.FacialHair] != null)
                _equipped[CustomizeSlot.FacialHair].SetActive(false);
            if (_equipped.ContainsKey(CustomizeSlot.Eyebrows) && _equipped[CustomizeSlot.Eyebrows] != null)
                _equipped[CustomizeSlot.Eyebrows].SetActive(false);
        }

        if (_equipped.ContainsKey(CustomizeSlot.Hat) && _equipped[CustomizeSlot.Hat] != null)
        {
            if (_equipped.ContainsKey(CustomizeSlot.Hair) && _equipped[CustomizeSlot.Hair] != null)
                _equipped[CustomizeSlot.Hair].SetActive(false);
            if (_equipped.ContainsKey(CustomizeSlot.Elf_Ear) && _equipped[CustomizeSlot.Elf_Ear] != null)
                _equipped[CustomizeSlot.Elf_Ear].SetActive(false);
        }
    }

}
