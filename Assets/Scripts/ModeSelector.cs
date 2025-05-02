using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages UI toggles, toggle layouts, dropdown, and delete/clear buttons for AR interaction modes.
/// Attach to a GameObject (e.g., UI Canvas) and assign components in the Inspector.
/// </summary>
public class ModeSelector : MonoBehaviour
{
    /// <summary>
    /// UI toggle for Translation mode.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable Translation mode.")]
    Toggle translationToggle;

    /// <summary>
    /// UI toggle for Scaling mode.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable Scaling mode.")]
    Toggle scalingToggle;

    /// <summary>
    /// UI toggle for Rotation mode.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable Rotation mode.")]
    Toggle rotationToggle;

    /// <summary>
    /// GameObject containing scaling toggles (X, Y, Z).
    /// </summary>
    [SerializeField]
    [Tooltip("GameObject containing scaling axis toggles (X, Y, Z).")]
    GameObject scalingToggleLayout;

    /// <summary>
    /// GameObject containing rotation toggles (X, Y, Z).
    /// </summary>
    [SerializeField]
    [Tooltip("GameObject containing rotation axis toggles (X, Y, Z).")]
    GameObject rotationToggleLayout;

    /// <summary>
    /// Toggle for scaling on X axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable scaling on X axis.")]
    Toggle scaleXToggle;

    /// <summary>
    /// Toggle for scaling on Y axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable scaling on Y axis.")]
    Toggle scaleYToggle;

    /// <summary>
    /// Toggle for scaling on Z axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable scaling on Z axis.")]
    Toggle scaleZToggle;

    /// <summary>
    /// Toggle for rotation on X axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable rotation on X axis.")]
    Toggle rotateXToggle;

    /// <summary>
    /// Toggle for rotation on Y axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable rotation on Y axis.")]
    Toggle rotateYToggle;

    /// <summary>
    /// Toggle for rotation on Z axis.
    /// </summary>
    [SerializeField]
    [Tooltip("Toggle to enable rotation on Z axis.")]
    Toggle rotateZToggle;

    /// <summary>
    /// Dropdown to select the prefab to spawn.
    /// </summary>
    [SerializeField]
    [Tooltip("Dropdown to select the prefab to spawn.")]
    TMP_Dropdown prefabDropdown;

    /// <summary>
    /// Button to delete the currently selected item.
    /// </summary>
    [SerializeField]
    [Tooltip("Button to delete the currently selected item.")]
    Button deleteButton;

    /// <summary>
    /// Button to clear all spawned items.
    /// </summary>
    [SerializeField]
    [Tooltip("Button to clear all spawned items.")]
    Button clearButton;

    /// <summary>
    /// Public properties to get toggle states.
    /// </summary>
    public bool IsTranslationEnabled => translationToggle != null && translationToggle.isOn;
    public bool IsScalingEnabled => scalingToggle != null && scalingToggle.isOn;
    public bool IsRotationEnabled => rotationToggle != null && rotationToggle.isOn;
    public bool IsScaleXEnabled => scaleXToggle != null && scaleXToggle.isOn;
    public bool IsScaleYEnabled => scaleYToggle != null && scaleYToggle.isOn;
    public bool IsScaleZEnabled => scaleZToggle != null && scaleZToggle.isOn;
    public bool IsRotateXEnabled => rotateXToggle != null && rotateXToggle.isOn;
    public bool IsRotateYEnabled => rotateYToggle != null && rotateYToggle.isOn;
    public bool IsRotateZEnabled => rotateZToggle != null && rotateZToggle.isOn;

    /// <summary>
    /// Event triggered when the delete button is clicked.
    /// </summary>
    public event Action OnDeleteButtonClicked;

    /// <summary>
    /// Event triggered when the clear button is clicked.
    /// </summary>
    public event Action OnClearButtonClicked;

