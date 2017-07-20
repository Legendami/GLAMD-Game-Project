﻿using System;
using Assets.Scripts.Enumerations;
using Assets.Scripts.Extensions;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Models;

namespace Assets.Scripts.Controllers
{
	public class PlayerController : MonoBehaviour
	{
		private const int LANE_DISTANCE = 2;

		[SerializeField]
		private float acceleration;

		[SerializeField]
		private float minSpeed;

		[SerializeField]
		private float maxSpeed;

		private float currentSpeed;
		private Lane lane = Lane.Middle;
		private Animator animator;

		private void Awake()
		{
			animator = GetComponent<Animator>();
			currentSpeed = (maxSpeed + minSpeed) / 2;

			Orientation = Orientation.North;
		}

		public Orientation Orientation { get; private set; }

		public bool IsOnLeftCorner { get; set; }

		public bool IsOnRightCorner { get; set; }

		private bool IsOnCorner
		{
			get
			{
				return IsOnLeftCorner || IsOnRightCorner;
			}
		}

		public LanePosition[] TurningPositions { get; set; }

		private void Update()
		{
			Move();

			if (IsOnCorner)
			{
				TakeCorner();
			}
			else
			{
				LaneSwap();
			}
		}

		private void TakeCorner()
		{
			var previousOrientation = Orientation;
			if (IsOnLeftCorner && Input.GetKeyDown(KeyCode.A))
			{
				Orientation = Orientation.GetLeftOrientation();
				IsOnLeftCorner = false;
			}
			else if (IsOnRightCorner && Input.GetKeyDown(KeyCode.D))
			{
				Orientation = Orientation.GetRightOrientation();
				IsOnRightCorner = false;
			}
			
			if (previousOrientation != Orientation)
			{
				transform.rotation = Quaternion.Euler(0, (int)Orientation * 90, 0);
				var turningPosition = TurningPositions.OrderBy(x => Vector3.Distance(x.Position, transform.position)).First();
				transform.position = new Vector3(turningPosition.Position.x, transform.position.y, turningPosition.Position.z);
				lane = turningPosition.Lane;
			}
		}

		private void LaneSwap()
		{
			if (lane != Lane.Left && Input.GetKeyDown(KeyCode.LeftArrow))
			{
				transform.position += Orientation.GetLeftOrientation().GetDirectionVector3() * LANE_DISTANCE;
				lane--;
			}
			else if (lane != Lane.Right && Input.GetKeyDown(KeyCode.RightArrow))
			{
				transform.position += Orientation.GetRightOrientation().GetDirectionVector3() * LANE_DISTANCE;
				lane++;
			}
		}

		private void Move()
		{
			if (currentSpeed < maxSpeed)
			{
				currentSpeed += acceleration;
				animator.SetFloat("Speed", currentSpeed);
			}
			
			transform.position += Orientation.GetDirectionVector3() * currentSpeed * Time.deltaTime;
		}
	}
}
