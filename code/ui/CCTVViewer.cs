﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Facepunch.Hidden
{
	public class CCTVViewer : Panel
	{
		public Panel Blinker { get; private set; }
		public Label Name { get; private set; }

		public CCTVViewer()
		{
			StyleSheet.Load( "/ui/CCTVViewer.scss" );

			Blinker = Add.Panel( "blinker" );
			Name = Add.Label( "CCTV", "name" );
		}

		public override void Tick()
		{
			var player = Local.Pawn as Player;
			if ( player == null ) return;

			var game = Game.Instance;
			if ( game == null ) return;

			var round = game.Round;
			if ( round == null ) return;

			var shouldShow = false;

			if ( player.CameraMode is SpectateCamera camera )
			{
				shouldShow = true;

				if ( camera.CCTVEntity.IsValid() )
					Name.Text = camera.CCTVEntity.AreaName;
				else if ( camera.TargetPlayer.IsValid() )
					Name.Text = camera.TargetPlayer.Client.Name;
				else
					Name.Text = "CCTV";
			}

			SetClass( "hidden", !shouldShow );

			base.Tick();
		}
	}
}
