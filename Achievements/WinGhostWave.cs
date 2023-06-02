using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_WinGhostWave", "BulwarksHaunt_SwordUnleashed", null, typeof(Server))]
	public class WinGhostWave : BaseAchievement
	{
		public override void OnInstall()
		{
			base.OnInstall();
			SetServerTracked(true);
		}

		public override void OnUninstall()
		{
			SetServerTracked(false);
			base.OnUninstall();
		}

		public class Server : BaseServerAchievement
		{
			public override void OnInstall()
			{
				base.OnInstall();
				GhostWave.BulwarksHauntGhostWaveController.onGhostWaveComplete += BulwarksHauntGhostWaveController_onGhostWaveComplete;
			}

			private void BulwarksHauntGhostWaveController_onGhostWaveComplete()
			{
				Grant();
				Items.Sword.reloadLogbook = true;
			}

			public override void OnUninstall()
			{
				base.OnUninstall();
				GhostWave.BulwarksHauntGhostWaveController.onGhostWaveComplete -= BulwarksHauntGhostWaveController_onGhostWaveComplete;
			}
		}
	}
}
