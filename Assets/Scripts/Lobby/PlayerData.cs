
using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData> , INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes name;
    public int IndexColor;
    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId && name == other.name;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
    }
}
