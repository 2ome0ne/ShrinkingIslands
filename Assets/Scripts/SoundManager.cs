using System;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class EnviormentSound
{
    public AudioSource audioSource;
    public float multiplier;
}

public class SoundManager : NetworkBehaviour
{
    public GameObject soundPrefab;

    public AudioClip[] soundClips;
    
    public float soundVolume;
    public float EnviromentSoundVolume;
    
    [SerializeField] private EnviormentSound[] EnviromentSource;

    private void FixedUpdate()
    {
        if (GameManager.Instance.escapeMenu.Pausing)
        {
            foreach (EnviormentSound source in EnviromentSource)
            {
                source.audioSource.volume = EnviromentSoundVolume * source.multiplier;
            }
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnSoundRpc(Vector3 position, float soundRadius, float volume, float pitch , int soundIndex)
    {
        GameObject sound = Instantiate(soundPrefab, position, Quaternion.identity);
        AudioSource soundSource = sound.GetComponent<AudioSource>();
        soundSource.pitch = pitch;
        soundSource.volume = volume * soundVolume;
        soundSource.minDistance = soundRadius;
        soundSource.generator = soundClips[soundIndex];
        soundSource.Play();
    }
}
