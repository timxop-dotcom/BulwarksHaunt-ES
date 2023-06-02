using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_Bandit2WinGhostWave", "Skins.Bandit2.BulwarksHaunt_Alt", null, null)]
	public class Bandit2WinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("Bandit2Body");
		}
	}
}
