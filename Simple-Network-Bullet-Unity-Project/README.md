# Simple Network Bullet Demo

This project demonstrates NetworkObject spawning and despawning using a bullet fired from a gun.

## How it works

1. Press **Space**.
2. `GunShooter` sends an RPC to the server.
3. The server instantiates the bullet prefab.
4. The server calls `bullet.Spawn()`.
5. `NetworkTransform` shows the moving bullet on the Host and Client.
6. After three seconds, the server calls `NetworkObject.Despawn()`.

## Test it

1. Open `Assets/Scenes/NetworkBulletDemo.unity`.
2. Make a Windows build.
3. Run the build and press **Start Host**.
4. Enter Play Mode in Unity and press **Start Client**.
5. Press **Space** in either window.
6. The same bullet should appear, move, and disappear in both windows.

## Three scripts

- `NetworkMenu.cs` — starts the Host or Client.
- `GunShooter.cs` — asks the server to instantiate and spawn a bullet.
- `NetworkBullet.cs` — moves the bullet and despawns it after three seconds.

## Inspector setup

- `NetworkManager` has `UnityTransport`.
- `NetworkBullet.prefab` has `NetworkObject`, `NetworkTransform`, and `NetworkBullet`.
- `NetworkBullet.prefab` is registered in `NetworkPrefabsList.asset`.
- The scene gun has `NetworkObject` and `GunShooter`.
