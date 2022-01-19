
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ProtectedAvatarPedestal : UdonSharpBehaviour
{
    [SerializeField] GameObject Mirror;
    VRC_AvatarPedestal avatarPedestal;
    private void Start()
    {
        avatarPedestal = (VRC_AvatarPedestal)GetComponent(typeof(VRC_AvatarPedestal));
    }
    private void Interact()
    {
        avatarPedestal.SetAvatarUse(Networking.LocalPlayer);
        Networking.LocalPlayer.UseAttachedStation();
    }
    public override void OnStationEntered()
    {
        Mirror.SetActive(true);
    }
    public override void OnStationExited()
    {
        Mirror.SetActive(false);
    }
}
