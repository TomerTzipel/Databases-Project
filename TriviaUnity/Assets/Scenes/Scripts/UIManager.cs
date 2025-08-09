using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DataManager dataManager;

    public static implicit operator UIManager(Ron_UIManager v)
    {
        throw new NotImplementedException();
    }
}
