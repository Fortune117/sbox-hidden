﻿using Sandbox;

namespace Facepunch.Hidden
{
	public abstract partial class Throwable<T> : BulletDropWeapon<T> where T : BulletDropProjectile, new()
	{
		public override string ImpactEffect => null;
		public override string ViewModelPath => "models/grenade/fp_grenade.vmdl";
		public override int ViewModelMaterialGroup => 1;
		public override string MuzzleFlashEffect => null;
		public override float PrimaryRate => 1f;
		public override float SecondaryRate => 1f;
		public override float Speed => 1300f;
		public override float Gravity => 5f;
		public override float InheritVelocity => 0f;
		public override string ProjectileModel => string.Empty;
		public override int ClipSize => 0;
		public override float ReloadTime => 2.3f;
		public override float ProjectileLifeTime => 4f;

		public virtual float ThrowAnimationTime => 0.8f;
		public virtual string ThrowSound => null;

		[Net, Predicted] private TimeUntil NextThrowTime { get; set; }
		[Net, Predicted] private bool HasBeenThrown { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/weapons/w_held_item.vmdl" );
		}

		public override void ActiveEnd( Entity owner, bool dropped )
		{
			if ( HasBeenThrown )
			{
				HasBeenThrown = false;
				AmmoClip++;
			}

			base.ActiveEnd( owner, dropped );
		}

		public override void AttackPrimary()
		{
			if ( HasBeenThrown ) return;

			if ( !TakeAmmo( 1 ) ) return;

			if ( !string.IsNullOrEmpty( ThrowSound ) )
				PlaySound( ThrowSound );

			PlayAttackAnimation();
			ShootEffects();
			OnThrown();

			NextThrowTime = ThrowAnimationTime;
			HasBeenThrown = true;
		}

		public override void Simulate( Client owner )
		{
			if ( Prediction.FirstTime && HasBeenThrown && NextThrowTime )
			{
				if ( AmmoClip > 0 )
				{
					ViewModelEntity?.SetAnimParameter( "deploy", true );
				}

				Rand.SetSeed( Time.Tick );
				FireProjectile();

				HasBeenThrown = false;
			}

			base.Simulate( owner );
		}

		public override void CreateViewModel()
		{
			if ( AmmoClip > 0 )
			{
				base.CreateViewModel();
				ViewModelEntity?.SetAnimParameter( "deploy", true );
			}
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetAnimParameter( "holdtype", 5 );
			anim.SetAnimParameter( "aim_body_weight", 1 );

			ViewModelEntity?.SetAnimParameter( "b_grounded", false );

			if ( Owner.IsValid() )
			{
				ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
			}
		}

		protected virtual void OnThrown()
		{

		}

		protected override void ShootEffects()
		{
			base.ShootEffects();

			ViewModelEntity?.SetAnimParameter( "attack", true );
			ViewModelEntity?.SetAnimParameter( "holdtype_attack", 1 );
		}

		protected override void OnProjectileFired( T projectile )
		{
			if ( IsClient && IsFirstPersonMode )
			{
				//projectile.Position = EffectEntity.Position + EffectEntity.Rotation.Forward * 24f + EffectEntity.Rotation.Right * 8f + EffectEntity.Rotation.Down * 4f;
			}
		}

		protected override void OnProjectileHit( T projectile, TraceResult trace )
		{
			
		}
	}
}
