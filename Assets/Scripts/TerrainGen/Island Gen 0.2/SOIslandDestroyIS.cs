using Unity.Netcode;
using UnityEngine;

public class SingleIslandGenerator : NetworkBehaviour
{
    [SerializeField] private float DestroyTime;

    public override void OnNetworkSpawn()
    {
        Invoke("DestroyNow", DestroyTime);
    }

    private void DestroyNow()
    {
        GameManager.Instance.islandHeart.IslandCrumble(GetComponent<SOIslandTile>());
    }
}