    private void Awake()
    {
        // Verify UI components
        if (translationToggle == null || scalingToggle == null || rotationToggle == null)
        {
            Debug.LogError("One or more UI toggles (Translation, Scaling, Rotation) not assigned in the Inspector!");
        }
        if (scalingToggleLayout == null || rotationToggleLayout == null)
        {
            Debug.LogError("One or more toggle layouts (Scaling, Rotation) not assigned in the Inspector!");
        }
        if (scaleXToggle == null || scaleYToggle == null || scaleZToggle == null ||
            rotateXToggle == null || rotateYToggle == null || rotateZToggle == null)
        {
            Debug.LogError("One or more toggles (Scale X/Y/Z, Rotate X/Y/Z) not assigned in the Inspector!");
        }
        if (prefabDropdown == null)
        {
            Debug.LogError("Prefab Dropdown not assigned in the Inspector!");
        }
        if (deleteButton == null)
        {
            Debug.LogError("Delete Button not assigned in the Inspector!");
        }
        if (clearButton == null)
        {
            Debug.LogError("Clear Button not assigned in the Inspector!");
        }

        // Set up toggle listeners
        if (translationToggle != null)
        {
            translationToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(translationToggle, isOn));
        }
        if (scalingToggle != null)
        {
            scalingToggle.onValueChanged.AddListener(isOn => { UpdateToggleColor(scalingToggle, isOn); UpdateToggleLayouts(); });
        }
        if (rotationToggle != null)
        {
            rotationToggle.onValueChanged.AddListener(isOn => { UpdateToggleColor(rotationToggle, isOn); UpdateToggleLayouts(); });
        }

        // Set up delete and clear button listeners
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(() => OnDeleteButtonClicked?.Invoke());
            deleteButton.gameObject.SetActive(false); // Initially hidden
        }
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(() => OnClearButtonClicked?.Invoke());
        }

        // Initialize toggles to off
        if (scaleXToggle != null) scaleXToggle.isOn = false;
        if (scaleYToggle != null) scaleYToggle.isOn = false;
        if (scaleZToggle != null) scaleZToggle.isOn = false;
        if (rotateXToggle != null) rotateXToggle.isOn = false;
        if (rotateYToggle != null) rotateYToggle.isOn = false;
        if (rotateZToggle != null) rotateZToggle.isOn = false;

        // Set up axis toggle color listeners
        if (scaleXToggle != null) scaleXToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(scaleXToggle, isOn));
        if (scaleYToggle != null) scaleYToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(scaleYToggle, isOn));
        if (scaleZToggle != null) scaleZToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(scaleZToggle, isOn));
        if (rotateXToggle != null) rotateXToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(rotateXToggle, isOn));
        if (rotateYToggle != null) rotateYToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(rotateYToggle, isOn));
        if (rotateZToggle != null) rotateZToggle.onValueChanged.AddListener(isOn => UpdateToggleColor(rotateZToggle, isOn));

        // Initialize toggle layouts
        if (scalingToggleLayout != null) scalingToggleLayout.SetActive(false);
        if (rotationToggleLayout != null) rotationToggleLayout.SetActive(false);

        // Initialize toggle visuals
        UpdateToggleColor(translationToggle, false);
        UpdateToggleColor(scalingToggle, false);
        UpdateToggleColor(rotationToggle, false);
    }

    /// <summary>
    /// Updates the color of the toggle's Child(0).Image based on its state.
    /// </summary>
    private void UpdateToggleColor(Toggle toggle, bool isOn)
    {
        if (toggle != null && toggle.transform.childCount > 0)
        {
            Image childImage = toggle.transform.GetChild(0).GetComponent<Image>();
            if (childImage != null)
            {
                childImage.color = isOn ? Color.green : Color.white;
            }
        }
    }

    /// <summary>
    /// Updates visibility of toggle layouts based on mode toggles.
    /// </summary>
    private void UpdateToggleLayouts()
    {
        if (scalingToggleLayout != null)
            scalingToggleLayout.SetActive(IsScalingEnabled);
        if (rotationToggleLayout != null)
            rotationToggleLayout.SetActive(IsRotationEnabled);
        Debug.Log($"Toggle layouts updated: Scaling={IsScalingEnabled}, Rotation={IsRotationEnabled}");
    }

    /// <summary>
    /// Populates the prefab dropdown with the provided options.
    /// </summary>
    public void PopulateDropdown(List<string> options)
    {
        if (prefabDropdown != null)
        {
            prefabDropdown.ClearOptions();
            prefabDropdown.AddOptions(options);
            Debug.Log("Prefab dropdown populated with options: " + string.Join(", ", options));
        }
    }

    /// <summary>
    /// Gets the index of the selected prefab in the dropdown.
    /// </summary>
    public int GetSelectedPrefabIndex()
    {
        return prefabDropdown != null ? prefabDropdown.value : 0;
    }

    /// <summary>
    /// Sets the visibility of the delete button.
    /// </summary>
    public void SetDeleteButtonVisibility(bool isVisible)
    {
        if (deleteButton != null)
        {
            deleteButton.gameObject.SetActive(isVisible);
            Debug.Log($"Delete button visibility set to: {isVisible}");
        }
    }
}