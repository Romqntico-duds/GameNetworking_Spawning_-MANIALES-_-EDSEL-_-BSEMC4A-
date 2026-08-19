using Unity.Netcode;
using UnityEngine;

public class GunShooter : NetworkBehaviour
{
    [SerializeField] private NetworkObject bulletPrefab;
    [SerializeField] private Transform muzzle;

    private void Update()
    {
        if (IsSpawned && Input.GetKeyDown(KeyCode.Space))
            ShootRpc();
    }

    // A client pressing Space asks the server to shoot.
    [Rpc(SendTo.Server)]
    private void ShootRpc()
    {
        // 1. Instantiate the registered Network Prefab.
        NetworkObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

        // 2. Configure it before spawning.
        bullet.name = "Network Bullet";

        // 3. Spawn it across the network.
        bullet.Spawn();
    }
}
