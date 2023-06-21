namespace Cass.Items.Guns
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Default shotgun", menuName = "Guns/Shotguns/Default shotgun")]
    public class Shotgun : AbstractGun
    {
        public override List<Vector3> Shoot(Vector3 forward)
        {
            List<Vector3> velocities = new List<Vector3>(_bulletsPerShoot);

            for (int i = 0; i < _bulletsPerShoot; i++)
            {
                velocities.Add(new Vector3(forward.x + Random.Range(-_scatter, _scatter), 0, forward.z + Random.Range(-_scatter, _scatter)) * _bulletSpeed );
            }

            return velocities;
        }
    }

}
