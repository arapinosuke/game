using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGenerator : MonoBehaviour
{
    public GameObject Ball;
    public GameObject CharacterParent;

    private void Start()
    {
        for (int i = 0; i < 30; i++)
        {
            Instantiate(Ball, CharacterParent.transform);
        }
    }
}
