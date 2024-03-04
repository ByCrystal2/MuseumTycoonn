using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationData : MonoBehaviour
{
    public int id;
    public bool isLocked;
    public bool HasImage;
    // Start is called before the first frame update
    //void Start()
    //{
    //    PictureElement PE;
    //    if (transform.parent.TryGetComponent(out PE))
    //        return;

    //    //NpcManager.instance.Locations.Add(this);
    //}

    public void SetVisittible(bool _isVisittible, bool _hasImage)
    {
        isLocked = !_isVisittible;
        if (_isVisittible)
        {
            if (!NpcManager.instance.Locations.Contains(this))
            {
                NpcManager.instance.Locations.Add(this);
                HasImage = _hasImage;
            }
        }
        else
        {
            if (NpcManager.instance.Locations.Contains(this))
            {
                NpcManager.instance.Locations.Remove(this);
                HasImage = _hasImage;
            }
        }
    }
}
