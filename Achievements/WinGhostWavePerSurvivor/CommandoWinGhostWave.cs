using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	[RegisterAchievement("BulwarksHaunt_CommandoWinGhostWave", "Skins.Commando.BulwarksHaunt_Alt", null, null)]
	public class CommandoWinGhostWave : BaseWinGhostWavePerSurvivor
	{
		public override BodyIndex LookUpRequiredBodyIndex()
		{
			return BodyCatalog.FindBodyIndex("CommandoBody");
		}
	}
}
