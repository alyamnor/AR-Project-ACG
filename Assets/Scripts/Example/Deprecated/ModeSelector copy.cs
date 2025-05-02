// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// Manages UI buttons for selecting AR interaction modes (Translation, Scaling, Rotation).
// /// Attach to a GameObject (e.g., UI Canvas) and assign the buttons in the Inspector.
// /// </summary>
// public class ModeSelector : MonoBehaviour
// {
//     /// <summary>
//     /// Enum to define interaction modes.
//     /// </summary>
//     public enum InteractionMode
//     {
//         Translation,
//         Scaling,
//         Rotation
//     }

//     /// <summary>
//     /// UI button for Translation mode.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Button to select Translation mode.")]
//     Button translationButton;

//     /// <summary>
//     /// UI button for Scaling mode.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Button to select Scaling mode.")]
//     Button scalingButton;

//     /// <summary>
//     /// UI button for Rotation mode.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Button to select Rotation mode.")]
//     Button rotationButton;

//     /// <summary>
//     /// Current interaction mode.
//     /// </summary>
//     private InteractionMode currentMode = InteractionMode.Translation;

//     /// <summary>
//     /// Public property to get the current mode.
//     /// </summary>
//     public InteractionMode CurrentMode => currentMode;

//     private void Awake()
//     {
//         // Verify UI buttons
//         if (translationButton == null || scalingButton == null || rotationButton == null)
//         {
//             Debug.LogError("One or more UI buttons (Translation, Scaling, Rotation) not assigned in the Inspector!");
//         }
//         else
//         {
//             // Set up button listeners
//             translationButton.onClick.AddListener(() => SetMode(InteractionMode.Translation));
//             scalingButton.onClick.AddListener(() => SetMode(InteractionMode.Scaling));
//             rotationButton.onClick.AddListener(() => SetMode(InteractionMode.Rotation));
//             Debug.Log("UI buttons assigned and listeners set.");
//         }

//         // Initialize button visuals
//         UpdateButtonVisuals();
//     }

//     /// <summary>
//     /// Sets the current interaction mode and updates button visuals.
//     /// </summary>
//     /// <param name="mode">The mode to set.</param>
//     private void SetMode(InteractionMode mode)
//     {
//         currentMode = mode;
//         Debug.Log($"Switched to {mode} mode.");
//         UpdateButtonVisuals();
//     }

//     /// <summary>
//     /// Updates button interactability to indicate the active mode.
//     /// </summary>
//     private void UpdateButtonVisuals()
//     {
//         translationButton.interactable = (currentMode != InteractionMode.Translation);
//         scalingButton.interactable = (currentMode != InteractionMode.Scaling);
//         rotationButton.interactable = (currentMode != InteractionMode.Rotation);
//     }
// }