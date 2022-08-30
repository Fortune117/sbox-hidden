﻿using Sandbox;
using System;

namespace Facepunch.Hidden
{
	public partial class HiddenController : CustomWalkController
	{
		[Net, Predicted] public bool IsFrozen { get; set; }

		public override float SprintSpeed { get; set; } = 380f;

		public float LeapVelocity { get; set; } = 300f;
		public float LeapStaminaLoss { get; set; } = 40f;

		public override void AddJumpVelocity()
		{
			if ( Pawn is Player player )
			{
				var minLeapVelocity = (LeapVelocity * 0.2f);
				var extraLeapVelocity = (LeapVelocity * 0.8f);
				var actualLeapVelocity = minLeapVelocity + ( extraLeapVelocity / 100f) * player.Stamina;

				Velocity += (Pawn.EyeRotation.Forward * actualLeapVelocity);

				player.Stamina = MathF.Max( player.Stamina - LeapStaminaLoss, 0f );
			}

			base.AddJumpVelocity();
		}

		public override float GetWishSpeed()
		{
			var speed = base.GetWishSpeed();

			if ( Pawn is Player player )
			{
				if ( player.Deployment == DeploymentType.HIDDEN_BEAST )
					speed *= 0.75f;
				else if ( player.Deployment == DeploymentType.HIDDEN_ROGUE )
					speed *= 1.25f;
			}

			return speed;
		}

		public override void Simulate()
		{
			if ( IsFrozen )
			{
				if ( Input.Pressed( InputButton.Jump ) )
				{
					BaseVelocity = Vector3.Zero;
					WishVelocity = Vector3.Zero;
					Velocity = (Input.Rotation.Forward * LeapVelocity * 2f);
					IsFrozen = false;
				}

				return;
			}

			if ( Pawn is Player player )
			{
				player.Stamina = MathF.Min( player.Stamina + (10f * Time.Delta), 100f );
			}

			base.Simulate();
		}
	}
}
