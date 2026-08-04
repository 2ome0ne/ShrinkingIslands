using System;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class EventAnnouncer : NetworkBehaviour
{
    public static EventAnnouncer Instance;
    
    public GameObject eventAnnouncerPrefab;
    public Transform eventAnnouncerContainer;

    private void Awake()
    {
        Instance = this;
    }

    [Rpc(SendTo.Everyone)]
    public void AnnounceEventRpc(string eventName)
    {
        GameObject currentAnnouncer = Instantiate(eventAnnouncerPrefab, eventAnnouncerContainer);
        Debug.Log("ANC: " + eventName + ", currentAnnouncer: " + currentAnnouncer.name);
        currentAnnouncer.GetComponent<AnnounceerPrefab>().Announce(eventName);
    }
}
