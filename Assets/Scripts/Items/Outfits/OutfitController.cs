using Cass.Character;
using Cass.Items;
using System.Threading.Tasks;
using UnityEngine;

public class OutfitController : MonoBehaviour
{

    private Transform _bodyTransform = default;

    private Transform _glassesTransform = default;

    private Transform _hatTransform = default;

    private Transform _tailTransform = default;

    private Transform _outfitChangeTransform = default;

    public Task BuildCharacter(PlayerInfo info)
    {
        var bodyObj = ItemsContainer.Instance.Items.Find(_ => _.ItemId == info.BodyId).PrefabTransform;
        var glassesObj = ItemsContainer.Instance.Items.Find(_ => _.ItemId == info.GlassesId).PrefabTransform;
        var hatObj = ItemsContainer.Instance.Items.Find(_ => _.ItemId == info.HatId).PrefabTransform;
        var tailObj = ItemsContainer.Instance.Items.Find(_ => _.ItemId == info.TailId).PrefabTransform;

        if (bodyObj != null)
        {
            _bodyTransform = Instantiate(bodyObj, transform);
        }

        if (glassesObj != null) 
        {
            _glassesTransform = Instantiate(glassesObj, transform);
        }

        if(hatObj != null)
        {
            _hatTransform = Instantiate(hatObj, transform);
        }

        if(tailObj != null)
        {
            _tailTransform = Instantiate(tailObj, transform);
        }

        return Task.CompletedTask;
    }

    public Task RebuildCharacter(Outfit item, bool isActive, bool isPermanent)
    {
        var newObj = ItemsContainer.Instance.Items.Find(_ => _.ItemId == item.ItemId).PrefabTransform;

        if (isPermanent)
        {
            if(_outfitChangeTransform == null)
            {
                _outfitChangeTransform = Instantiate(newObj, transform);
            }

            switch (item.OutfitType)
            {
                case OutfitType.Hat:
                    if (_hatTransform != null)
                    {
                        Destroy(_hatTransform.gameObject);
                    }
                    _hatTransform = _outfitChangeTransform;
                    _outfitChangeTransform = null;
                    break;
                case OutfitType.Glasses:
                    if (_glassesTransform != null)
                    {
                        Destroy(_glassesTransform.gameObject);
                    }
                    _glassesTransform = _outfitChangeTransform;
                    _outfitChangeTransform = null;
                    break;
                case OutfitType.Body:
                    if (_bodyTransform != null)
                    {
                        Destroy(_bodyTransform.gameObject);
                    }
                    _bodyTransform = _outfitChangeTransform;
                    _outfitChangeTransform = null;
                    break;
                case OutfitType.Tail:
                    if (_tailTransform != null)
                    {
                        Destroy(_tailTransform.gameObject);
                    }
                    _tailTransform = _outfitChangeTransform;
                    _outfitChangeTransform = null;
                    break;
            }
            return Task.CompletedTask;
        }

        if (!isActive && _outfitChangeTransform != null)
        {
            EnableOutfit(item.OutfitType, true);
            Destroy(_outfitChangeTransform.gameObject);
            _outfitChangeTransform = null;
            return Task.CompletedTask;
        }

        _outfitChangeTransform = Instantiate(newObj, transform);

        EnableOutfit(item.OutfitType, false);

        return Task.CompletedTask;
    }

    private void EnableOutfit(OutfitType type, bool isEnable)
    {
        switch (type)
        {
            case OutfitType.Hat:
                if (_hatTransform != null)
                {
                    _hatTransform.gameObject.SetActive(isEnable);
                }
                break;
            case OutfitType.Glasses:
                if (_glassesTransform != null)
                {
                    _glassesTransform.gameObject.SetActive(isEnable);
                }
                break;
            case OutfitType.Body:
                if (_bodyTransform != null)
                {
                    _bodyTransform.gameObject.SetActive(isEnable);
                }
                break;
            case OutfitType.Tail:
                if (_tailTransform != null)
                {
                    _tailTransform.gameObject.SetActive(isEnable);
                }
                break;
        }
    }
    
}
