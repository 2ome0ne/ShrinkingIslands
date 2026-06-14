using System;
using UnityEngine;
using UnityEngine.UI;
public class IndexColorHolder : MonoBehaviour
{
    public RelayManager relayManager;
    public Image colorShower;
    public IndexColorManager indexColorManager;
    public GameObject GTX_selected;
    public int ColorIndex;

    public bool selected;
    

    public void Click()
    {
        indexColorManager.DeSelectEveryColor();
        relayManager.player_color_index = ColorIndex;
        GTX_selected.SetActive(true);
    }
}
