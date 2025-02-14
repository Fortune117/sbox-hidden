﻿using Sandbox;
using System;

namespace Facepunch.Hidden
{
	public struct ViewModelAimConfig
	{
		public float Speed { get; set; }
		public Angles Rotation { get; set; }
		public Vector3 Position { get; set; }
	}

	public partial class ViewModel : BaseViewModel
	{
		public ViewModelAimConfig AimConfig { get; set; }
		public bool IsAiming { get; set; }

		private Vector3 PositionOffset { get; set; }
		private Angles RotationOffset { get; set; }

		private float SwingInfluence => 0.05f;
		private float ReturnSpeed => 5f;
		private float MaxOffsetLength => 10f;
		private float BobCycleTime => 7f;
		private Vector3 BobDirection => new Vector3( 0.0f, 0.5f, 0.25f );

		private Vector3 SwingOffset { get; set; }
		private float LastPitch { get; set; }
		private float LastYaw { get; set; }
		private float BobAnim { get; set; }

		float TargetRoll = 0f;

		Vector3 TargetPos = 0f;

		float MyRoll = 0f;

		public ViewModel() : base()
		{
			AimConfig = new ViewModelAimConfig
			{
				Speed = 1f
			};
		}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			AddCameraEffects( ref camSetup );

			camSetup.ViewModel.FieldOfView = 75f;
			if ( Owner is not Player player )
			return;
			
			if ( player.Controller is IrisController ctrl )
			{
			
				TargetPos = TargetPos.LerpTo( Vector3.Up * (ctrl.Duck.IsActive ? -2f : 0f), 2f * Time.Delta );
				Position += TargetPos;

				TargetRoll = ctrl.Duck.IsActive ? -35f : 0f;

			}
		}


		
		private void AddCameraEffects( ref CameraSetup camSetup )
		{
			if ( Owner is not Player player )
				return;

			Rotation = Local.Pawn.EyeRotation;

			if ( IsAiming )
			{
				PositionOffset = PositionOffset.LerpTo( AimConfig.Position, Time.Delta * AimConfig.Speed );
				RotationOffset = Angles.Lerp( RotationOffset, AimConfig.Rotation, Time.Delta * AimConfig.Speed );
			}
			else
			{
				PositionOffset = PositionOffset.LerpTo( Vector3.Zero, Time.Delta * AimConfig.Speed );
				RotationOffset = Angles.Lerp( RotationOffset, Angles.Zero, Time.Delta * AimConfig.Speed );
			}

			Position += Rotation.Forward * PositionOffset.x + Rotation.Left * PositionOffset.y + Rotation.Up * PositionOffset.z;

			var angles = Rotation.Angles();
			angles += RotationOffset;
			Rotation = angles.ToRotation();

			MyRoll = MyRoll.LerpTo( TargetRoll, Time.Delta * 5f );
			Rotation *= Rotation.From( 0, 0, MyRoll );

			if ( !IsAiming )
			{
				var velocity = player.Velocity;
				var newPitch = Rotation.Pitch();
				var newYaw = Rotation.Yaw();
				var pitchDelta = Angles.NormalizeAngle( newPitch - LastPitch );
				var yawDelta = Angles.NormalizeAngle( LastYaw - newYaw );

				var verticalDelta = velocity.z * Time.Delta;
				var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
				verticalDelta *= (1.0f - MathF.Abs( viewDown.Cross( Vector3.Down ).y ));
				pitchDelta -= verticalDelta * 1;

				var offset = CalcSwingOffset( pitchDelta, yawDelta );
				offset += CalcBobbingOffset( velocity );

				Position += Rotation * offset;

				LastPitch = newPitch;
				LastYaw = newYaw;
			}
		}

		private Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
		{
			Vector3 swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

			SwingOffset -= SwingOffset * ReturnSpeed * Time.Delta;
			SwingOffset += (swingVelocity * SwingInfluence);

			if ( SwingOffset.Length > MaxOffsetLength )
			{
				SwingOffset = SwingOffset.Normal * MaxOffsetLength;
			}

			return SwingOffset;
		}

		private Vector3 CalcBobbingOffset( Vector3 velocity )
		{
			BobAnim += Time.Delta * BobCycleTime;

			var twoPI = MathF.PI * 2.0f;

			if ( BobAnim > twoPI )
			{
				BobAnim -= twoPI;
			}

			var speed = new Vector2( velocity.x, velocity.y ).Length;
			speed = speed > 10.0 ? speed : 0.0f;
			var offset = BobDirection * (speed * 0.005f) * MathF.Cos( BobAnim );
			offset = offset.WithZ( -MathF.Abs( offset.z ) );

			return offset;
		}
	}
}
