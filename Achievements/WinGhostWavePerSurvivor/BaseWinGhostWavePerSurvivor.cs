using RoR2;
using RoR2.Achievements;

namespace BulwarksHaunt.Achievements
{
	public class BaseWinGhostWavePerSurvivor : BaseAchievement
	{
		public override void OnBodyRequirementMet()
		{
			base.OnBodyRequirementMet();
			Run.onClientGameOverGlobal += OnClientGameOverGlobal;
		}

		public override void OnBodyRequirementBroken()
		{
			Run.onClientGameOverGlobal -= OnClientGameOverGlobal;
			base.OnBodyRequirementBroken();
		}

		private void OnClientGameOverGlobal(Run run, RunReport runReport)
		{
			if (!runReport.gameEnding) return;
			if (runReport.gameEnding == BulwarksHauntContent.GameEndings.BulwarksHaunt_HauntedEnding)
			{
				Grant();
			}
		}
	}
}
