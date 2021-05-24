using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFireTime; //sonraki ateşin zamanını ayarlamak için kullan

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) { return; }

        if (!CanFireAtTarget()) { return; }

        //hedefin dönüşü
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        // dönen hedefe göre nesnenin dönüşü
        transform.rotation = Quaternion.RotateTowards( transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            //merminin dönüşü
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);
            //mermi oluştur
            GameObject projectileInstance = Instantiate( projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            NetworkServer.Spawn(projectileInstance, connectionToClient);

            lastFireTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude
            <= fireRange * fireRange;
    }
}
