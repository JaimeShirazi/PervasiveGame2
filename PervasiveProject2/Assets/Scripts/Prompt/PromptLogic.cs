using TMPro;
using UnityEngine;

public class PromptLogic : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private PromptSO promptSet;

    void Start()
    {
        //Calling it from here temporarily
        GlobalPromptState.NewPrompt(Random.Range(3, 6));
    }

    private void OnEnable()
    {
        GlobalPromptState.OnNewPrompt += Begin;
    }
    private void OnDisable()
    {
        GlobalPromptState.OnNewPrompt -= Begin;
    }
    public void Begin(int _)
    {
        text.text = "Create a \"" + promptSet.prompts[Random.Range(0, promptSet.prompts.Count)] + "\" progression";
    }
}
