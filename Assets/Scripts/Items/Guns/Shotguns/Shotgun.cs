namespace Cass.Items.Guns
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Default shotgun", menuName = "Guns/Shotguns/Default shotgun")]
    public class Shotgun : AbstractGun
    {
        protected override List<Vector3> Shoot()
        {
            throw new System.NotImplementedException();
        }
    }

}
