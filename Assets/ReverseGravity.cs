using UnityEngine;

public class ReverseGravity: MonoBehaviour
{
    public void SetGravity(bool isOn)
    {
        if (isOn)
        {
            Physics.gravity = new Vector3(0, 9.81f, 0);
        }
        else
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
        }
    }
}
