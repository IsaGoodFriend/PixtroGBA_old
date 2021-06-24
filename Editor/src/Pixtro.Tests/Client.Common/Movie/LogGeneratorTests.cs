using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixtro.Client.Common;
using Pixtro.Common;
using Pixtro.Emulation.Common;

namespace Pixtro.Tests.Client.Common.Movie
{
	[TestClass]
	public class LogGeneratorTests
	{
		private SimpleController _boolController = null!;
		private SimpleController _axisController = null!;

		[TestInitialize]
		public void Initializer()
		{
			_boolController = new SimpleController
			{
				Definition = new ControllerDefinition { BoolButtons = { "A" } }
			};

			_axisController = new SimpleController
			{
				Definition = new ControllerDefinition().AddXYPair("Stick{0}", AxisPairOrientation.RightAndUp, 0.RangeTo(200), 100)
			};
		}

		[TestMethod]
		public void GenerateLogEntry_ExclamationForUnknownButtons()
		{
			var controller = new SimpleController
			{
				Definition = new ControllerDefinition
				{
					BoolButtons = new List<string> {"Unknown Button"}
				},
				["Unknown Button"] = true
			};

		}

		[TestMethod]
		public void GenerateLogEntry_BoolPressed_GeneratesMnemonic()
		{
			_boolController["A"] = true;
		}

		[TestMethod]
		public void GenerateLogEntry_BoolUnPressed_GeneratesPeriod()
		{
			_boolController["A"] = false;
		}

		[TestMethod]
		public void GenerateLogEntry_Floats()
		{
		}
	}
}