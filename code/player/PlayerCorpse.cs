﻿using Sandbox;

namespace Facepunch.Hidden
{
	public class PlayerCorpse : ModelEntity
	{
		public Player Player { get; set; }

		public PlayerCorpse()
		{
			PhysicsEnabled = true;
			UsePhysicsCollision = true;

			Tags.Add( "corpse" );
		}

		public void CopyFrom( Player player )
		{
			SetModel( player.GetModelName() );
			TakeDecalsFrom( player );

			// We have to use `this` to refer to the extension methods.
			this.CopyBonesFrom( player );
			this.SetRagdollVelocityFrom( player );

			foreach ( var child in player.Children )
			{
				if ( child is ModelEntity e )
				{
					var model = e.GetModelName();

					if ( model != null && !model.Contains( "clothes" ) )
						continue;

					var clothing = new ModelEntity();
					clothing.SetModel( model );
					clothing.SetParent( this, true );
				}
			}
		}

		public void ApplyForceToBone( Vector3 force, int forceBone )
		{
			PhysicsGroup.AddVelocity( force );

			if ( forceBone >= 0 )
			{
				var body = GetBonePhysicsBody( forceBone );

				if ( body != null )
				{
					body.ApplyForce( force * 1000 );
				}
				else
				{
					PhysicsGroup.AddVelocity( force );
				}
			}
		}
	}
}
