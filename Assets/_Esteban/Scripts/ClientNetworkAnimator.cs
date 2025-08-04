using Unity.Netcode.Components;

/// <summary>
/// A componet you add to an object to synchronize animations, when the object
/// is owned by a device (ex. a player game object you control).
/// 
/// Note: Although this is an added component, and not part of the Netcode solution,
///   how to easily add this code is mentioned in our documentation.
///
/// To learn more about network animators, got the following link:
/// <see cref="https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.4/manual/components/networkanimator.html#owner-authoritative"/>
/// </summary>

public class ClientNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}