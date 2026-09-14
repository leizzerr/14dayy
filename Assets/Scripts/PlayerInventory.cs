using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public bool hasDepthStone;

    void Awake()
    {
        Instance = this;
    }
}
