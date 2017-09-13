using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{

    enum PlayMode { Mode_1, Mode_2 };
    enum Direction { Left, Right, Up, Down };

    int _tempCubeID;
    PhotonView photonView;

    public bool isTurn = false;

    //the target field of view
    float targetFoV = 10f;
    //How quickly the zoom reaches the target value
    float Dampening = 10.0f;
    // The minimum field of view value we want to zoom to
    public float MinFov = 30.0f;
    // The maximum field of view value we want to zoom to
    public float MaxFov = 80.0f;

    public float wheelSpeed = 50f;
    public float rotateSpeed = 50f;
    public float controllerFactor = 3f;
    public float cubeThreshold = 0.15f;
	public float heightThreshold = 0.4f;
	public float moveFactor = 25f;
    public float rotationFactor = 10f;

    Vector3 tempPosi = Vector3.zero;
    bool isUsing = false;

    AudioManager audioManager;

    [SerializeField]
    Text controlModeText;

    [SerializeField]
    Text playModeText;

    PlayMode playMode;

    float move_3;
    float move_4;
    float move_5;
    float move_6;
    float move_7;

    bool allowSwitch = false;

    Cube hightedCube;

    List<Cube> potentialCubes;

    float H_input;
    float V_input;
	
		float longPressTime;
	float switchCubeTime;

    // Use this for initialization
    void Start()
    {

        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        photonView = GetComponent<PhotonView>();

        targetFoV = Camera.main.fieldOfView;

        //Set Control Mode text
        if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Keyboard)
        {
            controlModeText.text = "PC Keyboard";
        }
        else if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Controller)
        {
            Cube.highlightedCubeID = 1;
            controlModeText.text = "Controller";
        }
        else if (TurnGameManager.controlMode == GameControl.ControlMode.Mobile)
        {
            controlModeText.text = "Mobile";
        }
        else if (TurnGameManager.controlMode == GameControl.ControlMode.PC_VR)
        {
            controlModeText.text = "PC VR";
        }

        //set default playmode
        playMode = PlayMode.Mode_1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.isMine && PhotonNetwork.connected)
        {
            return;
        }

        if (photonView.isMine)
        {
            if (isTurn)
            {


                if (!PhotonNetwork.player.IsMasterClient)
                {
                    if (PhotonNetwork.SetMasterClient(PhotonNetwork.player))
                    {
                        //Debug.Log("Master player set to local player");
                    }
                }

                //SelectCube();
                MoveCube();

                if (TurnGameManager.controlMode != GameControl.ControlMode.Mobile)
                    RotateCamera();

                if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Controller)
                    Cube.showHighlight = true;

            }
            else
            {

                if (TurnGameManager.controlMode != GameControl.ControlMode.Mobile)
                    RotateCamera();

                if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Controller)
                    Cube.showHighlight = false;
            }
        }

    }

    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected)
        {
            return;
        }

        if (photonView.isMine)
        {
            if (isTurn)
            {

                if (!PhotonNetwork.player.IsMasterClient)
                {
                    if (PhotonNetwork.SetMasterClient(PhotonNetwork.player))
                    {
                        //Debug.Log("Master player set to local player");
                    }
                }

                SelectCube();


            }
        }
    }


    //the function for the player to select the cube
    void SelectCube()
    {
        if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Keyboard)
        {
            if (Input.GetMouseButtonDown(0) && Cube.currentCubeID == 0)
            {

                RaycastHit _hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, Mathf.Infinity))
                {
                    //if it is the object is raycasted
                    if (_hit.transform.gameObject.GetComponent<Cube>() != null)
                    {
                        //store the local id of the select cube(for cmd function)
                        _tempCubeID = _hit.transform.gameObject.GetComponent<Cube>().GetCubeLocalID();

                        //need function to set tempCube to null
                        if (_tempCubeID != 0)
                        {
                            if (audioManager != null)
                                audioManager.Play("Click");

                            if (!PhotonNetwork.offlineMode)
                                photonView.RPC("RPCSelectCube", PhotonTargets.All, _tempCubeID);
                            else
                                GameControl.GetCube(_tempCubeID).Select();

                            _tempCubeID = 0;
                        }
                    }
                }
            }
            else if (Input.GetMouseButtonDown(0) && Cube.currentCubeID != 0)
            {
                if (audioManager != null)
                    audioManager.Play("Click");

                if (!PhotonNetwork.offlineMode)
                    photonView.RPC("RPCSelectCube", PhotonTargets.All, Cube.currentCubeID);
                else
                    GameControl.GetCube(Cube.currentCubeID).Select();
            }
        }
        //Have been updated
        else if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Controller)
        {


            if (Input.GetButtonDown("A") && Cube.currentCubeID == 0)
            {

                if (Cube.highlightedCubeID != 0)
                {

                    if (Cube.highlightedCubeID != 0)
                    {
                        if (GameControl.IsAtTop(Cube.highlightedCubeID))
                        {
                            if (audioManager != null)
                                audioManager.Play("Step");

                            //Debug.Log("Cube at top");
                        }
                        else
                        {
                            if (audioManager != null)
                                audioManager.Play("Click");

                            if (!PhotonNetwork.offlineMode)
                                photonView.RPC("RPCSelectCube", PhotonTargets.All, Cube.highlightedCubeID);
                            else
                                GameControl.GetCube(Cube.highlightedCubeID).Select();
                        }

                    }
                }
            }
            else if (Input.GetButtonDown("A") && Cube.currentCubeID != 0)
            {
                if (GameControl.CheckContact(Cube.currentCubeID, 0.7f))
                {
                    if (audioManager != null)
                        audioManager.Play("Step");

                    //Debug.Log("Cube at top");
                }
                else
                {
                    if (audioManager != null)
                        audioManager.Play("Click");

                    if (!PhotonNetwork.offlineMode)
                        photonView.RPC("RPCSelectCube", PhotonTargets.All, Cube.currentCubeID);
                    else
                        GameControl.GetCube(Cube.currentCubeID).Select();
                }


            }
            else if (Cube.currentCubeID == 0)
            {
                H_input = Input.GetAxisRaw("Horizontal");
                V_input = Input.GetAxisRaw("Vertical");

                int tempHighlightID = Cube.highlightedCubeID;

				if (H_input != 0f || V_input != 0f)
				{
					longPressTime += Time.deltaTime;

					if (longPressTime > 0.3f)
						allowSwitch = true;
				}
				else
				{
					longPressTime = 0f;
				}

				if (allowSwitch)
                {
					switchCubeTime += Time.deltaTime;

					if (switchCubeTime < 0.1f)
						return;

					if (H_input != 0f || V_input != 0f)
                    {
						if (Mathf.Abs(H_input) >= Mathf.Abs(V_input))
                        {
                            //left
                            if (H_input <= 0)
                            {

                                //function to switch highlighted cube
                                SwitchCubes(Direction.Left);
                            }
                            //right
                            else if (H_input > 0)
                            {

                                //function to switch highlighted cube
                                SwitchCubes(Direction.Right);
                            }
                        }
                        else if (Mathf.Abs(H_input) < Mathf.Abs(V_input))
                        {
                            //up
                            if (V_input >= 0)
                            {

                                //function to switch highlighted cube
                                SwitchCubes(Direction.Up);
                            }
                            //down
                            else if (V_input < 0)
                            {

                                //function to switch highlighted cube
                                SwitchCubes(Direction.Down);
                            }
                        }

                    }

                    if (Cube.highlightedCubeID != tempHighlightID)
                    {
						allowSwitch = false;
						switchCubeTime = 0;
					}
                }
                else
                {
                    if (H_input == 0f && V_input == 0f)
                    {
                        allowSwitch = true;
                    }
                }
            }




        }
        //      else if (TurnGameManager.controlMode == GameControl.ControlMode.PC_VR)
        //      {
        //	if (Input.GetMouseButtonDown(0))
        //	{
        //		Debug.Log("VR click");
        //		if (_tempCubeID != 0)
        //		{
        //			photonView.RPC("RPCSelectCube", PhotonTargets.All, _tempCubeID);
        //			_tempCubeID = 0;
        //		}
        //		else
        //		{
        //			RaycastHit _hit;
        //			if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out _hit, Mathf.Infinity))
        //			{
        //				//if it is the object is raycasted
        //				if (_hit.transform.gameObject.GetComponent<Cube>() != null)
        //				{
        //					//store the local id of the select cube(for cmd function)
        //					_tempCubeID = _hit.transform.gameObject.GetComponent<Cube>().GetCubeLocalID();

        //					//need function to set tempCube to null
        //					if (_tempCubeID != 0)
        //					{
        //						photonView.RPC("RPCSelectCube", PhotonTargets.All, _tempCubeID);
        //					}
        //				}
        //			}
        //		}
        //	}
        //}
        //else if(TurnGameManager.controlMode == GameControl.ControlMode.Mobile)
        //{

        //}
    }

    //the function to let the player to move the selected cube
    void MoveCube()
    {
        if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Keyboard)
        {
            if (Cube.currentCubeID == 0)
            {
                return;
            }
            else if (Cube.currentCubeID != 0)
            {

                if (Input.GetKey(KeyCode.Q))
                {
                    playMode = PlayMode.Mode_2;
                }
                else
                {
                    playMode = PlayMode.Mode_1;
                }

                //calculate the local direction
                Vector3 localForward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                Vector3 localRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

                if (Cube.currentCubeID != 0)
                {
                    if (playMode == PlayMode.Mode_1)
                    {
                        playModeText.text = "Play Mode 1";

                        Vector3 movement = localForward * V_input + localRight * H_input;
                        if (movement != Vector3.zero)
                        {
                            //if (!PhotonNetwork.offlineMode)
                            //{
                            //    photonView.RPC("RPCMoveCube_1", PhotonTargets.MasterClient, Cube.currentCubeID,
                            //        movement);
                            //}
                            //else
                            //{
                                Vector3 newPosition = GameControl.GetCube(Cube.currentCubeID).transform.position + movement;
                                GameControl.GetCube(Cube.currentCubeID).GetComponent<Rigidbody>().MovePosition(newPosition);
                            //}

                        }
                    }
                    else if (playMode == PlayMode.Mode_2)
                    {
                        playModeText.text = "Play Mode 2";

                        Vector3 movement = V_input * Vector3.up + H_input * new Vector3(rotateSpeed, 0, 0);

                        if (movement != Vector3.zero)
                        {
                            //if (!PhotonNetwork.offlineMode)
                            //{
                            //    photonView.RPC("RPCMoveCube_2", PhotonTargets.MasterClient, Cube.currentCubeID,
                            //    movement);
                            //}
                            //else
                            //{
                                Vector3 newPosition = GameControl.GetCube(Cube.currentCubeID).transform.position + new Vector3(0, movement.y, 0);
                                float rotateRadius = movement.x;

                                GameControl.GetCube(Cube.currentCubeID).GetComponent<Rigidbody>().MovePosition(newPosition);
                                GameControl.GetCube(Cube.currentCubeID).transform.Rotate(Vector3.up, rotateRadius, Space.Self);
                            //}
                        }
                    }
                }
            }
        }
        else if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Controller)
        {
            if (Cube.currentCubeID == 0)
            {
                return;
            }
            else if (Cube.currentCubeID != 0)
            {
                H_input = Input.GetAxisRaw("Horizontal");
                V_input = Input.GetAxisRaw("Vertical");

                if (Input.GetButton("Y"))
                {
                    playMode = PlayMode.Mode_2;
                }
                else
                {
                    playMode = PlayMode.Mode_1;
                }

                Rigidbody _rb = GameControl.GetCube(Cube.currentCubeID).GetComponent<Rigidbody>();

                //calculate the local direction
                Vector3 localForward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
                Vector3 localRight = new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z);

                if (Cube.currentCubeID != 0)
                {
                    if (playMode == PlayMode.Mode_1)
                    {
                        playModeText.text = "Play Mode 1";

                        Vector3 movement = localForward * V_input / moveFactor + localRight * H_input / moveFactor;
                        if (movement != Vector3.zero)
                        {
                            //if (!PhotonNetwork.offlineMode)
                            //{
                            //    photonView.RPC("RPCMoveCube_1", PhotonTargets.MasterClient, Cube.currentCubeID,
                            //        movement);
                            //}
                            //else
                            //{
                                _rb.MovePosition(_rb.position + movement);
                            //}

                        }


                        move_6 = Input.GetAxis("6th");
                        move_7 = Input.GetAxis("7th");

                        Vector3 movement_2 = Vector3.zero;

                        if (Mathf.Abs(move_7) >= Mathf.Abs(move_6))
                        {
                            movement_2 = move_7 / moveFactor * Vector3.up;
                        }
                        else
                        {
                            movement_2 = move_6 / moveFactor * new Vector3(rotateSpeed, 0, 0);
                        }


                        if (movement_2 != Vector3.zero)
                        {

                            Vector3 newPosition = _rb.position + new Vector3(0, movement_2.y, 0);
                            float rotateRadius = movement_2.x;

                            _rb.MovePosition(newPosition);
                            GameControl.GetCube(Cube.currentCubeID).transform.Rotate(Vector3.up, rotateRadius, Space.World);
                        }
                    }
                    else if (playMode == PlayMode.Mode_2)
                    {
                        playModeText.text = "Play Mode 2";

                        Vector3 movement = Vector3.zero;


                        if (Mathf.Abs(V_input) >= Mathf.Abs(H_input))
                        {
                            movement = V_input / moveFactor * Vector3.up;
                        }
                        else
                        {
                            movement = H_input / moveFactor * new Vector3(rotateSpeed, 0, 0);
                        }


                        if (movement != Vector3.zero)
                        {
                            //if (!PhotonNetwork.offlineMode)
                            //{
                            //    photonView.RPC("RPCMoveCube_2", PhotonTargets.MasterClient, Cube.currentCubeID,
                            //    movement);
                            //}
                            //else
                            //{
                                Vector3 newPosition = _rb.position + new Vector3(0, movement.y, 0);
                                float rotateRadius = movement.x;

                                _rb.MovePosition(newPosition);
                                GameControl.GetCube(Cube.currentCubeID).transform.Rotate(Vector3.up, rotateRadius, Space.World);
                            //}
                        }
                    }
                }
            }
        }

    }

    void RotateCamera()
    {
        if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Keyboard)
        {
            //reset Rotation
            if (Input.GetMouseButtonDown(1))
            {
                Camera.main.transform.localRotation = Quaternion.Euler(30, 45, 0);
                Camera.main.transform.root.rotation = Quaternion.Euler(0, 130, 0);
            }

            //Drag the camera by scoller
            if (Input.GetMouseButtonDown(2))
            {
                tempPosi = Input.mousePosition;
                isUsing = true;
            }
            if (Input.GetMouseButton(2) && isUsing)
            {
                Vector3 newDir = Input.mousePosition;

                Vector3 deltaRotation = rotateSpeed * (newDir - tempPosi);

                Camera.main.transform.Rotate(Vector3.right, -deltaRotation.y / Screen.height, Space.Self);
                Camera.main.transform.parent.Rotate(Vector3.up, deltaRotation.x / Screen.width);

                tempPosi = newDir;
            }
            if (Input.GetMouseButtonUp(2))
            {
                tempPosi = Input.mousePosition;
                isUsing = false;
            }

            //zoom by scoller
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                targetFoV = Mathf.Clamp(Camera.main.fieldOfView - wheelSpeed * Input.GetAxis("Mouse ScrollWheel"), MinFov, MaxFov);

                // The framerate independent damping factor
                var factor = 1.0f - Mathf.Exp(-Dampening * Time.fixedDeltaTime);

                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFoV, factor);
            }

        }
        else if (TurnGameManager.controlMode == GameControl.ControlMode.PC_Controller)
        {

            move_3 = Input.GetAxis("3rd");
   //         move_4 = Input.GetAxis("4th");
   //         move_5 = Input.GetAxis("5th");

			////rotate camera by controller
			//if (move_4 != 0f || move_5 != 0f && isUsing)
			//{
			//	Vector2 rotateVector = new Vector2(move_4, move_5);

			//	Vector3 newDir = Input.mousePosition;

			//	Vector3 deltaRotation = rotateSpeed * rotateVector;

			//	if ((-deltaRotation.y < 0 && Camera.main.transform.localRotation.x >= 0f) || (-deltaRotation.y > 0 && Camera.main.transform.localRotation.x <= 0.45f))
			//	{
			//		Camera.main.transform.Rotate(Vector3.right, -deltaRotation.y / rotationFactor / 2f, Space.Self);

			//	}

			//	Camera.main.transform.parent.Rotate(Vector3.up, deltaRotation.x / rotationFactor);

			//	tempPosi = newDir;

			//	isUsing = false;
			//}

			//if (move_4 == 0f || move_5 == 0f)
			//{
			//	tempPosi = new Vector2(move_4, move_5);
			//	isUsing = true;
			//}

			//zoom by scoller
			if (move_3 != 0f)
			{
				targetFoV = Mathf.Clamp(Camera.main.fieldOfView - wheelSpeed * move_3, MinFov, MaxFov);

				// The framerate independent damping factor
				var factor = 1.0f - Mathf.Exp(-Dampening * Time.fixedDeltaTime);

				Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFoV, factor);
			}
		}

    }

    void SwitchCubes(Direction _dir)
    {
        if (Cube.highlightedCubeID == 0)
            return;

        hightedCube = GameControl.GetCube(Cube.highlightedCubeID);

        potentialCubes = new List<Cube>();

        if (GameControl.Cubes.Values.Count != 0)
        {
            switch (_dir)
            {
                case Direction.Left:

                    foreach (Cube _cube in GameControl.Cubes.Values)
                    {
                        Vector3 _cDir = (_cube.transform.position - hightedCube.transform.position).normalized;

                        if (Vector3.Dot(_cDir, new Vector3(-Camera.main.transform.right.x, 0, -Camera.main.transform.right.z).normalized) > 0f)
                        {
                            if (Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) <= cubeThreshold)
                            {
                                potentialCubes.Add(_cube);
                            }
                        }
                    }

                    Cube.highlightedCubeID = GetClosestCubeID(potentialCubes);

                    if (Cube.highlightedCubeID != hightedCube.GetCubeLocalID())
                        if (audioManager != null)
                            audioManager.Play("Ding");

                    break;
                case Direction.Right:

                    foreach (Cube _cube in GameControl.Cubes.Values)
                    {
                        Vector3 _cDir = (_cube.transform.position - hightedCube.transform.position).normalized;

                        if (Vector3.Dot(_cDir, new Vector3(Camera.main.transform.right.x, 0, Camera.main.transform.right.z).normalized) > 0f)
                        {
                            if (Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) <= cubeThreshold)
                            {
                                potentialCubes.Add(_cube);
                            }
                        }
                    }

                    Cube.highlightedCubeID = GetClosestCubeID(potentialCubes);

                    if (Cube.highlightedCubeID != hightedCube.GetCubeLocalID())
                        if (audioManager != null)
                            audioManager.Play("Ding");

                    break;
                case Direction.Up:

                    foreach (Cube _cube in GameControl.Cubes.Values)
                    {
                        Vector3 _cDir = (_cube.transform.position - hightedCube.transform.position).normalized;

                        if (Vector3.Dot(_cDir, Vector3.up) > 0f)
                        {
                            if (Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) > cubeThreshold  && Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) < heightThreshold)
                            {
                                potentialCubes.Add(_cube);
                            }
                        }
                    }

					if (potentialCubes.Count == 0)
					{
						foreach (Cube _cube in GameControl.Cubes.Values)
						{
							Vector3 _cDir = (_cube.transform.position - hightedCube.transform.position).normalized;

							if (Vector3.Dot(_cDir, Vector3.up) > 0f)
							{
								if (Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) > cubeThreshold)
								{
									potentialCubes.Add(_cube);
								}
							}
						}
					}

                    Cube.highlightedCubeID = GetClosestCubeID(potentialCubes, true);

                    if (Cube.highlightedCubeID != hightedCube.GetCubeLocalID())
                        if (audioManager != null)
                            audioManager.Play("Ding");

                    break;
                case Direction.Down:

                    foreach (Cube _cube in GameControl.Cubes.Values)
                    {
                        Vector3 _cDir = (_cube.transform.position - hightedCube.transform.position).normalized;

                        if (Vector3.Dot(_cDir, Vector3.down) > 0f)
                        {
                            if (Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) > cubeThreshold && Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) < heightThreshold)
                            {
                                potentialCubes.Add(_cube);
                            }
                        }
                    }

					if (potentialCubes.Count == 0)
					{
						foreach (Cube _cube in GameControl.Cubes.Values)
						{
							Vector3 _cDir = (_cube.transform.position - hightedCube.transform.position).normalized;

							if (Vector3.Dot(_cDir, Vector3.down) > 0f)
							{
								if (Mathf.Abs(_cube.transform.position.y - hightedCube.transform.position.y) > cubeThreshold)
								{
									potentialCubes.Add(_cube);
								}
							}
						}
					}

                    Cube.highlightedCubeID = GetClosestCubeID(potentialCubes, true);

                    if (Cube.highlightedCubeID != hightedCube.GetCubeLocalID())
                        if (audioManager != null)
                            audioManager.Play("Ding");

                    break;
                default:
                    break;
            }
        }

        potentialCubes.Clear();
    }

    int GetClosestCubeID(List<Cube> _potentialCubes, bool isNearCamera = false)
    {
        if (_potentialCubes.Count == 0)
            return Cube.highlightedCubeID;

        int id = Cube.highlightedCubeID;

        float minDistance = 50f;

        if (isNearCamera)
        {
            for (int i = 0; i < _potentialCubes.Count; i++)
            {
                Vector3 refPoint = new Vector3(Camera.main.transform.position.x, _potentialCubes[i].transform.position.y, Camera.main.transform.position.z);

                float _tempDis = (_potentialCubes[i].transform.position - hightedCube.transform.position).magnitude + 2*(refPoint - _potentialCubes[i].GetComponent<Collider>().ClosestPoint(refPoint)).magnitude;
                if (_tempDis < minDistance && Cube.highlightedCubeID != _potentialCubes[i].transform.GetComponent<Cube>().localID)
                {
                    id = _potentialCubes[i].transform.GetComponent<Cube>().localID;
                    minDistance = _tempDis;
                }
            }
        }
        else
        {
            for (int i = 0; i < _potentialCubes.Count; i++)
            {
                float _tempDis = (_potentialCubes[i].transform.position - hightedCube.transform.position).magnitude;
                if (_tempDis < minDistance && Cube.highlightedCubeID != _potentialCubes[i].transform.GetComponent<Cube>().localID)
                {
                    id = _potentialCubes[i].transform.GetComponent<Cube>().localID;
                    minDistance = _tempDis;
                }
            }
        }


        return id;
    }


    public void ShowClickSound()
    {
        if (audioManager != null)
            audioManager.Play("Click");
    }

    //the network function
    [PunRPC]
    void RPCSelectCube(int _id)
    {
        GameControl.GetCube(_id).Select();
        //GameControl.UpdateCube();
    }

    //[PunRPC]
    //void RPCMoveCube_1(int _id, Vector3 _deltaPosition)
    //{
    //    Vector3 newPosition = GameControl.GetCube(_id).transform.position + _deltaPosition;

    //    GameControl.GetCube(_id).GetComponent<Rigidbody>().MovePosition(newPosition);
    //}

    //[PunRPC]
    //void RPCMoveCube_2(int _id, Vector3 _deltaPosition)
    //{
    //    Vector3 newPosition = GameControl.GetCube(_id).transform.position + new Vector3(0, _deltaPosition.y, 0);
    //    float rotateRadius = _deltaPosition.x;

    //    GameControl.GetCube(_id).GetComponent<Rigidbody>().MovePosition(newPosition);
    //    GameControl.GetCube(_id).transform.Rotate(Vector3.up, rotateRadius, Space.World);
    //}

}
