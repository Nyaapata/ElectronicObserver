﻿using ElectronicObserver.Data;
using ElectronicObserver.Data.Battle;
using ElectronicObserver.Data.Battle.Detail;
using ElectronicObserver.Data.Battle.Phase;
using ElectronicObserver.Observer;
using ElectronicObserver.Resource;
using ElectronicObserver.Window.Control;
using ElectronicObserver.Window.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace ElectronicObserver.Window {

	public partial class FormBattle : DockContent {

		private readonly Color WinRankColor_Win = Utility.Configuration.Config.UI.ForeColor;
		private readonly Color WinRankColor_Lose = Utility.Configuration.Config.UI.Color_Red;

		private readonly Size DefaultBarSize = new Size( 80, 20 );
		private readonly Size SmallBarSize = new Size( 60, 20 );

		private List<ShipStatusHP> HPBars;

		public Font MainFont { get; set; }
		public Font SubFont { get; set; }



		public FormBattle( FormMain parent ) {
			InitializeComponent();

			ControlHelper.SetDoubleBuffered( TableTop );
			ControlHelper.SetDoubleBuffered( TableBottom );


			HPBars = new List<ShipStatusHP>( 24 );


			TableBottom.SuspendLayout();
			for ( int i = 0; i < 24; i++ ) {
				HPBars.Add( new ShipStatusHP() );
				HPBars[i].Size = DefaultBarSize;
				HPBars[i].AutoSize = false;
				HPBars[i].AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
				HPBars[i].Margin = new Padding( 2, 0, 2, 0 );
				HPBars[i].Anchor = AnchorStyles.Left | AnchorStyles.Right;
				HPBars[i].MainFont = MainFont;
				HPBars[i].SubFont = SubFont;
				HPBars[i].UsePrevValue = true;
				HPBars[i].ShowDifference = true;
				HPBars[i].MaximumDigit = 9999;

				if ( i < 6 ) {
					TableBottom.Controls.Add( HPBars[i], 0, i + 1 );
				} else if ( i < 12 ) {
					TableBottom.Controls.Add( HPBars[i], 3, i - 5 );
				} else if ( i < 18 ) {
					TableBottom.Controls.Add( HPBars[i], 1, i - 11 );
				} else {
					TableBottom.Controls.Add( HPBars[i], 2, i - 17 );
				}
			}
			TableBottom.ResumeLayout();


			Searching.ImageList =
			SearchingFriend.ImageList =
			SearchingEnemy.ImageList =
			AACutin.ImageList =
			AirStage1Friend.ImageList =
			AirStage1Enemy.ImageList =
			AirStage2Friend.ImageList =
			AirStage2Enemy.ImageList =
			FleetFriend.ImageList =
				ResourceManager.Instance.Equipments;


			ConfigurationChanged();

			BaseLayoutPanel.Visible = false;


			Icon = ResourceManager.ImageToIcon( ResourceManager.Instance.Icons.Images[(int)ResourceManager.IconContent.FormBattle] );

		}



		private void FormBattle_Load( object sender, EventArgs e ) {

			APIObserver o = APIObserver.Instance;

			o.APIList["api_port/port"].ResponseReceived += Updated;
			o.APIList["api_req_map/start"].ResponseReceived += Updated;
			o.APIList["api_req_map/next"].ResponseReceived += Updated;
			o.APIList["api_req_sortie/battle"].ResponseReceived += Updated;
			o.APIList["api_req_sortie/battleresult"].ResponseReceived += Updated;
			o.APIList["api_req_battle_midnight/battle"].ResponseReceived += Updated;
			o.APIList["api_req_battle_midnight/sp_midnight"].ResponseReceived += Updated;
			o.APIList["api_req_sortie/airbattle"].ResponseReceived += Updated;
			o.APIList["api_req_sortie/ld_airbattle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/battle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/midnight_battle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/sp_midnight"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/airbattle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/battle_water"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/ld_airbattle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/ec_battle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/ec_midnight_battle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/each_battle"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/each_battle_water"].ResponseReceived += Updated;
			o.APIList["api_req_combined_battle/battleresult"].ResponseReceived += Updated;
			o.APIList["api_req_practice/battle"].ResponseReceived += Updated;
			o.APIList["api_req_practice/midnight_battle"].ResponseReceived += Updated;
			o.APIList["api_req_practice/battle_result"].ResponseReceived += Updated;

			Utility.Configuration.Instance.ConfigurationChanged += ConfigurationChanged;

		}


		private void Updated( string apiname, dynamic data ) {

			KCDatabase db = KCDatabase.Instance;
			BattleManager bm = db.Battle;
			bool hideDuringBattle = Utility.Configuration.Config.FormBattle.HideDuringBattle;

			BaseLayoutPanel.SuspendLayout();
			TableTop.SuspendLayout();
			TableBottom.SuspendLayout();
			switch ( apiname ) {

				case "api_port/port":
					BaseLayoutPanel.Visible = false;
					ToolTipInfo.RemoveAll();
					break;

				case "api_req_map/start":
				case "api_req_map/next":
					if ( !bm.Compass.HasAirRaid )
						goto case "api_port/port";

					SetFormation( bm );
					ClearSearchingResult();
					ClearBaseAirAttack();
					SetAerialWarfare( null, ( (BattleBaseAirRaid)bm.BattleDay ).BaseAirRaid );
					SetHPBar( bm.BattleDay );
					SetDamageRate( bm );

					BaseLayoutPanel.Visible = !hideDuringBattle;
					break;


				case "api_req_sortie/battle":
				case "api_req_practice/battle":
				case "api_req_sortie/ld_airbattle": {

						SetFormation( bm );
						SetSearchingResult( bm.BattleDay );
						SetBaseAirAttack( bm.BattleDay.BaseAirAttack );
						SetAerialWarfare( bm.BattleDay.JetAirBattle, bm.BattleDay.AirBattle );
						SetHPBar( bm.BattleDay );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_battle_midnight/battle":
				case "api_req_practice/midnight_battle": {

						SetNightBattleEvent( bm.BattleNight.NightBattle );
						SetHPBar( bm.BattleNight );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_battle_midnight/sp_midnight": {

						SetFormation( bm );
						ClearBaseAirAttack();
						ClearAerialWarfare();
						ClearSearchingResult();
						SetNightBattleEvent( bm.BattleNight.NightBattle );
						SetHPBar( bm.BattleNight );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_sortie/airbattle": {

						SetFormation( bm );
						SetSearchingResult( bm.BattleDay );
						SetBaseAirAttack( bm.BattleDay.BaseAirAttack );
						SetAerialWarfare( bm.BattleDay.JetAirBattle, bm.BattleDay.AirBattle, ( (BattleAirBattle)bm.BattleDay ).AirBattle2 );
						SetHPBar( bm.BattleDay );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_combined_battle/battle":
				case "api_req_combined_battle/battle_water":
				case "api_req_combined_battle/ld_airbattle":
				case "api_req_combined_battle/ec_battle":
				case "api_req_combined_battle/each_battle":
				case "api_req_combined_battle/each_battle_water": {

						SetFormation( bm );
						SetSearchingResult( bm.BattleDay );
						SetBaseAirAttack( bm.BattleDay.BaseAirAttack );
						SetAerialWarfare( bm.BattleDay.JetAirBattle, bm.BattleDay.AirBattle );
						SetHPBar( bm.BattleDay );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_combined_battle/airbattle": {

						SetFormation( bm );
						SetSearchingResult( bm.BattleDay );
						SetBaseAirAttack( bm.BattleDay.BaseAirAttack );
						SetAerialWarfare( bm.BattleDay.JetAirBattle, bm.BattleDay.AirBattle, ( (BattleCombinedAirBattle)bm.BattleDay ).AirBattle2 );
						SetHPBar( bm.BattleDay );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_combined_battle/midnight_battle":
				case "api_req_combined_battle/ec_midnight_battle": {

						SetNightBattleEvent( bm.BattleNight.NightBattle );
						SetHPBar( bm.BattleNight );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;

				case "api_req_combined_battle/sp_midnight": {

						SetFormation( bm );
						ClearAerialWarfare();
						ClearSearchingResult();
						ClearBaseAirAttack();
						SetNightBattleEvent( bm.BattleNight.NightBattle );
						SetHPBar( bm.BattleNight );
						SetDamageRate( bm );

						BaseLayoutPanel.Visible = !hideDuringBattle;
					} break;


				case "api_req_sortie/battleresult":
				case "api_req_combined_battle/battleresult":
				case "api_req_practice/battle_result": {

						SetMVPShip( bm );

						BaseLayoutPanel.Visible = true;
					} break;

			}

			TableTop.ResumeLayout();
			TableBottom.ResumeLayout();

			BaseLayoutPanel.ResumeLayout();


			if ( Utility.Configuration.Config.UI.IsLayoutFixed )
				TableTop.Width = TableTop.GetPreferredSize( BaseLayoutPanel.Size ).Width;
			else
				TableTop.Width = TableBottom.ClientSize.Width;
			TableTop.Height = TableTop.GetPreferredSize( BaseLayoutPanel.Size ).Height;

		}


		/// <summary>
		/// 陣形・交戦形態を設定します。
		/// </summary>
		private void SetFormation( BattleManager bm ) {

			FormationFriend.Text = Constants.GetFormationShort( bm.FirstBattle.Searching.FormationFriend );
			FormationEnemy.Text = Constants.GetFormationShort( bm.FirstBattle.Searching.FormationEnemy );
			Formation.Text = Constants.GetEngagementForm( bm.FirstBattle.Searching.EngagementForm );

			if ( bm.Compass != null && bm.Compass.EventID == 5 ) {
				FleetEnemy.ForeColor = Utility.Configuration.Config.UI.Color_Red;
			} else {
				FleetEnemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
			}
		}

		/// <summary>
		/// 索敵結果を設定します。
		/// </summary>
		private void SetSearchingResult( BattleData bd ) {

			int searchFriend = bd.Searching.SearchingFriend;
			SearchingFriend.Text = Constants.GetSearchingResultShort( searchFriend );
			SearchingFriend.ImageAlign = searchFriend > 0 ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter;
			SearchingFriend.ImageIndex = searchFriend > 0 ? (int)( searchFriend < 4 ? ResourceManager.EquipmentContent.Seaplane : ResourceManager.EquipmentContent.Radar ) : -1;
			ToolTipInfo.SetToolTip( SearchingFriend, null );

			int searchEnemy = bd.Searching.SearchingEnemy;
			SearchingEnemy.Text = Constants.GetSearchingResultShort( searchEnemy );
			SearchingEnemy.ImageAlign = searchEnemy > 0 ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter;
			SearchingEnemy.ImageIndex = searchEnemy > 0 ? (int)( searchEnemy < 4 ? ResourceManager.EquipmentContent.Seaplane : ResourceManager.EquipmentContent.Radar ) : -1;
			ToolTipInfo.SetToolTip( SearchingEnemy, null );

		}

		/// <summary>
		/// 索敵結果をクリアします。
		/// 索敵フェーズが発生しなかった場合にこれを設定します。
		/// </summary>
		private void ClearSearchingResult() {

			SearchingFriend.Text = "-";
			SearchingFriend.ImageAlign = ContentAlignment.MiddleCenter;
			SearchingFriend.ImageIndex = -1;
			ToolTipInfo.SetToolTip( SearchingFriend, null );

			SearchingEnemy.Text = "-";
			SearchingEnemy.ImageAlign = ContentAlignment.MiddleCenter;
			SearchingEnemy.ImageIndex = -1;
			ToolTipInfo.SetToolTip( SearchingEnemy, null );

		}

		/// <summary>
		/// 基地航空隊フェーズの結果を設定します。
		/// </summary>
		private void SetBaseAirAttack( PhaseBaseAirAttack pd ) {

			if ( pd != null && pd.IsAvailable ) {

				Searching.Text = "LBAS";
				Searching.ImageAlign = ContentAlignment.MiddleLeft;
				Searching.ImageIndex = (int)ResourceManager.EquipmentContent.LandAttacker;

				var sb = new StringBuilder();
				int index = 1;

				foreach ( var phase in pd.AirAttackUnits ) {

					sb.AppendFormat( GeneralRes.BaseWave + " - " + GeneralRes.BaseAirCorps + " :\r\n",
						index, phase.AirUnitID );

					if ( phase.IsStage1Available ) {
						sb.AppendFormat("　St1: " + GeneralRes.FriendlyAir + " -{0}/{1} | " + GeneralRes.EnemyAir + " -{2}/{3} | {4}\r\n",
							phase.AircraftLostStage1Friend, phase.AircraftTotalStage1Friend,
							phase.AircraftLostStage1Enemy, phase.AircraftTotalStage1Enemy,
							Constants.GetAirSuperiority( phase.AirSuperiority ) );
					}
					if ( phase.IsStage2Available ) {
						sb.AppendFormat("　St2: " + GeneralRes.FriendlyAir + " -{0}/{1} | " + GeneralRes.EnemyAir + " -{2}/{3}\r\n",
							phase.AircraftLostStage2Friend, phase.AircraftTotalStage2Friend,
							phase.AircraftLostStage2Enemy, phase.AircraftTotalStage2Enemy );
					}

					index++;
				}

				ToolTipInfo.SetToolTip( Searching, sb.ToString() );


			} else {
				ClearBaseAirAttack();
			}

		}

		/// <summary>
		/// 基地航空隊フェーズの結果をクリアします。
		/// </summary>
		private void ClearBaseAirAttack() {
			Searching.Text = GeneralRes.ClearBaseAirAttack;
			Searching.ImageAlign = ContentAlignment.MiddleCenter;
			Searching.ImageIndex = -1;
			ToolTipInfo.SetToolTip( Searching, null );
		}



		/// <summary>
		/// 航空戦情報を設定します。
		/// </summary>
		/// <param name="phaseJet">噴式航空戦を指定します。存在しない場合は null を指定してください。</param>
		/// <param name="phase1">通常の航空戦を指定します。</param>
		private void SetAerialWarfare( PhaseJetAirBattle phaseJet, PhaseAirBattleBase phase1 ) {
			SetAerialWarfare( phaseJet, phase1, null );
		}

		/// <summary>
		/// 航空戦情報を設定します。
		/// </summary>
		/// <param name="phaseJet">噴式航空戦を指定します。存在しない場合は null を指定してください。</param>
		/// <param name="phase1">第1次航空戦を指定します。</param>
		/// <param name="phase2">第2次航空戦を指定します。存在しない場合は null を指定してください。</param>
		private void SetAerialWarfare( PhaseJetAirBattle phaseJet, PhaseAirBattleBase phase1, PhaseAirBattleBase phase2 ) {

			bool phaseJetEnabled = phaseJet != null && phaseJet.IsAvailable;
			bool phase1Enabled = phase1 != null && phase1.IsAvailable;
			bool phase2Enabled = phase2 != null && phase2.IsAvailable;


			// 空対空戦闘
			if ( phase1Enabled && phase1.IsStage1Available ) {

				bool phaseJetStage1Enabled = phaseJetEnabled && phaseJet.IsStage1Available;
				bool phase2Stage1Enabled = phase2Enabled && phase2.IsStage1Available;
				bool needAppendInfo = phaseJetStage1Enabled || phase2Stage1Enabled;

				switch (phase1.AirSuperiority) {
					default: break;
					case 0: break; //AP
					case 1: //AS+
						AirSuperiority.ForeColor = Utility.Configuration.Config.UI.Color_Green;
						break;
					case 2:
						AirSuperiority.ForeColor = Utility.Configuration.Config.UI.Color_Green;
						break;
					case 3:
						AirSuperiority.ForeColor = Utility.Configuration.Config.UI.Color_Red;
						break;
					case 4: //AI-
						AirSuperiority.ForeColor = Utility.Configuration.Config.UI.Color_Red;
						break;
				}

				AirSuperiority.Text = Constants.GetAirSuperiority( phase1.AirSuperiority );

				if ( needAppendInfo ) {

					var sb = new StringBuilder();

					if ( phaseJetStage1Enabled )
						sb.Append( "Jet: " ).AppendLine( Constants.GetAirSuperiority( phaseJet.AirSuperiority ) );

					sb.Append( "Stage 1: " ).AppendLine( Constants.GetAirSuperiority( phase1.AirSuperiority ) );

					if ( phase2Stage1Enabled )
						sb.Append( "Stage 2: " ).AppendLine( Constants.GetAirSuperiority( phase2.AirSuperiority ) );

					ToolTipInfo.SetToolTip( AirSuperiority, sb.ToString() );

				} else {
					ToolTipInfo.SetToolTip( AirSuperiority, null );
				}


				// friends
				int jetLostFriend = phaseJetStage1Enabled ? phaseJet.AircraftLostStage1Friend : 0;
				int phase1LostFriend = phase1.AircraftLostStage1Friend;
				int phase2LostFriend = phase2Stage1Enabled ? phase2.AircraftLostStage1Friend : 0;

				int jetTotalFriend = phaseJetStage1Enabled ? phaseJet.AircraftTotalStage1Friend : 0;
				int phase1TotalFriend = phase1.AircraftTotalStage1Friend;
				int phase2TotalFriend = phase2Stage1Enabled ? phase2.AircraftTotalStage1Friend : 0;

				int jetTouchFriend = phaseJetStage1Enabled ? phaseJet.TouchAircraftFriend : -1;
				int phase1TouchFriend = phase1.TouchAircraftFriend;
				int phase2TouchFriend = phase2Stage1Enabled ? phase2.TouchAircraftFriend : -1;

				if ( needAppendInfo ) {
					var text = new List<string>();

					if ( phaseJetStage1Enabled )
						text.Add( "-" + jetLostFriend );

					text.Add( "-" + phase1LostFriend );

					if ( phase2Stage1Enabled )
						text.Add( "-" + phase2LostFriend );

					AirStage1Friend.Text = string.Join( ",", text );

				} else {
					AirStage1Friend.Text = string.Format( "-{0}/{1}", phase1LostFriend, phase1TotalFriend );
				}

				if ( needAppendInfo ) {

					var sb = new StringBuilder();

					if ( phaseJetStage1Enabled )
						sb.AppendFormat( "Jet: -{0}/{1}\r\n", jetLostFriend, jetTotalFriend );

					sb.AppendFormat( "Stage 1: -{0}/{1}\r\n", phase1LostFriend, phase1TotalFriend );

					if ( phase2Stage1Enabled )
						sb.AppendFormat( "Stage 2: -{0}/{1}\r\n", phase2LostFriend, phase2TotalFriend );

					ToolTipInfo.SetToolTip( AirStage1Friend, sb.ToString() );
				} else {
					ToolTipInfo.SetToolTip( AirStage1Friend, null );
				}

				// lost flag
				if ( ( jetTotalFriend > 0 && jetLostFriend == jetTotalFriend ) ||
					( phase1TotalFriend > 0 && phase1LostFriend == phase1TotalFriend ) ||
					( phase2TotalFriend > 0 && phase2LostFriend == phase2TotalFriend ) ) {
					AirStage1Friend.ForeColor = Utility.Configuration.Config.UI.Color_Red;
				} else {
					AirStage1Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				}

				// touch
				if ( jetTouchFriend > 0 || phase1TouchFriend > 0 || phase2TouchFriend > 0 ) {
					AirStage1Friend.ImageAlign = ContentAlignment.MiddleLeft;
					AirStage1Friend.ImageIndex = (int)ResourceManager.EquipmentContent.Seaplane;

					var jetTouchPlane = KCDatabase.Instance.MasterEquipments[jetTouchFriend];
					var phase1TouchPlane = KCDatabase.Instance.MasterEquipments[phase1TouchFriend];
					var phase2TouchPlane = KCDatabase.Instance.MasterEquipments[phase2TouchFriend];

					var sb = new StringBuilder( ToolTipInfo.GetToolTip( AirStage1Friend ) );
					sb.AppendLine( "Contact" );

					if ( phaseJetStage1Enabled )
						sb.AppendFormat( "Jet: {0}\r\n", jetTouchPlane != null ? jetTouchPlane.Name : "(なし)" );
					if ( needAppendInfo )
						sb.Append( "Stage 1: " );
					sb.AppendFormat( "{0}\r\n", phase1TouchPlane != null ? phase1TouchPlane.Name : "(なし)" );
					if ( phase2Stage1Enabled )
						sb.AppendFormat( "Stage 2: {0}\r\n", phase2TouchPlane != null ? phase2TouchPlane.Name : "(なし)" );

					ToolTipInfo.SetToolTip( AirStage1Friend, sb.ToString() );

				} else {
					AirStage1Friend.ImageAlign = ContentAlignment.MiddleCenter;
					AirStage1Friend.ImageIndex = -1;
				}



				// enemies
				int jetLostEnemy = phaseJetStage1Enabled ? phaseJet.AircraftLostStage1Enemy : 0;
				int phase1LostEnemy = phase1.AircraftLostStage1Enemy;
				int phase2LostEnemy = phase2Stage1Enabled ? phase2.AircraftLostStage1Enemy : 0;

				int jetTotalEnemy = phaseJetStage1Enabled ? phaseJet.AircraftTotalStage1Enemy : 0;
				int phase1TotalEnemy = phase1.AircraftTotalStage1Enemy;
				int phase2TotalEnemy = phase2Stage1Enabled ? phase2.AircraftTotalStage1Enemy : 0;

				int jetTouchEnemy = phaseJetStage1Enabled ? phaseJet.TouchAircraftEnemy : -1;
				int phase1TouchEnemy = phase1.TouchAircraftEnemy;
				int phase2TouchEnemy = phase2Stage1Enabled ? phase2.TouchAircraftEnemy : -1;

				if ( needAppendInfo ) {
					var text = new List<string>();

					if ( phaseJetStage1Enabled )
						text.Add( "-" + jetLostEnemy );

					text.Add( "-" + phase1LostEnemy );

					if ( phase2Stage1Enabled )
						text.Add( "-" + phase2LostEnemy );

					AirStage1Enemy.Text = string.Join( ",", text );

				} else {
					AirStage1Enemy.Text = string.Format( "-{0}/{1}", phase1LostEnemy, phase1TotalEnemy );
				}

				if ( needAppendInfo ) {

					var sb = new StringBuilder();

					if ( phaseJetStage1Enabled )
						sb.AppendFormat( "Jet: -{0}/{1}\r\n", jetLostEnemy, jetTotalEnemy );

					sb.AppendFormat( "Stage 1: -{0}/{1}\r\n", phase1LostEnemy, phase1TotalEnemy );

					if ( phase2Stage1Enabled )
						sb.AppendFormat( "第2次: -{0}/{1}\r\n", phase2LostEnemy, phase2TotalEnemy );

					ToolTipInfo.SetToolTip( AirStage1Enemy, sb.ToString() );
				} else {
					ToolTipInfo.SetToolTip( AirStage1Enemy, null );
				}

				// lost flag
				if ( ( jetTotalEnemy > 0 && jetLostEnemy == jetTotalEnemy ) ||
					( phase1TotalEnemy > 0 && phase1LostEnemy == phase1TotalEnemy ) ||
					( phase2TotalEnemy > 0 && phase2LostEnemy == phase2TotalEnemy ) ) {
					AirStage1Enemy.ForeColor = Utility.Configuration.Config.UI.Color_Red;
				} else {
					AirStage1Enemy.ForeColor = SystemColors.ControlText;
				}

				// touch
				if ( jetTouchEnemy > 0 || phase1TouchEnemy > 0 || phase2TouchEnemy > 0 ) {
					AirStage1Enemy.ImageAlign = ContentAlignment.MiddleLeft;
					AirStage1Enemy.ImageIndex = (int)ResourceManager.EquipmentContent.Seaplane;

					var jetTouchPlane = KCDatabase.Instance.MasterEquipments[jetTouchEnemy];
					var phase1TouchPlane = KCDatabase.Instance.MasterEquipments[phase1TouchEnemy];
					var phase2TouchPlane = KCDatabase.Instance.MasterEquipments[phase2TouchEnemy];

					var sb = new StringBuilder( ToolTipInfo.GetToolTip( AirStage1Enemy ) );
					sb.AppendLine( "Contact" );

					if ( phaseJetStage1Enabled )
						sb.AppendFormat( "Jet: {0}\r\n", jetTouchPlane != null ? jetTouchPlane.Name : "(なし)" );
					if ( needAppendInfo )
						sb.Append( "Stage 1: " );
					sb.AppendFormat( "{0}\r\n", phase1TouchPlane != null ? phase1TouchPlane.Name : "(なし)" );
					if ( phase2Stage1Enabled )
						sb.AppendFormat( "Stage 2: {0}\r\n", phase2TouchPlane != null ? phase2TouchPlane.Name : "(なし)" );

					ToolTipInfo.SetToolTip( AirStage1Enemy, sb.ToString() );

				} else {
					AirStage1Enemy.ImageAlign = ContentAlignment.MiddleCenter;
					AirStage1Enemy.ImageIndex = -1;
				}



			} else {	// 空対空戦闘発生せず
				AirSuperiority.Text = Constants.GetAirSuperiority( -1 );
				ToolTipInfo.SetToolTip( AirSuperiority, null );
				AirStage1Friend.Text = "-";
				AirStage1Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				ToolTipInfo.SetToolTip( AirStage1Friend, null );
				AirStage1Enemy.Text = "-";
				AirStage1Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				ToolTipInfo.SetToolTip( AirStage1Enemy, null );
			}



			// 艦対空戦闘
			if ( phase1Enabled && phase1.IsStage2Available ) {

				bool phaseJetStage2Enabled = phaseJetEnabled && phaseJet.IsStage2Available;
				bool phase2Stage2Enabled = phase2Enabled && phase2.IsStage2Available;
				bool needAppendInfo = phaseJetStage2Enabled || phase2Stage2Enabled;

				// friends
				int jetLostFriend = phaseJetStage2Enabled ? phaseJet.AircraftLostStage2Friend : 0;
				int phase1LostFriend = phase1.AircraftLostStage2Friend;
				int phase2LostFriend = phase2Stage2Enabled ? phase2.AircraftLostStage2Friend : 0;

				int jetTotalFriend = phaseJetStage2Enabled ? phaseJet.AircraftTotalStage2Friend : 0;
				int phase1TotalFriend = phase1.AircraftTotalStage2Friend;
				int phase2TotalFriend = phase2Stage2Enabled ? phase2.AircraftTotalStage2Friend : 0;

				int jetTouchFriend = phaseJetStage2Enabled ? phaseJet.TouchAircraftFriend : -1;
				int phase1TouchFriend = phase1.TouchAircraftFriend;
				int phase2TouchFriend = phase2Stage2Enabled ? phase2.TouchAircraftFriend : -1;

				if ( needAppendInfo ) {
					var text = new List<string>();

					if ( phaseJetStage2Enabled )
						text.Add( "-" + jetLostFriend );

					text.Add( "-" + phase1LostFriend );

					if ( phase2Stage2Enabled )
						text.Add( "-" + phase2LostFriend );

					AirStage2Friend.Text = string.Join( ",", text );

				} else {
					AirStage2Friend.Text = string.Format( "-{0}/{1}", phase1LostFriend, phase1TotalFriend );
				}

				if ( needAppendInfo ) {

					var sb = new StringBuilder();

					if ( phaseJetStage2Enabled )
						sb.AppendFormat( "Jet: -{0}/{1}\r\n", jetLostFriend, jetTotalFriend );

					sb.AppendFormat( "Stage 1: -{0}/{1}\r\n", phase1LostFriend, phase1TotalFriend );

					if ( phase2Stage2Enabled )
						sb.AppendFormat( "Stage 2: -{0}/{1}\r\n", phase2LostFriend, phase2TotalFriend );

					ToolTipInfo.SetToolTip( AirStage2Friend, sb.ToString() );
				} else {
					ToolTipInfo.SetToolTip( AirStage2Friend, null );
				}

				// lost flag
				if ( ( jetTotalFriend > 0 && jetLostFriend == jetTotalFriend ) ||
					( phase1TotalFriend > 0 && phase1LostFriend == phase1TotalFriend ) ||
					( phase2TotalFriend > 0 && phase2LostFriend == phase2TotalFriend ) ) {
					AirStage2Friend.ForeColor = Utility.Configuration.Config.UI.Color_Red;
				} else {
					AirStage2Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				}


				// enemies
				int jetLostEnemy = phaseJetStage2Enabled ? phaseJet.AircraftLostStage2Enemy : 0;
				int phase1LostEnemy = phase1.AircraftLostStage2Enemy;
				int phase2LostEnemy = phase2Stage2Enabled ? phase2.AircraftLostStage2Enemy : 0;

				int jetTotalEnemy = phaseJetStage2Enabled ? phaseJet.AircraftTotalStage2Enemy : 0;
				int phase1TotalEnemy = phase1.AircraftTotalStage2Enemy;
				int phase2TotalEnemy = phase2Stage2Enabled ? phase2.AircraftTotalStage2Enemy : 0;

				int jetTouchEnemy = phaseJetStage2Enabled ? phaseJet.TouchAircraftEnemy : -1;
				int phase1TouchEnemy = phase1.TouchAircraftEnemy;
				int phase2TouchEnemy = phase2Stage2Enabled ? phase2.TouchAircraftEnemy : -1;

				if ( needAppendInfo ) {
					var text = new List<string>();

					if ( phaseJetStage2Enabled )
						text.Add( "-" + jetLostEnemy );

					text.Add( "-" + phase1LostEnemy );

					if ( phase2Stage2Enabled )
						text.Add( "-" + phase2LostEnemy );

					AirStage2Enemy.Text = string.Join( ",", text );

				} else {
					AirStage2Enemy.Text = string.Format( "-{0}/{1}", phase1LostEnemy, phase1TotalEnemy );
				}

				if ( needAppendInfo ) {

					var sb = new StringBuilder();

					if ( phaseJetStage2Enabled )
						sb.AppendFormat( "Jet: -{0}/{1}\r\n", jetLostEnemy, jetTotalEnemy );

					sb.AppendFormat( "Stage 1: -{0}/{1}\r\n", phase1LostEnemy, phase1TotalEnemy );

					if ( phase2Stage2Enabled )
						sb.AppendFormat( "Stage 2: -{0}/{1}\r\n", phase2LostEnemy, phase2TotalEnemy );

					ToolTipInfo.SetToolTip( AirStage2Enemy, sb.ToString() );
				} else {
					ToolTipInfo.SetToolTip( AirStage2Enemy, null );
				}

				// lost flag
				if ( ( jetTotalEnemy > 0 && jetLostEnemy == jetTotalEnemy ) ||
					( phase1TotalEnemy > 0 && phase1LostEnemy == phase1TotalEnemy ) ||
					( phase2TotalEnemy > 0 && phase2LostEnemy == phase2TotalEnemy ) ) {
					AirStage2Enemy.ForeColor = Utility.Configuration.Config.UI.Color_Red;
				} else {
					AirStage2Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				}


				// 対空カットイン
				{
					int jetAACutInKind = phaseJetStage2Enabled && phaseJet.IsAACutinAvailable ? phaseJet.AACutInKind : -1;
					int phase1AACutInKind = phase1.IsAACutinAvailable ? phase1.AACutInKind : -1;
					int phase2AACutInKind = phase2Stage2Enabled && phase2.IsAACutinAvailable ? phase2.AACutInKind : -1;

					int jetAACutInIndex = jetAACutInKind > 0 ? phaseJet.AACutInIndex : -1;
					int phase1AACutInIndex = phase1AACutInKind > 0 ? phase1.AACutInIndex : -1;
					int phase2AACutInIndex = phase2AACutInKind > 0 ? phase2.AACutInIndex : -1;

					if ( jetAACutInKind > 0 || phase1AACutInKind > 0 || phase2AACutInKind > 0 ) {

						var text = new List<string>();

						if ( jetAACutInKind > 0 )
							text.Add( ( jetAACutInIndex + 1 ).ToString() );
						else if ( phaseJetStage2Enabled )
							text.Add( "-" );

						if ( phase1AACutInKind > 0 )
							text.Add( ( phase1AACutInIndex + 1 ).ToString() );
						else
							text.Add( "-" );

						if ( phase2AACutInKind > 0 )
							text.Add( ( phase2AACutInIndex + 1 ).ToString() );
						else if ( phase2Stage2Enabled )
							text.Add( "-" );

						AACutin.Text = "#" + string.Join( "/", text );
						AACutin.ImageAlign = ContentAlignment.MiddleLeft;
						AACutin.ImageIndex = (int)ResourceManager.EquipmentContent.HighAngleGun;


						var sb = new StringBuilder();
						sb.AppendLine( "AACI" );

						if ( phaseJetStage2Enabled ) {
							sb.Append( "Jet: " );

							if ( jetAACutInKind > 0 ) {
								sb.AppendLine( phaseJet.AACutInShip.NameWithLevel );
								sb.AppendFormat( "AACI: {0} ({1})\r\n", jetAACutInKind, Constants.GetAACutinKind( jetAACutInKind ) );
							} else {
								sb.AppendLine( "(did not activate)" );
							}
						}

						if ( needAppendInfo )
							sb.Append( "Stage 1: " );
						if ( phase1AACutInKind > 0 ) {
							sb.AppendLine( phase1.AACutInShip.NameWithLevel );
							sb.AppendFormat( "AACI: {0} ({1})\r\n", phase1AACutInKind, Constants.GetAACutinKind( phase1AACutInKind ) );
						} else {
							sb.AppendLine( "(did not activate)" );
						}

						if ( phase2Stage2Enabled ) {
							sb.Append( "Stage 2: " );

							if ( phase2AACutInKind > 0 ) {
								sb.AppendLine( phase2.AACutInShip.NameWithLevel );
								sb.AppendFormat( "AACI: {0} ({1})\r\n", phase2AACutInKind, Constants.GetAACutinKind( phase2AACutInKind ) );
							} else {
								sb.AppendLine( "(did not activate)" );
							}
						}

						ToolTipInfo.SetToolTip( AACutin, sb.ToString() );

					} else {
						AACutin.Text = GeneralRes.AAPower;
						AACutin.ImageAlign = ContentAlignment.MiddleCenter;
						AACutin.ImageIndex = -1;
						ToolTipInfo.SetToolTip( AACutin, null );
					}
				}

			} else {	// 艦対空戦闘発生せず
				AirStage2Friend.Text = "-";
				AirStage2Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				ToolTipInfo.SetToolTip( AirStage2Friend, null );
				AirStage2Enemy.Text = "-";
				AirStage2Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
				ToolTipInfo.SetToolTip( AirStage2Enemy, null );
				AACutin.Text = GeneralRes.AAPower;
				AACutin.ImageAlign = ContentAlignment.MiddleCenter;
				AACutin.ImageIndex = -1;
				ToolTipInfo.SetToolTip( AACutin, null );
			}

			AirStage2Friend.ImageAlign = ContentAlignment.MiddleCenter;
			AirStage2Friend.ImageIndex = -1;
			AirStage2Enemy.ImageAlign = ContentAlignment.MiddleCenter;
			AirStage2Enemy.ImageIndex = -1;

		}


		/// <summary>
		/// 航空戦情報をクリアします。
		/// </summary>
		private void ClearAerialWarfare() {
			AirSuperiority.Text = "-";
			ToolTipInfo.SetToolTip( AirSuperiority, null );

			AirStage1Friend.Text = "-";
			AirStage1Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
			AirStage1Friend.ImageAlign = ContentAlignment.MiddleCenter;
			AirStage1Friend.ImageIndex = -1;
			ToolTipInfo.SetToolTip( AirStage1Friend, null );

			AirStage1Enemy.Text = "-";
			AirStage1Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
			AirStage1Enemy.ImageAlign = ContentAlignment.MiddleCenter;
			AirStage1Enemy.ImageIndex = -1;
			ToolTipInfo.SetToolTip( AirStage1Enemy, null );

			AirStage2Friend.Text = "-";
			AirStage2Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
			AirStage2Friend.ImageAlign = ContentAlignment.MiddleCenter;
			AirStage2Friend.ImageIndex = -1;
			ToolTipInfo.SetToolTip( AirStage2Friend, null );

			AirStage2Enemy.Text = "-";
			AirStage2Enemy.ImageAlign = ContentAlignment.MiddleCenter;
			AirStage2Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
			AirStage2Enemy.ImageIndex = -1;
			ToolTipInfo.SetToolTip( AirStage2Enemy, null );

			AACutin.Text = "-";
			AACutin.ImageAlign = ContentAlignment.MiddleCenter;
			AACutin.ImageIndex = -1;
			ToolTipInfo.SetToolTip( AACutin, null );
		}


		/// <summary>
		/// 両軍のHPゲージを設定します。
		/// </summary>
		private void SetHPBar( BattleData bd ) {

			KCDatabase db = KCDatabase.Instance;
			bool isPractice = ( bd.BattleType & BattleData.BattleTypeFlag.Practice ) != 0;
			bool isCombined = ( bd.BattleType & BattleData.BattleTypeFlag.Combined ) != 0;
			bool isEnemyCombined = ( bd.BattleType & BattleData.BattleTypeFlag.EnemyCombined ) != 0;
			bool isBaseAirRaid = ( bd.BattleType & BattleData.BattleTypeFlag.BaseAirRaid ) != 0;

			var initialHPs = bd.Initial.InitialHPs;
			var maxHPs = bd.Initial.MaxHPs;
			var resultHPs = bd.ResultHPs;
			var attackDamages = bd.AttackDamages;


			foreach ( var bar in HPBars )
				bar.SuspendUpdate();

			for ( int i = 0; i < 24; i++ ) {

				if ( initialHPs[i] != -1 ) {
					HPBars[i].Value = resultHPs[i];
					HPBars[i].PrevValue = initialHPs[i];
					HPBars[i].MaximumValue = maxHPs[i];
					HPBars[i].BackColor = Utility.Configuration.Config.UI.BackColor;
					HPBars[i].Visible = true;
				} else {
					HPBars[i].Visible = false;
				}
			}


			// friend main
			for ( int i = 0; i < 6; i++ ) {
				if ( initialHPs[i] != -1 ) {
					string name;
					bool isEscaped;
					bool isLandBase;

					var bar = HPBars[i];

					if ( isBaseAirRaid ) {
						name = string.Format( "Base {0}", i + 1 );
						isEscaped = false;
						isLandBase = true;
						bar.Text = "LB";		//note: Land Base (Landing Boat もあるらしいが考えつかなかったので)

					} else {
						ShipData ship = bd.Initial.FriendFleet.MembersInstance[i];
						name = string.Format( "{0} Lv. {1}", ship.MasterShip.NameWithClass, ship.Level );
						isEscaped = bd.Initial.FriendFleet.EscapedShipList.Contains( ship.MasterID );
						isLandBase = ship.MasterShip.IsLandBase;
						bar.Text = Constants.GetShipClassClassification( ship.MasterShip.ShipType );
					}

					ToolTipInfo.SetToolTip( bar, string.Format
						( "{0}\r\nHP: ({1} → {2})/{3} ({4}) [{5}]\r\n" + GeneralRes.DamageDone + ": {6}\r\n\r\n{7}",
						name,
						Math.Max( bar.PrevValue, 0 ),
						Math.Max( bar.Value, 0 ),
						bar.MaximumValue,
						bar.Value - bar.PrevValue,
						Constants.GetDamageState( (double)bar.Value / bar.MaximumValue, isPractice, isLandBase, isEscaped ),
						attackDamages[i],
						bd.GetBattleDetail( i )
						) );

					if ( isEscaped ) bar.BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsEscaped;
					else bar.BackColor = Utility.Configuration.Config.UI.BackColor;
				}
			}


			// enemy main
			for ( int i = 0; i < 6; i++ ) {
				if ( initialHPs[i + 6] != -1 ) {
					ShipDataMaster ship = bd.Initial.EnemyMembersInstance[i];

					var bar = HPBars[i + 6];
					bar.Text = Constants.GetShipClassClassification( ship.ShipType );

					ToolTipInfo.SetToolTip( bar,
						string.Format( "{0} Lv. {1}\r\nHP: ({2} → {3})/{4} ({5}) [{6}]\r\n\r\n{7}",
							ship.NameWithClass,
							bd.Initial.EnemyLevels[i],
							Math.Max( bar.PrevValue, 0 ),
							Math.Max( bar.Value, 0 ),
							bar.MaximumValue,
							bar.Value - bar.PrevValue,
							Constants.GetDamageState( (double)bar.Value / bar.MaximumValue, isPractice, ship.IsLandBase ),
							bd.GetBattleDetail( i + 6 )
							)
						);
				}
			}


			// friend escort
			if ( isCombined ) {
				FleetFriendEscort.Visible = true;

				for ( int i = 0; i < 6; i++ ) {
					if ( initialHPs[i + 12] != -1 ) {
						ShipData ship = bd.Initial.FriendFleetEscort.MembersInstance[i];
						bool isEscaped = bd.Initial.FriendFleetEscort.EscapedShipList.Contains( ship.MasterID );

						var bar = HPBars[i + 12];
						bar.Text = Constants.GetShipClassClassification( ship.MasterShip.ShipType );

						ToolTipInfo.SetToolTip( bar, string.Format(
							"{0} Lv. {1}\r\nHP: ({2} → {3})/{4} ({5}) [{6}]\r\n" + GeneralRes.DamageDone + ": {7}\r\n\r\n{8}",
							ship.MasterShip.NameWithClass,
							ship.Level,
							Math.Max( bar.PrevValue, 0 ),
							Math.Max( bar.Value, 0 ),
							bar.MaximumValue,
							bar.Value - bar.PrevValue,
							Constants.GetDamageState( (double)bar.Value / bar.MaximumValue, isPractice, ship.MasterShip.IsLandBase, isEscaped ),
							attackDamages[i + 12],
							bd.GetBattleDetail( i + 12 )
							) );

						if ( isEscaped ) bar.BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsEscaped;
						else bar.BackColor = Utility.Configuration.Config.UI.BackColor;
					}
				}

			} else {
				FleetFriendEscort.Visible = false;
			}


			// enemy escort
			if ( isEnemyCombined ) {
				FleetEnemyEscort.Visible = true;

				for ( int i = 0; i < 6; i++ ) {
					if ( initialHPs[i + 18] != -1 ) {
						ShipDataMaster ship = bd.Initial.EnemyMembersEscortInstance[i];

						var bar = HPBars[i + 18];
						bar.Text = Constants.GetShipClassClassification( ship.ShipType );

						ToolTipInfo.SetToolTip( bar,
							string.Format( "{0} Lv. {1}\r\nHP: ({2} → {3})/{4} ({5}) [{6}]\r\n\r\n{7}",
								ship.NameWithClass,
								bd.Initial.EnemyLevelsEscort[i],
								Math.Max( bar.PrevValue, 0 ),
								Math.Max( bar.Value, 0 ),
								bar.MaximumValue,
								bar.Value - bar.PrevValue,
								Constants.GetDamageState( (double)bar.Value / bar.MaximumValue, isPractice, ship.IsLandBase ),
								bd.GetBattleDetail( i + 18 )
								)
							);
					}
				}

			} else {
				FleetEnemyEscort.Visible = false;
			}


			if ( isCombined && isEnemyCombined ) {
				foreach ( var bar in HPBars ) {
					bar.Size = SmallBarSize;
					bar.Text = null;
				}
			} else {
				bool showShipType = Utility.Configuration.Config.FormBattle.ShowShipTypeInHPBar;

				foreach ( var bar in HPBars ) {
					bar.Size = DefaultBarSize;

					if ( !showShipType )
						bar.Text = "HP:";
				}
			}


			{	// support
				var battleday = bd as BattleDay;
				if ( battleday != null && battleday.Support != null && battleday.Support.IsAvailable ) {

					switch ( battleday.Support.SupportFlag ) {
						case 1:
							FleetFriend.ImageIndex = (int)ResourceManager.EquipmentContent.CarrierBasedTorpedo;
							break;
						case 2:
							FleetFriend.ImageIndex = (int)ResourceManager.EquipmentContent.MainGunL;
							break;
						case 3:
							FleetFriend.ImageIndex = (int)ResourceManager.EquipmentContent.Torpedo;
							break;
						default:
							FleetFriend.ImageIndex = (int)ResourceManager.EquipmentContent.Unknown;
							break;
					}

					FleetFriend.ImageAlign = ContentAlignment.MiddleLeft;
					ToolTipInfo.SetToolTip( FleetFriend, "支援攻撃\r\n" + battleday.Support.GetBattleDetail() );

					if ( isCombined && isEnemyCombined )
						FleetFriend.Text = "自軍";
					else
						FleetFriend.Text = "自軍艦隊";

				} else {

					FleetFriend.ImageIndex = -1;
					FleetFriend.ImageAlign = ContentAlignment.MiddleCenter;
					FleetFriend.Text = "自軍艦隊";
					ToolTipInfo.SetToolTip( FleetFriend, null );

				}
			}


			if ( bd.Initial.IsBossDamaged )
			{
				HPBars[6].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsBossDamaged;
				HPBars[6].RepaintHPtext();
			}

			if ( !isBaseAirRaid ) {
				foreach (int i in bd.MVPShipIndexes)
				{
					HPBars[i].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsMVP;
					HPBars[i].RepaintHPtext();
				}
				foreach (int i in bd.MVPShipCombinedIndexes)
				{
					HPBars[12 + i].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsMVP;
					HPBars[12 + i].RepaintHPtext();
				}
			}

			foreach ( var bar in HPBars )
				bar.ResumeUpdate();
		}



		/// <summary>
		/// 損害率と戦績予測を設定します。
		/// </summary>
		private void SetDamageRate( BattleManager bm ) {

			double friendrate, enemyrate;
			int rank = bm.PredictWinRank( out friendrate, out enemyrate );

			DamageFriend.Text = friendrate.ToString( "p1" );
			DamageEnemy.Text = enemyrate.ToString( "p1" );

			if ( bm.IsBaseAirRaid ) {
				int kind = bm.Compass.AirRaidDamageKind;
				WinRank.Text = Constants.GetAirRaidDamageShort( kind );
				WinRank.ForeColor = ( 1 <= kind && kind <= 3 ) ? WinRankColor_Lose : WinRankColor_Win;
			} else {
				WinRank.Text = Constants.GetWinRank( rank );
				WinRank.ForeColor = rank >= 4 ? WinRankColor_Win : WinRankColor_Lose;
			}

			WinRank.MinimumSize = Utility.Configuration.Config.UI.IsLayoutFixed ? new Size( DefaultBarSize.Width, 0 ) : new Size( HPBars[0].Width, 0 );
		}


		/// <summary>
		/// 夜戦における各種表示を設定します。
		/// </summary>
		/// <param name="hp">戦闘開始前のHP。</param>
		/// <param name="isCombined">連合艦隊かどうか。</param>
		/// <param name="bd">戦闘データ。</param>
		private void SetNightBattleEvent( PhaseNightBattle pd ) {

			FleetData fleet = pd.FriendFleet;

			//味方探照灯判定
			{
				int index = pd.SearchlightIndexFriend;

				if ( index != -1 ) {
					ShipData ship = fleet.MembersInstance[index];

					AirStage1Friend.Text = "#" + ( index + 1 );
					AirStage1Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
					AirStage1Friend.ImageAlign = ContentAlignment.MiddleLeft;
					AirStage1Friend.ImageIndex = (int)ResourceManager.EquipmentContent.Searchlight;
					ToolTipInfo.SetToolTip( AirStage1Friend, GeneralRes.SearchlightUsed + ": " + ship.NameWithLevel );
				} else {
					ToolTipInfo.SetToolTip( AirStage1Friend, null );
				}
			}

			//敵探照灯判定
			{
				int index = pd.SearchlightIndexEnemy;
				if ( index != -1 ) {
					AirStage1Enemy.Text = "#" + ( index + 1 );
					AirStage1Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
					AirStage1Enemy.ImageAlign = ContentAlignment.MiddleLeft;
					AirStage1Enemy.ImageIndex = (int)ResourceManager.EquipmentContent.Searchlight;
					ToolTipInfo.SetToolTip( AirStage1Enemy, GeneralRes.SearchlightUsed + ": " + pd.SearchlightEnemyInstance.NameWithClass );
				} else {
					ToolTipInfo.SetToolTip( AirStage1Enemy, null );
				}
			}


			//夜間触接判定
			if ( pd.TouchAircraftFriend != -1 ) {
				SearchingFriend.Text = GeneralRes.NightContact;
				SearchingFriend.ImageIndex = (int)ResourceManager.EquipmentContent.Seaplane;
				SearchingFriend.ImageAlign = ContentAlignment.MiddleLeft;
				ToolTipInfo.SetToolTip( SearchingFriend, GeneralRes.NightContacting + ": " + KCDatabase.Instance.MasterEquipments[pd.TouchAircraftFriend].Name );
			} else {
				ToolTipInfo.SetToolTip( SearchingFriend, null );
			}

			if ( pd.TouchAircraftEnemy != -1 ) {
				SearchingEnemy.Text = GeneralRes.NightContact;
				SearchingEnemy.ImageIndex = (int)ResourceManager.EquipmentContent.Seaplane;
				SearchingFriend.ImageAlign = ContentAlignment.MiddleLeft;
				ToolTipInfo.SetToolTip( SearchingEnemy, GeneralRes.NightContacting + ": " + KCDatabase.Instance.MasterEquipments[pd.TouchAircraftEnemy].Name );
			} else {
				ToolTipInfo.SetToolTip( SearchingEnemy, null );
			}

			//照明弾投射判定
			{
				int index = pd.FlareIndexFriend;

				if ( index != -1 ) {
					AirStage2Friend.Text = "#" + ( index + 1 );
					AirStage2Friend.ForeColor = Utility.Configuration.Config.UI.ForeColor;
					AirStage2Friend.ImageAlign = ContentAlignment.MiddleLeft;
					AirStage2Friend.ImageIndex = (int)ResourceManager.EquipmentContent.Flare;
					ToolTipInfo.SetToolTip( AirStage2Friend, GeneralRes.StarShellUsed + ": " + fleet.MembersInstance[index].NameWithLevel );

				} else {
					ToolTipInfo.SetToolTip( AirStage2Friend, null );
				}
			}

			{
				int index = pd.FlareIndexEnemy;

				if ( index != -1 ) {
					AirStage2Enemy.Text = "#" + ( index + 1 );
					AirStage2Enemy.ForeColor = Utility.Configuration.Config.UI.ForeColor;
					AirStage2Enemy.ImageAlign = ContentAlignment.MiddleLeft;
					AirStage2Enemy.ImageIndex = (int)ResourceManager.EquipmentContent.Flare;
					ToolTipInfo.SetToolTip( AirStage2Enemy, GeneralRes.StarShellUsed + ": " + pd.FlareEnemyInstance.NameWithClass );
				} else {
					ToolTipInfo.SetToolTip( AirStage2Enemy, null );
				}
			}
		}


		/// <summary>
		/// 戦闘終了後に、MVP艦の表示を更新します。
		/// </summary>
		/// <param name="bm">戦闘データ。</param>
		private void SetMVPShip( BattleManager bm ) {

			bool isCombined = bm.IsCombinedBattle;

			var bd = bm.StartsFromDayBattle ? (BattleData)bm.BattleDay : (BattleData)bm.BattleNight;
			var br = bm.Result;

			var friend = bd.Initial.FriendFleet;
			var escort = !isCombined ? null : bd.Initial.FriendFleetEscort;


			/*// DEBUG
			{
				BattleData lastbattle = bm.StartsFromDayBattle ? (BattleData)bm.BattleNight ?? bm.BattleDay : (BattleData)bm.BattleDay ?? bm.BattleNight;
				if ( lastbattle.MVPShipIndexes.Count() > 1 || !lastbattle.MVPShipIndexes.Contains( br.MVPIndex - 1 ) ) {
					Utility.Logger.Add( 1, "MVP is wrong : [" + string.Join( ",", lastbattle.MVPShipIndexes ) + "] => " + ( br.MVPIndex - 1 ) );
				}
				if ( isCombined && ( lastbattle.MVPShipCombinedIndexes.Count() > 1 || !lastbattle.MVPShipCombinedIndexes.Contains( br.MVPIndexCombined - 1 ) ) ) {
					Utility.Logger.Add( 1, "MVP is wrong (escort) : [" + string.Join( ",", lastbattle.MVPShipCombinedIndexes ) + "] => " + ( br.MVPIndexCombined - 1 ) );
				}
			}
			//*/


			for ( int i = 0; i < 6; i++ ) {
				if (friend.EscapedShipList.Contains(friend.Members[i]))
				{
					HPBars[i].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsEscaped;
					HPBars[i].RepaintHPtext();

				}
				else if (br.MVPIndex == i + 1)
				{
					HPBars[i].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsMVP;
					HPBars[i].RepaintHPtext();

				}
				else
				{
					HPBars[i].BackColor = Utility.Configuration.Config.UI.BackColor;
					HPBars[i].RepaintHPtext();
				}

				if ( escort != null ) {
					if (escort.EscapedShipList.Contains(escort.Members[i]))
					{
						HPBars[i + 12].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsEscaped;
					}
					else if (br.MVPIndexCombined == i + 1)
					{
						HPBars[i + 12].BackColor = Utility.Configuration.Config.UI.Battle_ColorHPBarsMVP;
					}
					else
					{
						HPBars[i + 12].BackColor = Utility.Configuration.Config.UI.BackColor;
					}
					HPBars[i + 12].RepaintHPtext();
				}
			}

			/*// debug
			if ( WinRank.Text.First().ToString() != bm.Result.Rank ) {
				Utility.Logger.Add( 1, string.Format( "戦闘評価予測が誤っています。(予測: {0}, 実際: {1})", WinRank.Text.First().ToString(), bm.Result.Rank ) );
			}
			//*/

		}


		private void RightClickMenu_Opening( object sender, CancelEventArgs e ) {

			var bm = KCDatabase.Instance.Battle;

			if ( bm == null || bm.BattleMode == BattleManager.BattleModes.Undefined )
				e.Cancel = true;

			RightClickMenu_ShowBattleResult.Enabled = !BaseLayoutPanel.Visible;
		}

		private void RightClickMenu_ShowBattleDetail_Click( object sender, EventArgs e ) {
			var bm = KCDatabase.Instance.Battle;

			if ( bm == null || bm.BattleMode == BattleManager.BattleModes.Undefined )
				return;

			var dialog = new Dialog.DialogBattleDetail();

			dialog.BattleDetailText = BattleDetailDescriptor.GetBattleDetail( bm );
			dialog.Location = RightClickMenu.Location;
			dialog.Show( this );

		}

		private void RightClickMenu_ShowBattleResult_Click( object sender, EventArgs e ) {
			BaseLayoutPanel.Visible = true;
		}




		void ConfigurationChanged() {

			var config = Utility.Configuration.Config;

			MainFont = TableTop.Font = TableBottom.Font = Font = config.UI.MainFont;
			SubFont = config.UI.SubFont;

			BaseLayoutPanel.AutoScroll = config.FormBattle.IsScrollable;


			bool fixSize = config.UI.IsLayoutFixed;
			bool showHPBar = config.FormBattle.ShowHPBar;

			TableBottom.SuspendLayout();
			if ( fixSize ) {
				ControlHelper.SetTableColumnStyles( TableBottom, new ColumnStyle( SizeType.AutoSize ) );
				ControlHelper.SetTableRowStyle( TableBottom, 0, new RowStyle( SizeType.Absolute, 21 ) );
				for ( int i = 1; i <= 6; i++ )
					ControlHelper.SetTableRowStyle( TableBottom, i, new RowStyle( SizeType.Absolute, showHPBar ? 21 : 16 ) );
				ControlHelper.SetTableRowStyle( TableBottom, 7, new RowStyle( SizeType.Absolute, 21 ) );
			} else {
				ControlHelper.SetTableColumnStyles( TableBottom, new ColumnStyle( SizeType.AutoSize ) );
				ControlHelper.SetTableRowStyles( TableBottom, new RowStyle( SizeType.AutoSize ) );
			}
			if ( HPBars != null ) {
				foreach ( var b in HPBars ) {
					b.MainFont = MainFont;
					b.SubFont = SubFont;
					b.AutoSize = !fixSize;
					if ( !b.AutoSize ) {
						b.Size = ( HPBars[12].Visible && HPBars[18].Visible ) ? SmallBarSize : DefaultBarSize;
					}
					b.HPBar.ColorMorphing = config.UI.BarColorMorphing;
					b.HPBar.SetBarColorScheme( config.UI.BarColorScheme.Select( col => col.ColorData ).ToArray() );
					b.ShowHPBar = showHPBar;
				}
			}
			FleetFriend.MaximumSize =
			FleetFriendEscort.MaximumSize =
			FleetEnemy.MaximumSize =
			FleetEnemyEscort.MaximumSize =
			DamageFriend.MaximumSize =
			DamageEnemy.MaximumSize =
				fixSize ? DefaultBarSize : Size.Empty;

			WinRank.MinimumSize = fixSize ? new Size( 80, 0 ) : new Size( HPBars[0].Width, 0 );

			TableBottom.ResumeLayout();

			TableTop.SuspendLayout();
			if ( fixSize ) {
				ControlHelper.SetTableColumnStyles( TableTop, new ColumnStyle( SizeType.Absolute, 21 * 4 ) );
				ControlHelper.SetTableRowStyles( TableTop, new RowStyle( SizeType.Absolute, 21 ) );
				TableTop.Width = TableTop.GetPreferredSize( BaseLayoutPanel.Size ).Width;
			} else {
				ControlHelper.SetTableColumnStyles( TableTop, new ColumnStyle( SizeType.Percent, 100 ) );
				ControlHelper.SetTableRowStyles( TableTop, new RowStyle( SizeType.AutoSize ) );
				TableTop.Width = TableBottom.ClientSize.Width;
			}
			TableTop.Height = TableTop.GetPreferredSize( BaseLayoutPanel.Size ).Height;
			TableTop.ResumeLayout();

		}



		private void TableTop_CellPaint( object sender, TableLayoutCellPaintEventArgs e ) {
			if ( e.Row == 1 || e.Row == 3 )
				e.Graphics.DrawLine(Utility.Configuration.Config.UI.SubBackColorPen, e.CellBounds.X, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1 );
		}

		private void TableBottom_CellPaint( object sender, TableLayoutCellPaintEventArgs e ) {
			if ( e.Row == 7 )
				e.Graphics.DrawLine(Utility.Configuration.Config.UI.SubBackColorPen, e.CellBounds.X, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1 );
		}


		protected override string GetPersistString() {
			return "Battle";
		}


	}

}
