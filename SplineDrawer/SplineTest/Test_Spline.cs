using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Collections.Generic;
using SplineLib;
using RuntimeFunctionParser;

namespace SplineTest
{
	[TestClass]
	public class Test_Spline
	{
		/// <summary>
		/// Test the Spline calculation with an online example
		/// </summary>
		/// <seealso cref="http://www.tm-mathe.de/Themen/html/funnatsplines.html"/>
		[TestMethod]
		public void TestSplineWithOnlineExample()
		{
			List<Point> input = new List<Point>();
			input.Add(new Point(1, 8));
			input.Add(new Point(7, 10));
			input.Add(new Point(12, 7));
			input.Add(new Point(15, 8));
			input.Add(new Point(19, 7));

			Spline s = new Spline(input);

			List<string> expected = new List<string>();
			expected.Add("8+0.7097*x+0.01045*x^3");
			expected.Add("10-0.4194*x-0.1882*x^2+0.03041*x^3");
			expected.Add("7-0.02026*x+0.2680*x^2-0.05005*x^3");
			expected.Add("8+0.2365*x-0.1824*x^2+0.01520*x^3");

			for(int i = 0; i < s.PartialPolynomials.Count; i++)
			{
				Assert.AreEqual(expected[i], s.PartialPolynomials[i].OriginalFunction, 
					"Online example failed with "+s.PartialPolynomials[i].OriginalFunction);
			}
		}
	}
}
