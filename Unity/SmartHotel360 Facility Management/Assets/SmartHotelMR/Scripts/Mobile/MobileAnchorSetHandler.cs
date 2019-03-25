using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SmartHotelMR
{
    public class MobileAnchorSetHandler : MonoBehaviour
    {
        private AnchorSet AnchorSet { get; set; }

        [SerializeField]
        [Tooltip("Prefab for input dialog")]
        public GameObject InputDialogPrefab;

        [SerializeField]
        [Tooltip("Prefab for delete confirmation")]
        public GameObject ConfirmationDialogPrefab;

        [SerializeField]
        [Tooltip("Root scene object to use as parent for dialogs")]
        public Transform SceneRoot;

        public void HandleNewAnchorSet()
        {
            InputDialog.ShowDialog(InputDialogPrefab, SceneRoot, "New Anchor Set", "Please enter a name...", (result, name) =>
            {
                AnchorSetManager.Instance.IsEnabled = true;

                Debug.Log(string.Format("New dialog closed, result: {0}, name: {1}", result, name));

                if (result)
                {
                    StartCoroutine(AnchorSetManager.Instance.AddNewAnchorSet(name));
                }
            });
        }

        public void HandleDeleteAnchorSet(AnchorSet anchorSet)
        {
            AnchorSet = anchorSet;
            string message = string.Format("Are you sure you want to delete anchor set {0}?", anchorSet.name);

            ConfirmationDialog.ShowDialog(ConfirmationDialogPrefab, SceneRoot, message, (result) =>
            {
                AnchorSetManager.Instance.IsEnabled = true;

                Debug.Log(string.Format("Delete dialog closed, result: {0}", result));

                if (result)
                {
                    StartCoroutine(AnchorSetManager.Instance.DeleteAnchorSet(AnchorSet));
                }
            });
        }
    }
}