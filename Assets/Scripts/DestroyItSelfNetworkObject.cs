using Unity.Netcode;using UnityEngine;

public class DestroyItSelfNetworkObject : NetworkBehaviour
{
    public float DespawnTime = 3f;
    public override void OnNetworkSpawn()
    {
        Invoke(nameof(DespawnItSelf), DespawnTime);
    }

    private void DespawnItSelf()
    {
        NetworkObject.Despawn(this);
    }
}
