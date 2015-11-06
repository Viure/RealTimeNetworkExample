using UnityEngine;
using UnityEngine.UI;


public class InputDebugView : MonoBehaviour 
{
	[SerializeField] private Image ButtonW;
	[SerializeField] private Image ButtonA;
	[SerializeField] private Image ButtonS;
	[SerializeField] private Image ButtonD;

    // Reset the debug view so no button is seen as pressed
	public void Reset()
	{
		ButtonW.color = Color.white;
		ButtonS.color = Color.white;
		ButtonA.color = Color.white;
		ButtonD.color = Color.white;
	}

    // make a button as pressed in the debug view
	public void MarkPressed(KeyCode k)
	{
		switch (k)
		{
            case (InputController.KEYCODE_THRUST_FORWARD):
			{
				ButtonW.color = Color.red;
				break;
			}
            case (InputController.KEYCODE_THRUST_LEFT):
			{
				ButtonA.color = Color.red;
				break;
			}
            case (InputController.KEYCODE_THRUST_BACKWARD):
			{
				ButtonS.color = Color.red;
				break;
			}
            case (InputController.KEYCODE_THRUST_RIGHT):
			{
				ButtonD.color = Color.red;
				break;
			}
		}
	}
}
