using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public GameObject moveto = null;
    public GameObject lookat = null;
    public float Movespeed = 4;
    public float Lookspeed = 320;

    private void LateUpdate()
    {
        if (moveto != null)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, moveto.transform.position, Movespeed * Time.deltaTime);
        }

        if (lookat != null)
        {
            Quaternion rotTarget = Quaternion.LookRotation(lookat.transform.position - this.transform.position);
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotTarget, Lookspeed * Time.deltaTime);
        }
    }
}
