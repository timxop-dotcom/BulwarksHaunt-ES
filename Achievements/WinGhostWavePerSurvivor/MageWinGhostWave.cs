using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_MageWinGhostWave", "Skins.Mage.BulwarksHaunt_Alt", null, null)]
	public class MageWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("MageBody");
		}
	}
}
