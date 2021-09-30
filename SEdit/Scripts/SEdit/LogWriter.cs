using System;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Log.CombatLog_ThreadSystem.LogThreads.Common;

namespace OwlcatModification.Modifications.SEdit
{
		public class LogWriter : LogThreadBase, ILogMessageUIHandler, IGlobalSubscriber, ISubscriber
		{
			public void HandleLogMessage(string text){
				base.AddMessage(new CombatLogMessage(text, LogThreadBase.Colors.WarningLogColor, PrefixIcon.None, null, true));
			}

		}
}
