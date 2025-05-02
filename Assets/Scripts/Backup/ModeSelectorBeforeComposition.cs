// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// /// <summary>
// /// Manages UI buttons, toggle layouts, dropdown, and delete/clear buttons for AR interaction modes.
// /// Attach to a GameObject (e.g., UI Canvas) and assign components in the Inspector.
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
//     /// GameObject containing scaling toggles (X, Y, Z).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("GameObject containing scaling axis toggles (X, Y, Z).")]
//     GameObject scalingToggleLayout;

//     /// <summary>
//     /// GameObject containing rotation toggles (X, Y, Z).
//     /// </summary>
//     [SerializeField]
//     [Tooltip("GameObject containing rotation axis toggles (X, Y, Z).")]
//     GameObject rotationToggleLayout;

//     /// <summary>
//     /// Toggle for scaling on X axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Toggle to enable scaling on X axis.")]
//     Toggle scaleXToggle;

//     /// <summary>
//     /// Toggle for scaling on Y axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Toggle to enable scaling on Y axis.")]
//     Toggle scaleYToggle;

//     /// <summary>
//     /// Toggle for scaling on Z axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Toggle to enable scaling on Z axis.")]
//     Toggle scaleZToggle;

//     /// <summary>
//     /// Toggle for rotation on X axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Toggle to enable rotation on X axis.")]
//     Toggle rotateXToggle;

//     /// <summary>
//     /// Toggle for rotation on Y axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Toggle to enable rotation on Y axis.")]
//     Toggle rotateYToggle;

//     /// <summary>
//     /// Toggle for rotation on Z axis.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Toggle to enable rotation on Z axis.")]
//     Toggle rotateZToggle;

//     /// <summary>
//     /// Dropdown to select the prefab to spawn.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Dropdown to select the prefab to spawn.")]
//     TMP_Dropdown prefabDropdown;

//     /// <summary>
//     /// Button to delete the currently selected item.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Button to delete the currently selected item.")]
//     Button deleteButton;

//     /// <summary>
//     /// Button to clear all spawned items.
//     /// </summary>
//     [SerializeField]
//     [Tooltip("Button to clear all spawned items.")]
//     Button clearButton;

//     /// <summary>
//     /// Current interaction mode.
//     /// </summary>
//     private InteractionMode currentMode = InteractionMode.Translation;

//     /// <summary>
//     /// Public property to get the current mode.
//     /// </summary>
//     public InteractionMode CurrentMode => currentMode;

//     /// <summary>
//     /// Public properties to get toggle states.
//     /// </summary>
//     public bool IsScaleXEnabled => scaleXToggle != null && scaleXToggle.isOn;
//     public bool IsScaleYEnabled => scaleYToggle != null && scaleYToggle.isOn;
//     public bool IsScaleZEnabled => scaleZToggle != null && scaleZToggle.isOn;
//     public bool IsRotateXEnabled => rotateXToggle != null && rotateXToggle.isOn;
//     public bool IsRotateYEnabled => rotateYToggle != null && rotateYToggle.isOn;
//     public bool IsRotateZEnabled => rotateZToggle != null && rotateZToggle.isOn;

//     /// <summary>
//     /// Event triggered when the delete button is clicked.
//     /// </summary>
//     public event Action OnDeleteButtonClicked;

//     /// <summary>
//     /// Event triggered when the clear button is clicked.
//     /// </summary>
//     public event Action OnClearButtonClicked;

//     private void Awake()
//     {
//         // Verify UI components
//         if (translationButton == null || scalingButton == null || rotationButton == null)
//         {
//             Debug.LogError("One or more UI buttons (Translation, Scaling, Rotation) not assigned in the Inspector!");
//         }
//         if (scalingToggleLayout == null || rotationToggleLayout == null)
//         {
//             Debug.LogError("One or more toggle layouts (Scaling, Rotation) not assigned in the Inspector!");
//         }
//         if (scaleXToggle == null || scaleYToggle == null || scaleZToggle == null ||
//             rotateXToggle == null || rotateYToggle == null || rotateZToggle == null)
//         {
//             Debug.LogError("One or more toggles (Scale X/Y/Z, Rotate X/Y/Z) not assigned in the Inspector!");
//         }
//         if (prefabDropdown == null)
//         {
//             Debug.LogError("Prefab Dropdown not assigned in the Inspector!");
//         }
//         if (deleteButton == null)
//         {
//             Debug.LogError("Delete Button not assigned in the Inspector!");
//         }
//         if (clearButton == null)
//         {
//             Debug.LogError("Clear Button not assigned in the Inspector!");
//         }

//         // Set up button listeners
//         if (translationButton != null && scalingButton != null && rotationButton != null)
//         {
//             translationButton.onClick.AddListener(() => SetMode(InteractionMode.Translation));
//             scalingButton.onClick.AddListener(() => SetMode(InteractionMode.Scaling));
//             rotationButton.onClick.AddListener(() => SetMode(InteractionMode.Rotation));
//             Debug.Log("UI buttons assigned and listeners set.");
//         }

