using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_TreebotWinGhostWave", "Skins.Treebot.BulwarksHaunt_Alt", null, null)]
	public class TreebotWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("TreebotBody");
		}
	}
}
