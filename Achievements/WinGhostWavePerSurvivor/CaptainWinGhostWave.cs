using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_CaptainWinGhostWave", "Skins.Captain.BulwarksHaunt_Alt", null, null)]
	public class CaptainWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("CaptainBody");
		}
	}
}
