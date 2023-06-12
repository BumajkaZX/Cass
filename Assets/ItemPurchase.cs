using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cass.StoreManager;
using Cass.Items;

public class ItemPurchase : MonoBehaviour
{

    [SerializeField]
    private PlayerItem _item = default;

    public void Purchase()
    {
        StoreManager.Instance.Purchase(_item);
    }
}
