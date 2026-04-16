using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerWinsManager : NetworkBehaviour
{
    [System.Serializable]
    public class CurrentPlayers
    {
        public ulong playerId;
        public int currentwins;
    }
    
    public List<CurrentPlayers> currentplayers = new List<CurrentPlayers>();
    public string WinnerName;

    
    [Rpc(SendTo.Everyone , InvokePermission = RpcInvokePermission.Everyone)]
    public void AddCurrentPlayerByPlayerIdRpc(ulong playerId)
    {
        currentplayers.Find(_CurrentPlayers => _CurrentPlayers.playerId == playerId).currentwins += 1;
    }

    public int GetCurrentWinsByPlayerId(ulong playerId)
    {
        return currentplayers.Find(_CurrentPlayers => _CurrentPlayers.playerId == playerId).currentwins;
    }

    public bool CheckIfAnyoneWon()
    {
        foreach (var player in currentplayers)
        {
            if (player.currentwins >= 3)
            {
                return true;
            }
        }
        return false;
    }
}
