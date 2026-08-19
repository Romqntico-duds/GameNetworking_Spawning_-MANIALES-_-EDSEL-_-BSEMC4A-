using Unity.Netcode;
using UnityEngine;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;

    private float timer;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            timer = lifetime;
    }

    private void Update()
    {
        // Only the server moves and removes the bullet.
        if (!IsServer)
            return;

        transform.position += transform.forward * speed * Time.deltaTime;
        timer -= Time.deltaTime;

        if (timer <= 0f)
            NetworkObject.Despawn();
    }
}
