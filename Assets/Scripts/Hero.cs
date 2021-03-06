﻿using UnityEngine;
using System.Collections.Generic;
using Jolly;


public class Hero : MonoBehaviour {
	public float MaxSpeed;
	public float MoveForce;
	public float JumpForce;
	public GameObject GroundDetector;
	public GameObject ScreenEdgeDetector;
	public GameObject ProjectileEmitLocator;
	public GameObject Projectile;
	public Camera RenderingCamera;

	public float ForceStrength = 1f;

	private HeroController HeroController;
	private ForceGenerator forceGenerator;
	private MagneticObject magneticObject;

	private bool ShouldJump = false;
	private bool AtEdgeOfScreen = false;
	private bool FacingRight = true;
	private Animator anim;
	private float jumpTime, jumpDelay = 0.5f;
	private bool jumped;

	void Start () {
		this.HeroController = this.GetComponent<HeroController> ();
		this.forceGenerator = this.GetComponentInChildren<ForceGenerator> ();
		this.magneticObject = this.GetComponent<MagneticObject> ();
		this.anim = this.GetComponent<Animator> ();

		JollyDebug.Watch (this, "FacingRight", delegate () {
			return this.FacingRight;
		});
	}

	void Update () {
		bool grounded = Physics2D.Linecast (this.transform.position, this.GroundDetector.transform.position, 1 << LayerMask.NameToLayer ("Ground"));
		JollyDebug.Watch (this, "Grounded", grounded);
		if (this.HeroController.Jump && grounded) {
			this.ShouldJump = true;
		
		}
		float viewportPointOfEdgeDetector = this.RenderingCamera.WorldToViewportPoint (this.ScreenEdgeDetector.transform.position).x;
		this.AtEdgeOfScreen = viewportPointOfEdgeDetector < 0.0f || viewportPointOfEdgeDetector >= 1.0f;

	}

	void FixedUpdate () {
		float horizontal = this.HeroController.HorizontalMovementAxis;
		bool grounded = Physics2D.Linecast (this.transform.position, this.GroundDetector.transform.position, 1 << LayerMask.NameToLayer ("Ground"));
		anim.SetFloat ("Speed", Mathf.Abs (horizontal));
		
		bool movingIntoScreenEdge = (horizontal > 0 && this.FacingRight) || (horizontal < 0 && !this.FacingRight);
		if (this.AtEdgeOfScreen && movingIntoScreenEdge) {
			this.rigidbody2D.velocity = new Vector2 (0, this.rigidbody2D.velocity.y);
			horizontal = 0.0f;
		}

		if (horizontal * this.rigidbody2D.velocity.x < this.MaxSpeed) {
			this.rigidbody2D.AddForce (Vector2.right * horizontal * MoveForce);
		}

		float maxSpeed = Mathf.Abs (this.MaxSpeed * horizontal);
		if (Mathf.Abs (this.rigidbody2D.velocity.x) > maxSpeed) {
			this.rigidbody2D.velocity = new Vector2 (Mathf.Sign (this.rigidbody2D.velocity.x) * maxSpeed, this.rigidbody2D.velocity.y);
		}

		if (this.ShouldJump) {
			this.rigidbody2D.AddForce (Vector2.up * JumpForce);
			this.anim.SetTrigger ("Jump");
			jumpTime = jumpDelay;
			jumped = true;
			this.ShouldJump = false;
		}
		jumpTime -= Time.deltaTime;
		if (jumpTime <= 0 && grounded && jumped) {
			this.anim.SetTrigger ("Land");
			jumped = false;
		}

		if ((horizontal > 0 && !this.FacingRight) || (horizontal < 0 && this.FacingRight)) {
			this.Flip ();
		}

		this.forceGenerator.Active = this.HeroController.ForceOn;
		this.magneticObject.Active = this.HeroController.ForceOn;
	}

	void Flip () {
		this.FacingRight = !this.FacingRight;
		this.transform.localScale = this.transform.localScale.SetX (this.transform.localScale.x * -1f);
	}
}
