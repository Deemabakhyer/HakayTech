using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VariableBlock : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    private int selectedShellID = -1;

    public void PopulateDropdown(Dictionary<string, int> savedVariables)
    {
        if (dropdown == null)
        {
            Debug.LogError("Dropdown مو مربوط في الـ Inspector على البلوك!");
            return;
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(savedVariables.Keys));

        if (dropdown.options.Count > 0)
        {
            string first = dropdown.options[0].text;
            if (savedVariables.ContainsKey(first))
                selectedShellID = savedVariables[first];
        }

        dropdown.onValueChanged.AddListener(index =>
        {
            string chosen = dropdown.options[index].text;
            if (savedVariables.ContainsKey(chosen))
                selectedShellID = savedVariables[chosen];
        });
    }

    public int GetSelectedShellID() => selectedShellID;
    public string GetSelectedName() => dropdown.options.Count > 0 ?
        dropdown.options[dropdown.value].text : "";
}