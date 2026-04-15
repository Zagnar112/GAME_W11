using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override bool AttemptFire()
    {
        if (!base.AttemptFire())
            return false;

        var b = Instantiate(bulletPrefab, gunBarrelEnd.transform.position, gunBarrelEnd.rotation);
        var c = Instantiate(bulletPrefab, gunBarrelEnd.transform.position, gunBarrelEnd.rotation);
        b.transform.Rotate(0, -5f, 0);
        c.transform.Rotate(0, 5f, 0);
        b.GetComponent<Projectile>().Initialize(12, 100, 1, 5, null);
        c.GetComponent<Projectile>().Initialize(12, 100, 1, 5, null);

        anim.SetTrigger("shoot");
        elapsed = 0;
        ammo -= 2;

        return true;
    }
}
