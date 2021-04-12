﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiddenGamemode
{
    public class HuntRound : BaseRound
	{
		public override string RoundName => "Hunt / Survive";
		public override int RoundDuration => 300;

		public List<Player> Spectators = new();

		private string _hiddenHunter;
		private string _firstDeath;
		private int _hiddenKills;

		protected override void OnStart()
		{
			Log.Info( "Started Hunt Round" );

			if ( Host.IsServer )
			{
				Sandbox.Player.All.ForEach( ( player ) => OnPlayerStart( player as Player ) );
			}
		}

		protected override void OnFinish()
		{
			Log.Info( "Finished Hunt Round" );

			if ( Host.IsServer )
			{
				Spectators.Clear();
			}
		}

		protected override void OnTimeUp()
		{
			Log.Info( "Hunt Time Up!" );

			LoadStatsRound( "I.R.I.S. Survived Long Enough" );

			base.OnTimeUp();
		}

		protected void OnPlayerStart(Player player)
		{
			// Give everyone who is alive their starting loadouts.
			if ( player.Team != null && player.LifeState == LifeState.Alive )
			{
				player.Team.SupplyLoadout( player );
				AddPlayer( player );
			}
		}

		public override void OnPlayerKilled( Player player )
		{
			Players.Remove( player );
			Spectators.Add( player );

			player.MakeSpectator();

			if ( player.Team is HiddenTeam )
			{
				if ( player.LastAttacker is Player attacker )
				{
					_hiddenHunter = attacker.Name;
				}

				LoadStatsRound( "I.R.I.S. Eliminated The Hidden" );

				return;
			}
			else
			{
				if ( string.IsNullOrEmpty( _firstDeath ) )
				{
					_firstDeath = player.Name;
				}

				_hiddenKills++;
			}

			if ( Players.Count <= 1 )
			{
				LoadStatsRound( "The Hidden Eliminated I.R.I.S." );
			}
		}

		public override void OnTick()
		{
			if ( Host.IsClient && Sandbox.Player.Local is Player local )
			{
				var hidden = Game.Instance.GetTeamPlayers<HiddenTeam>().First();

				if ( hidden != null && hidden.IsValid() && hidden != local )
				{
					var distance = local.Pos.Distance( hidden.Pos );

					hidden.RenderAlpha = 0.2f - ((0.2f / 1500f) * distance);
				}
			}

			base.OnTick();
		}

		public override void OnPlayerLeave( Player player )
		{
			base.OnPlayerLeave( player );

			Spectators.Remove( player );

			if ( player.Team is HiddenTeam )
			{
				LoadStatsRound( "The Hidden Disconnected" );
			}
		}

		public override void OnPlayerSpawn( Player player )
		{
			player.SetModel( "models/citizen/citizen.vmdl" );
			player.MakeSpectator();

			Spectators.Add( player );
			Players.Remove( player );

			base.OnPlayerSpawn( player );
		}

		private void LoadStatsRound(string winner)
		{
			var hidden = Game.Instance.GetTeamPlayers<HiddenTeam>().First();

			Game.Instance.ChangeRound( new StatsRound
			{
				HiddenName = hidden != null ? hidden.Name : "",
				HiddenKills = _hiddenKills,
				FirstDeath = _firstDeath,
				HiddenHunter = _hiddenHunter,
				Winner = winner
			} );
		}
	}
}
