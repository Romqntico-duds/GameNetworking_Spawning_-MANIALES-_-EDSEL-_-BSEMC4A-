using Unity.Netcode;
using UnityEngine;

public class NetworkMenu : MonoBehaviour
{
    private void OnGUI()
    {
        NetworkManager manager = NetworkManager.Singleton;

        GUILayout.BeginArea(new Rect(10, 10, 230, 170), GUI.skin.box);
        GUILayout.Label("NETWORK BULLET DEMO");

        if (!manager.IsListening)
        {
            if (GUILayout.Button("Start Host", GUILayout.Height(35)))
                manager.StartHost();

            if (GUILayout.Button("Start Client", GUILayout.Height(35)))
                manager.StartClient();
        }
        else
        {
            GUILayout.Label(manager.IsHost ? "Running as HOST" : "Running as CLIENT");
            GUILayout.Label("Press SPACE to shoot");

            if (GUILayout.Button("Disconnect"))
                manager.Shutdown();
        }

        GUILayout.EndArea();
    }
}
