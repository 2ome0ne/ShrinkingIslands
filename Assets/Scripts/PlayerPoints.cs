using UnityEngine;
using TMPro;
public class PlayerPoints : MonoBehaviour
{
    public TextMeshProUGUI pointsText;

    [SerializeField] private Transform content;

    [SerializeField]
    private GameObject pointsPrefab;
    [SerializeField] private GameObject HollowPointPrefab;


    public void AssignPoints(int points, string playerName)
    {
        int hollowPoints = 3 - points;
        for (int i = 0; i < hollowPoints; i++)
        {
            Instantiate(HollowPointPrefab, content);
        }

        points = 3 - hollowPoints;
        for (int i = 0; i < points; i++)
        {
            Instantiate(pointsPrefab, content);
        }
        pointsText.text = playerName.ToString()+ " :";
    }
}
