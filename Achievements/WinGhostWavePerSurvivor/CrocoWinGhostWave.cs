using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_CrocoWinGhostWave", "Skins.Croco.BulwarksHaunt_Alt", null, null)]
	public class CrocoWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("CrocoBody");
		}
	}
}
