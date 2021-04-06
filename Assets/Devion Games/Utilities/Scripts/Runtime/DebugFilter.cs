using UnityEngine;
using System.Collections;

namespace DevionGames
{
	public class DebugFilter
	{
		private int level;
		private string prefix;

		public DebugFilter (FilterLevel level)
		{
			this.level = (int)level;
		}

		public DebugFilter (FilterLevel level, string prefix)
		{
			this.level = (int)level;
			this.prefix = prefix;
		}

		public void LogInfo (object msg)
		{
			if (this.level >= 2) {
				Debug.Log (prefix + " " + msg);
			}
		}

		public void LogWarning (object msg)
		{
			if (this.level >= 1) {
				Debug.LogWarning (prefix + " " + msg);
			}
		}

		public void LogError (object msg)
		{
			if (this.level >= 0) {
				Debug.LogError (prefix + " " + msg);
			}
		}

		public enum FilterLevel
		{
			Error = 0,
			Warn = 1,
			Info = 2,
		}
	}
}