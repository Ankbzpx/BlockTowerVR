using UnityEngine;
using System.Collections;

public class ModifyCam : MonoBehaviour {
    public float speed = 3f;
	float defaultHeight = 4f;

    void Start()
    {
        defaultHeight = gameObject.transform.position.y;
        transform.root.position = new Vector3(0f, GameControl.MaximunHeight()+1f, 0f);
    }
	
	// Update is called once per frame
    //modify the camera height based on the maximum height of the cube
	void FixedUpdate () {
        float _maxH = GameControl.MaximunHeight();

        if (_maxH >= defaultHeight)
        {
            Vector3 _delH = new Vector3(0f, _maxH+1f, 0f);
            transform.root.position = Vector3.Lerp(transform.position, _delH, 1);
        }

    }
}