//         // Set up delete and clear button listeners
//         if (deleteButton != null)
//         {
//             deleteButton.onClick.AddListener(() => OnDeleteButtonClicked?.Invoke());
//             deleteButton.gameObject.SetActive(false); // Initially hidden
//         }
//         if (clearButton != null)
//         {
//             clearButton.onClick.AddListener(() => OnClearButtonClicked?.Invoke());
//         }

//         // Initialize toggles to off
//         if (scaleXToggle != null) scaleXToggle.isOn = false;
//         if (scaleYToggle != null) scaleYToggle.isOn = false;
//         if (scaleZToggle != null) scaleZToggle.isOn = false;
//         if (rotateXToggle != null) rotateXToggle.isOn = false;
//         if (rotateYToggle != null) rotateYToggle.isOn = false;
//         if (rotateZToggle != null) rotateZToggle.isOn = false;

//         // Set up toggle color listeners
//         if (scaleXToggle != null) scaleXToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(scaleXToggle, isOn));
//         if (scaleYToggle != null) scaleYToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(scaleYToggle, isOn));
//         if (scaleZToggle != null) scaleZToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(scaleZToggle, isOn));
//         if (rotateXToggle != null) rotateXToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(rotateXToggle, isOn));
//         if (rotateYToggle != null) rotateYToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(rotateYToggle, isOn));
//         if (rotateZToggle != null) rotateZToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(rotateZToggle, isOn));

//         // Initialize toggle layouts
//         if (scalingToggleLayout != null) scalingToggleLayout.SetActive(false);
//         if (rotationToggleLayout != null) rotationToggleLayout.SetActive(false);

//         // Initialize button visuals
//         UpdateButtonVisuals();
//     }

//     /// <summary>
//     /// Updates the color of the toggle's Child(0).Image based on its state.
//     /// </summary>
//     /// <param name="toggle">The toggle to update.</param>
//     /// <param name="isOn">Whether the toggle is on or off.</param>
//     private void UpdateToggleColor(Toggle toggle, bool isOn)
//     {
//         if (toggle != null && toggle.transform.childCount > 0)
//         {
//             Image childImage = toggle.transform.GetChild(0).GetComponent<Image>();
//             if (childImage != null)
//             {
//                 childImage.color = isOn ? Color.green : Color.white;
//             }
//         }
//     }

//     /// <summary>
//     /// Sets the current interaction mode, updates button visuals, and manages toggle layouts.
//     /// </summary>
//     /// <param name="mode">The mode to set.</param>
//     private void SetMode(InteractionMode mode)
//     {
//         currentMode = mode;
//         Debug.Log($"Switched to {mode} mode.");

//         // Manage toggle layouts
//         if (scalingToggleLayout != null)
//             scalingToggleLayout.SetActive(mode == InteractionMode.Scaling);
//         if (rotationToggleLayout != null)
//             rotationToggleLayout.SetActive(mode == InteractionMode.Rotation);

//         // Reset toggles to off when switching modes
//         if (scaleXToggle != null) scaleXToggle.isOn = false;
//         if (scaleYToggle != null) scaleYToggle.isOn = false;
//         if (scaleZToggle != null) scaleZToggle.isOn = false;
//         if (rotateXToggle != null) rotateXToggle.isOn = false;
//         if (rotateYToggle != null) rotateYToggle.isOn = false;
//         if (rotateZToggle != null) rotateZToggle.isOn = false;

//         UpdateButtonVisuals();
//     }

//     /// <summary>
//     /// Updates button interactability to indicate the active mode.
//     /// </summary>
//     private void UpdateButtonVisuals()
//     {
//         if (translationButton != null)
//             translationButton.interactable = (currentMode != InteractionMode.Translation);
//         if (scalingButton != null)
//             scalingButton.interactable = (currentMode != InteractionMode.Scaling);
//         if (rotationButton != null)
//             rotationButton.interactable = (currentMode != InteractionMode.Rotation);
//     }

//     /// <summary>
//     /// Populates the prefab dropdown with the provided options.
//     /// </summary>
//     /// <param name="options">List of prefab names.</param>
//     public void PopulateDropdown(List<string> options)
//     {
//         if (prefabDropdown != null)
//         {
//             prefabDropdown.ClearOptions();
//             prefabDropdown.AddOptions(options);
//             Debug.Log("Prefab dropdown populated with options: " + string.Join(", ", options));
//         }
//     }

//     /// <summary>
//     /// Gets the index of the selected prefab in the dropdown.
//     /// </summary>
//     /// <returns>The selected index, or 0 if dropdown is null.</returns>
//     public int GetSelectedPrefabIndex()
//     {
//         return prefabDropdown != null ? prefabDropdown.value : 0;
//     }

//     /// <summary>
//     /// Sets the visibility of the delete button.
//     /// </summary>
//     /// <param name="isVisible">Whether the delete button should be visible.</param>
//     public void SetDeleteButtonVisibility(bool isVisible)
//     {
//         if (deleteButton != null)
//         {
//             deleteButton.gameObject.SetActive(isVisible);
//             Debug.Log($"Delete button visibility set to: {isVisible}");
//         }
//     }
// }