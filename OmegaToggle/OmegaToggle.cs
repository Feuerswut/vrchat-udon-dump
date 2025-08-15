
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

using VRC.SDKBase;
using VRC.Udon;

using TMPro;

public class OmegaToggle : UdonSharpBehaviour
{
    [Header("Targets to Toggle")]
    [SerializeField] private GameObject[] targetObjects = new GameObject[1];
    [SerializeField] private bool[] defaultStates = new bool[1];

    [Header("Cooldown")]
    [SerializeField, Range(0f, 10f)] private float cooldownSeconds = 0f;

    [Header("Button Visual (opt)")]
    [SerializeField] private Image buttonImage;

    [Header("Status Text (opt)")]
    [SerializeField] private TMP_Text statusText;

    [UdonSynced(UdonSyncMode.None)]
    private bool ToggleBool   = false;
    private bool cooldownBool = false;
    private bool lastSyncedValue;

    private void Start()
    {
        lastSyncedValue = ToggleBool;
        ApplyState();
    }

    public override void OnDeserialization()
    {
        // Only skip if the incoming synced value is the same as last time we DESERIALIZED
        if (ToggleBool == lastSyncedValue) return;

        lastSyncedValue = ToggleBool;
        ApplyState();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        // If the master left, button might turn usable or not
        UpdateButtonColor();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        UpdateButtonColor();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        UpdateButtonColor();
    }

    public void Toggle()
    {
        // cooldown prevent further execution
        if (cooldownBool) return;

        // If not master, run GetMaster() instead
        if (!IsLocalMaster())
        {
            GetMaster();
            return;
        }

        // apply toggle state
        ToggleBool = !ToggleBool;

        ApplyState();
        RequestSerialization();

        cooldownBool = true;
        SendCustomEventDelayedSeconds(nameof(ResetCooldown), cooldownSeconds);

        // update button and status text
        UpdateStatusText();
        UpdateButtonColor();
    }

    public void ResetCooldown()
    {
        cooldownBool = false;
        UpdateButtonColor();
    }

    private bool IsLocalMaster()
    {
        return Networking.IsOwner(gameObject);
    }

    private void GetMaster()
    {
        // Cooldown to avoid spam
        cooldownBool = true;
        SendCustomEventDelayedSeconds(nameof(ResetCooldown), cooldownSeconds * 2f);

        // Try to become master
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        // Just in case, update visuals immediately to show cooldown
        UpdateButtonColor();
    }

    private void ApplyState()
    {
        bool usedArray = false;

        if (targetObjects != null && defaultStates != null)
        {
            int len = targetObjects.Length;
            for (int i = 0; i < len; i++)
            {
                GameObject go = targetObjects[i];
                if (go != null)
                {
                    bool finalState = ToggleBool ? defaultStates[i] : !defaultStates[i];
                    go.SetActive(finalState);
                    usedArray = true;
                }
            }
        }

        // update button and status text
        UpdateStatusText();
        UpdateButtonColor();
    }

    // Update the Button status color if assigned.
    private void UpdateButtonColor()
    {
        if (buttonImage == null) return;

        if (!IsLocalMaster())
        {
            buttonImage.color = Color.yellow; // 🟡 Not master
        }
        else if (cooldownBool)
        {
            buttonImage.color = Color.red; // 🔴 Cooldown
        }
        else if (ToggleBool)
        {
            buttonImage.color = Color.white; // ⚪ ON
        }
        else
        {
            buttonImage.color = Color.gray; // ⚫ OFF
        }
    }

    // Update the TMP status text if assigned.
    private void UpdateStatusText()
    {
        if (statusText == null) return;
        
        statusText.text = $"Toggle Status: {(ToggleBool ? "opposite" : "default")}";
    }
}
