using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_ToolbotWinGhostWave", "Skins.Toolbot.BulwarksHaunt_Alt", null, null)]
	public class ToolbotWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("ToolbotBody");
		}
	}
}
