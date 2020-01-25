using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindAnimation : MonoBehaviour
{
    private Meteo meteo;
    private Dictionary<Transform, Quaternion> initialRotation = new Dictionary<Transform, Quaternion>();
    public Vector3 wind;

    // Start is called before the first frame update
    void Start()
    {
        meteo = Meteo.Instance;
        Stack iterativePath = new Stack();
        iterativePath.Push(this.transform);
        while (iterativePath.Count != 0)
        {
            // pop and add childs to path
            Transform bone = (Transform)iterativePath.Pop();
            foreach (Transform child in bone)
                iterativePath.Push(child);

            // process current bone
            initialRotation[bone] = bone.localRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        wind = meteo.GetWind(transform.position);

        // move skeleton relatively to wind attributes
        Stack iterativePath = new Stack();
        iterativePath.Push(this.transform);
        while(iterativePath.Count != 0)
        {
            // pop and add childs to path
            Transform bone = (Transform) iterativePath.Pop();
            foreach (Transform child in bone)
                iterativePath.Push(child);

            // process current bone
            Vector3 axis = Vector3.Cross(transform.InverseTransformDirection(wind), bone.localPosition);
            Quaternion q = Quaternion.AngleAxis(wind.magnitude * transform.lossyScale.y * 0.01f, axis);
            bone.localRotation = initialRotation[bone] * q;
        }
    }
}
