using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{

	[BoxGroup("Movement Settings")] [SerializeField] private float _walkSpeed;
	[BoxGroup("Movement Settings")] [SerializeField] private float _runSpeed;
	[BoxGroup("Movement Settings")] [SerializeField] private float _gravitySpeed;
	[Space]
	[BoxGroup("Movement Settings")] public float runThreshold;
	[Space]
	[BoxGroup("Movement Settings")] [SerializeField] private float _stopSmoothSpeed;
	[BoxGroup("Movement Settings")] [SerializeField] private float _startSmoothSpeed;
	[BoxGroup("Movement Settings")] [SerializeField] private float _rotationSmoothSpeed;

	[Space]
	[SerializeField] private Transform _IKTargetPivot;


	private Joystick _movementJoystick;
	private Joystick _aimJoystick;

	private CharacterController _characterController;
	public PhotonView photonView;

	private Vector2 _inputRawMovement;
	private Vector2 _inputRawAim;
	private Vector3 _finalInput;

	private bool _active;
	private bool _running;

	private float _movementAngle;
	private float _aimAngle;

	public PlayerBehaviour playerBehaviour;

	void Start()
	{

	}

	void Update()
	{
		if (!_active)
			return;

		if (GameManager.Instance)
			if (GameManager.Instance.online)
				if (photonView)
					if (!photonView.IsMine)
						return;



		GetInput();
		HandleInput();
		CalculateDirection();
		Move();
		Rotate();

	}

	public void Initialize()
	{
		_active = true;
		_movementJoystick = UIManager.Instance.movementJoystick;
		_aimJoystick = UIManager.Instance.aimJoytstick;

		TryGetComponent<CharacterController>(out _characterController);
		TryGetComponent<PhotonView>(out photonView);

		playerBehaviour.die += Die;
	}

	private void GetInput()
	{
		_inputRawMovement.x = _movementJoystick.Horizontal;
		_inputRawMovement.y = _movementJoystick.Vertical;

		_inputRawAim.x = _aimJoystick.Horizontal;
		_inputRawAim.y = _aimJoystick.Vertical;
	}

	Vector3 smoothedVector;
	private void HandleInput()
	{
		_running = _inputRawMovement.magnitude > runThreshold ? true : false;
		_inputRawMovement.Normalize();
		float smoothSpeed = _inputRawMovement.magnitude != 0 ? _startSmoothSpeed : _stopSmoothSpeed;

		Vector3 smoothTarget = new Vector3(_inputRawMovement.x, 0, _inputRawMovement.y);
		smoothedVector = Vector3.Lerp(smoothedVector, smoothTarget, smoothSpeed * Time.deltaTime);

		_finalInput = smoothedVector;
	}

	private void Move()
	{
		float speed = _running ? _runSpeed : _walkSpeed;
		_finalInput = _finalInput * speed;
		_finalInput.y -= _gravitySpeed;

		_characterController.Move(_finalInput * Time.deltaTime);
	}

	private void CalculateDirection()
	{
		if (_inputRawMovement.magnitude == 0 && _inputRawAim.magnitude == 0)
			return;

		_movementAngle = Mathf.Atan2(_inputRawMovement.x, _inputRawMovement.y);
		_movementAngle = Mathf.Rad2Deg * _movementAngle;

		_aimAngle = Mathf.Atan2(_inputRawAim.x, _inputRawAim.y);
		_aimAngle = Mathf.Rad2Deg * _aimAngle;

		//_angle += Camera.main.transform.parent.eulerAngles.y; //Camera Based Direction
	}

	float _desiredAngle;
	private void Rotate()
	{

		if (_inputRawAim.magnitude != 0 && _inputRawMovement.magnitude == 0 && _aimAngle != 0)
			_desiredAngle = _aimAngle;
		else
			_desiredAngle = _movementAngle;

		if (_desiredAngle == 0)
			return;

		Quaternion IKTargetRotation = Quaternion.Euler(0, _aimAngle, 0);
		if (_aimAngle != 0)
			_IKTargetPivot.rotation = Quaternion.Slerp(_IKTargetPivot.rotation, IKTargetRotation, Time.deltaTime * _rotationSmoothSpeed / 1.2f);
		else
			_IKTargetPivot.rotation = Quaternion.Slerp(_IKTargetPivot.rotation, transform.localRotation, Time.deltaTime * _rotationSmoothSpeed / 1.2f);

		Quaternion targetRotation = Quaternion.Euler(0, _desiredAngle, 0);
		transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * _rotationSmoothSpeed);
	}

	private void Die()
	{
		_active = false;
	}

}
