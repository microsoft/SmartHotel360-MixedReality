using HoloToolkit.UI.Keyboard;
using HoloToolkit.UX.Dialog;
using SmartHotelMR;
using System;
using UnityEngine;

public class AnchorSetHandler : MonoBehaviour
{
    private const float KeyBoardPositionOffset = 0.045f;

    private AnchorSet AnchorSet { get; set; }

    [SerializeField]
    public Dialog DialogPrefab;

    [SerializeField]
    [Tooltip("GameObject used to position the keyboard relative to")]
    public GameObject KeyboardTarget;

    public void HandleNewAnchorSet()
    {
        Keyboard.Instance.Close();
        Keyboard.Instance.PresentKeyboard(string.Empty, Keyboard.LayoutType.Alpha);
        Keyboard.Instance.RepositionKeyboard(KeyboardTarget ? KeyboardTarget.gameObject.transform : this.gameObject.transform, null, KeyBoardPositionOffset);
        Keyboard.Instance.OnTextSubmitted += Keyboard_OnTextSubmitted;
        Keyboard.Instance.OnClosed += Keyboard_OnClosed;
    }

    private void Keyboard_OnClosed(object sender, EventArgs e)
    {
        AnchorSetManager.Instance.IsEnabled = true;
        Keyboard.Instance.OnClosed -= Keyboard_OnClosed;
        Keyboard.Instance.OnTextSubmitted -= Keyboard_OnTextSubmitted;
    }

    private void Keyboard_OnTextSubmitted(object sender, EventArgs args)
    {
        var keyboard = (Keyboard)sender;
        var name = keyboard.InputField.text;

        Keyboard.Instance.Close();

        StartCoroutine(AnchorSetManager.Instance.AddNewAnchorSet(name));
    }

    public void HandleDeleteAnchorSet(AnchorSet anchorSet)
    {
        AnchorSet = anchorSet;
        string message = string.Format("Are you sure you want to delete anchor set {0}?", anchorSet.name);

        Dialog dialog = Dialog.Open(DialogPrefab.gameObject, DialogButtonType.Yes | DialogButtonType.No, "Delete Anchor Set", message);
        dialog.OnClosed += OnDeleteDialogClosed;
    }

    private void OnDeleteDialogClosed(DialogResult result)
    {
        AnchorSetManager.Instance.IsEnabled = true;

        if (result.Result == DialogButtonType.Yes)
        {
            StartCoroutine(AnchorSetManager.Instance.DeleteAnchorSet(AnchorSet));
        }
    }

}
