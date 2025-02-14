﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Hidden
{
    public abstract partial class BaseRound : BaseNetworkable
	{
		public virtual int RoundDuration => 0;
		public virtual string RoundName => "";
		public virtual bool CanPlayerSuicide => false;
		public virtual bool CanPlayerTakeDamage => true;

		public List<Player> Players = new();

		public float RoundEndTime { get; set; }

		public float TimeLeft
		{
			get
			{
				return RoundEndTime - Time.Now;
			}
		}

		[Net] public string TimeLeftFormatted { get; set; }

		public void Start()
		{
			if ( Host.IsServer && RoundDuration > 0 )
			{
				RoundEndTime = Time.Now + RoundDuration;
			}
			
			OnStart();
		}

		public void Finish()
		{
			if ( Host.IsServer )
			{
				RoundEndTime = 0f;
				Players.Clear();
			}

			OnFinish();
		}

		public void AddPlayer( Player player )
		{
			Host.AssertServer();

			if ( !Players.Contains(player) )
			{
				Players.Add( player );
			}
		}

		public virtual void OnPlayerSpawn( Player player ) { }

		public virtual void OnPlayerKilled( Player player ) { }

		public virtual void OnPlayerLeave( Player player )
		{
			Players.Remove( player );
		}

		public virtual void OnTick() { }

		public virtual void OnSecond()
		{
			if ( Host.IsServer )
			{
				if ( RoundEndTime == 0f )
				{
					TimeLeftFormatted = TimeSpan.FromSeconds( 0f ).ToString( @"mm\:ss" );
					return;
				}

				if ( RoundEndTime > 0f && Time.Now >= RoundEndTime )
				{
					RoundEndTime = 0f;
					OnTimeUp();
				}
				else
				{
					TimeLeftFormatted = TimeSpan.FromSeconds( TimeLeft ).ToString( @"mm\:ss" );
				}
			}
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }
	}
}
