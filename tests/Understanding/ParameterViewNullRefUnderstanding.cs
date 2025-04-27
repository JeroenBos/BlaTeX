using System.Linq;
using Microsoft.AspNetCore.Components;
using Xunit;
using JBSnorro;

namespace BlaTeX.Tests.Understanding
{
	public class ParameterViewNullRefUnderstanding
	{
		[Fact]
		public void AParameterInAParameterViewCanBeNull()
		{
			// There's annoying null deref warnings in the setter on a ParameterView. Let's see if these warnings actually can be ignored:
			var dict = ParameterView.Empty.ToDictionary().ToDictionary(); // autotyped as Dictionary<string, object>, not as Dictionary<string, object?> which I expect
			dict["someName"] = null;

			Assert.Null(dict["someName"]);
		}
	}
}
