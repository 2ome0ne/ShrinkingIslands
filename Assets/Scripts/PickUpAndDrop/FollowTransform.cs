using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    public Transform player;

    public void SetTargetTransform(Transform targetTransform , Transform Player)
    {
        this.targetTransform = targetTransform;
        player = Player;
    }

    private void LateUpdate()
    {
        if(targetTransform == null){ return;}
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }
}
