using UnityEngine;
using UnityEngine.UI;

// Put this on each keypad button. digit 0-9 types that digit;
// -1 = backspace, -2 = OK (lock in the number), -3 = close the keypad.
[RequireComponent(typeof(Button))]
public class KeypadButton : MonoBehaviour
{
    public SafeKeypad keypad;
    public int digit;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Press);
    }

    void Press()
    {
        if (keypad == null) return;

        if (digit >= 0) keypad.PressDigit(digit);
        else if (digit == -1) keypad.Backspace();
        else if (digit == -2) keypad.Enter();
        else keypad.CloseKeypad();
    }
}
