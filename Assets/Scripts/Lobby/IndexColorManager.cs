using System;
using System.Collections.Generic;
using UnityEngine;

public class IndexColorManager : MonoBehaviour
{
    [System.Serializable]
    public class indexColor
    {
        public int index;
        public Color color;
    }
    
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private Transform content;
    
    public indexColor[] indexColors;
    public GameObject indexColorHolder;
    
    public List<IndexColorHolder> indexColorHolders;

    private void Start()
    {
        Spawn();
    }

    public void DeSelectEveryColor()
    {
        foreach (var color in indexColorHolders)
        {
            color.selected = false;
            color.GTX_selected.SetActive(false);
        }
    }
    
    void Spawn()
    {
        foreach (var color in indexColors)
        {
            IndexColorHolder ICH = Instantiate(indexColorHolder, content).GetComponent<IndexColorHolder>();
            ICH.ColorIndex = color.index;
            ICH.relayManager = relayManager;
            ICH.indexColorManager = this;
            ICH.colorShower.color = color.color;
            indexColorHolders.Add(ICH);
        }
    }
}
