using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_EngiWinGhostWave", "Skins.Engi.BulwarksHaunt_Alt", null, null)]
	public class EngiWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("EngiBody");
		}
	}
}
