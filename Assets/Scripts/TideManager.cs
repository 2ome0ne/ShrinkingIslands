using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TideManager : NetworkBehaviour
{
    public enum TideType
    {
        lowTide,
        mediumTide,
        highTide
    }
    
    [Header("-Settings-")] [SerializeField]
    private float currentTide;
    [SerializeField] private float maxTide;
    [SerializeField] private float minTide;

    public TideType tideType;

    //All Tide Y levels
    public float midTideY;
    public float lowTideY;
    public float HighTideY;

    private bool finishedMoving;
    [SerializeField] private float tideTransitionSpeed;

    [SerializeField] private Transform _theSea;
    [SerializeField] private float WaitTimeTide;

    [Header("announcerStuff")] 
    [SerializeField] private string lowtideIncoming = "Low Tide";
    [SerializeField] private string hightideIncoming = "High Tide";
    [SerializeField] private string midtideIncoming  = "Normal Tide";

    [SerializeField] private bool Testing;
    
    public override void OnNetworkSpawn()
    {
        if(!IsHost) return;
        SetCurrentTide();

        if (Testing)
        {
            SetTideTypeRpc(TideType.lowTide);
            SetCurrentTide();
        }
    }

    private void SetCurrentTide()
    {
        currentTide = Random.Range(minTide, maxTide);
    }

    private void Update()
    {
        if(!IsHost) return;
        MoveTide();
        if (currentTide <= 0 && !Testing)
        {
            Invoke(nameof(allowMove), WaitTimeTide);
            switch (tideType)
            {
                case TideType.lowTide:
                    if (Random.Range(0, 100) < 50)
                    {
                        //go high tide
                        SetTideTypeRpc(TideType.highTide);
                        EventAnnouncer.Instance.AnnounceEventRpc(hightideIncoming);
                    }
                    else
                    {
                        //go mid tide
                        SetTideTypeRpc(TideType.mediumTide);
                        EventAnnouncer.Instance.AnnounceEventRpc(midtideIncoming);
                    }
                    break;
                case TideType.mediumTide:
                    if (Random.Range(0, 100) < 50)
                    {
                        //go low tide
                        SetTideTypeRpc(TideType.lowTide);
                        EventAnnouncer.Instance.AnnounceEventRpc(lowtideIncoming);
                    }
                    else
                    {
                        //go high tide
                        SetTideTypeRpc(TideType.highTide);
                        EventAnnouncer.Instance.AnnounceEventRpc(hightideIncoming);
                    }
                    break;
                case TideType.highTide:
                    if (Random.Range(0, 100) < 50)
                    {
                        //go low tide
                        SetTideTypeRpc(TideType.lowTide);
                        EventAnnouncer.Instance.AnnounceEventRpc(lowtideIncoming);
                    }
                    else
                    {
                        //go mid tide
                        SetTideTypeRpc(TideType.mediumTide);
                        EventAnnouncer.Instance.AnnounceEventRpc(midtideIncoming);
                    }
                    break;
            }
            SetCurrentTide();
        }
        else if (Testing && currentTide <= 0)
        {
            SetTideTypeRpc(TideType.lowTide);
            SetCurrentTide();
        }
        else
        {
            currentTide -= Time.deltaTime;
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetTideTypeRpc(TideType _tideType)
    {
        tideType = _tideType;
    }

    private void allowMove()
    {
        Debug.Log("MOVE NOW");
        finishedMoving = false;
        GameManager.Instance.soundManager.SpawnSoundRpc(transform.position , 100 , 0.4f , 1 , 11);
    }

    private void MoveTide()
    {
        if(finishedMoving) return;
        switch (tideType)
        {
            case TideType.lowTide:
                _theSea.position = Vector3.Lerp(_theSea.position , new Vector3(0 , lowTideY , 0) , tideTransitionSpeed * Time.deltaTime);
                if (Vector3.Distance(_theSea.position , new Vector3(0, lowTideY , 0)) <= 0.03f)
                {
                    finishedMoving = true;
                }
                break;
            case TideType.mediumTide:
                _theSea.position = Vector3.Lerp(_theSea.position , new Vector3(0 , midTideY , 0) , tideTransitionSpeed * Time.deltaTime);
                if (Vector3.Distance(_theSea.position , new Vector3(0, midTideY , 0)) <= 0.03f)
                {
                    finishedMoving = true;
                }
                break;
            case TideType.highTide:
                _theSea.position = Vector3.Lerp(_theSea.position , new Vector3(0 , HighTideY , 0) , tideTransitionSpeed * Time.deltaTime);
                if (Vector3.Distance(_theSea.position , new Vector3(0, HighTideY , 0)) <= 0.03f)
                {
                    finishedMoving = true;
                }
                break;
        }
    }
}
