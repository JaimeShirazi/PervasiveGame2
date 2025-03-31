using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Prompt Set", menuName = "ScriptableObjects/Prompt Set")]
public class PromptSO : ScriptableObject
{
    public List<string> prompts;
}
