﻿using UnityEngine;
using System.Collections;

public class PlayerBody : MonoBehaviour {

	[SerializeField] float jumpPower = 5f;

	public GameObject waterlevel;

	public float moveSpeedMultiplier;
	Rigidbody m_Rigidbody;
	Animator animator;
	public enum BodyStatus{isOnGround,isJumping,isSwimming,isFlying};
	public BodyStatus bodyStatus;

	public Vector3 groundNormal;


	public Transform PlayerCamera;
	public float DistanceToGround;
	public Vector3 PlayerVelocity;
	public bool ReadyToFly;


	
	void Start()
	{
		ReadyToFly = false;
		bodyStatus = BodyStatus.isOnGround;
		animator = GetComponent<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		moveSpeedMultiplier = GameInformation.BasePlayer.Balance / 20f;
		m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}
	

	public void MoveBody(Vector3 groundMove,Vector3 freeMove, bool jump)
	{

		CheckGroundStatus ();
		groundMove = Vector3.ProjectOnPlane(groundMove, groundNormal);
		// control and velocity handling is different when grounded and airborne:
		if	(bodyStatus == BodyStatus.isOnGround && jump) 		StartJumping ();
		else if (bodyStatus == BodyStatus.isJumping && jump) 	StartAscending ();
		else if (bodyStatus == BodyStatus.isFlying && jump) 	StartFalling ();
		else if (bodyStatus == BodyStatus.isOnGround) 			m_Rigidbody.velocity = new Vector3 (groundMove.x*moveSpeedMultiplier,m_Rigidbody.velocity.y,groundMove.z*moveSpeedMultiplier) ;
		else if (bodyStatus == BodyStatus.isFlying) 			m_Rigidbody.velocity = m_Rigidbody.velocity*0.96f+freeMove*moveSpeedMultiplier*0.1f ;
		else if (bodyStatus == BodyStatus.isSwimming) 			m_Rigidbody.velocity = m_Rigidbody.velocity*0.90f+freeMove*moveSpeedMultiplier*0.1f ;

		//Debuging
		PlayerVelocity = m_Rigidbody.velocity;
			
		// send input and other state parameters to the animator
		//UpdateAnimator(groundMove);
	}
	
	

	
	
	void UpdateAnimator(Vector3 move)
	{
		// update the animator parameters
		animator.SetFloat("Forward", move.magnitude, 0.1f, Time.deltaTime);
		//animator.SetFloat("Right", move.x, 0.1f, Time.deltaTime);
		animator.SetBool("OnGround", bodyStatus==BodyStatus.isOnGround ? true : false); 

		if (bodyStatus==BodyStatus.isOnGround ? false : true) animator.SetFloat("Jump", m_Rigidbody.velocity.y);


	}

	
	
	void StartJumping()
	{
		m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, jumpPower, m_Rigidbody.velocity.z);
		bodyStatus = BodyStatus.isJumping;

		//animator.applyRootMotion = false;
	}

	void StartAscending()
	{
		m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 4*jumpPower, m_Rigidbody.velocity.z);
		ReadyToFly = true;
	}

	void StartFalling()
	{
		ReadyToFly = false;
		bodyStatus = BodyStatus.isJumping;
		m_Rigidbody.mass = 1f;
	}



	
	void CheckGroundStatus()
	{
		RaycastHit hitInfo;
		// 0.1f is a small offset to start the ray from inside the character
		// it is also good to note that the transform position in the sample assets is at the base of the character


		Physics.Raycast (transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 15000);
		DistanceToGround = hitInfo.distance - 0.1f;
		groundNormal = hitInfo.normal;

		if (transform.position.y<waterlevel.transform.position.y) {
			bodyStatus = BodyStatus.isSwimming;
			m_Rigidbody.mass = 0f;
			//animator.applyRootMotion = false;

		} else if (bodyStatus == BodyStatus.isSwimming || (DistanceToGround < 0.1f && (bodyStatus == BodyStatus.isJumping) && m_Rigidbody.velocity.y <0.01f)) {
			bodyStatus = BodyStatus.isOnGround;
			m_Rigidbody.mass = 1f;
			//animator.applyRootMotion = true;
		} else if (DistanceToGround < 10 && bodyStatus == BodyStatus.isFlying) {
			bodyStatus = BodyStatus.isJumping;
		} else if (DistanceToGround > 15 && bodyStatus == BodyStatus.isJumping && ReadyToFly) {
			bodyStatus = BodyStatus.isFlying;
			m_Rigidbody.mass = 0f;
			ReadyToFly=false;
		}
	}

}