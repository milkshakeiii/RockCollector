using UnityEngine;

public class NumbersOnlyInput : MonoBehaviour
{
    public int maxDigits = 4;
    public TMPro.TMP_InputField inputField;

    public void OnValueChanged()
    {
        // limit to only 4 digits between 0 and 9
        if (inputField.text.Length > maxDigits)
        {
            inputField.text = inputField.text.Substring(0, 4);
        }
        else
        {
            foreach (char c in inputField.text)
            {
                if (c < '0' || c > '9')
                {
                    inputField.text = inputField.text.Replace(c.ToString(), "");
                }
            }
        }
    }
}
