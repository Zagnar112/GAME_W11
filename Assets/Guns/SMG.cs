using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMG : Gun
{
    public override bool AttemptFire()
    {
        if (!base.AttemptFire())
            return false;

        var b = Instantiate(bulletPrefab, gunBarrelEnd.transform.position, gunBarrelEnd.rotation);
        b.GetComponent<Projectile>().Initialize(2, 100, 2, 5, null); // version without special effect

        anim.SetTrigger("shoot");

        return true;
    }

    public override void TryFireHeld()
    {
        AttemptAutomaticFire();
    }
}
