using System;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class OnLaunchGame : MonoBehaviour
{
    public static OnLaunchGame Instance;
    public static event Action<string> OnServerStarted; // Passes the Code to join the lobby
    private void Start()
    {
        Instance = this;
        ConnectOnline();
    }
    // Setting Up Online services and signing in
    public async void ConnectOnline()
    {
        try
        {
            await UnityServices.InitializeAsync();
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                StartHost();
            }
            catch (RelayServiceException e)
            {
                ConnectionFailed(e);
            }
        }
        catch (RelayServiceException e)
        {
            ConnectionFailed(e);
        }
    }
    // Failed at getting Online services set up
    private void ConnectionFailed(RelayServiceException e)
    {
        Debug.LogError(e);
    }
    // Starting Host Server
    private async void StartHost()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(3);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        OnServerStarted?.Invoke(joinCode);
        Debug.Log(joinCode);
        NetworkManager.Singleton.StartHost();
    }

    // Closing Host server and joining another host
    public async void StartClient(string joinCode)
    {

        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);

            if (allocation == null) return;

            NetworkManager.Singleton.Shutdown();
            StartCoroutine(ConnectToClient(allocation));
        }
        catch (Exception e)
        {
            Debug.Log("No Server Found: " + e);
        }
    }
    private IEnumerator ConnectToClient(JoinAllocation allocation)
    {
        yield return new WaitUntil(() => NetworkManager.Singleton.ShutdownInProgress == false);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        NetworkManager.Singleton.StartClient();
    }
}
