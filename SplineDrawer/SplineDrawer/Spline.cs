using Math_Collection.LinearAlgebra.Matrices;
using RuntimeFunctionParser;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System;
using Math_Collection.LGS;

namespace SplineDrawer
{
	internal class Spline
	{
		private SortedList<double, double> _sourcePoints { get; set; }

		private List<Function> _partialPolynomials;
		public List<Function> PartialPolynomials
		{
			get { return _partialPolynomials; }
			set { _partialPolynomials = value; }
		}

		/// <summary>
		/// Returns the amount of spline parts
		/// </summary>
		private int _amountOfSplineParts
		{
			get
			{
				return _sourcePoints.Count - 1;
			}
		}

		public Spline()
		{
			_sourcePoints = new SortedList<double, double>();
			PartialPolynomials = new List<Function>();
		}

		public Spline(List<Point> points)
		{
			_sourcePoints = new SortedList<double, double>();
			points.ForEach(p => _sourcePoints.Add(p.X, p.Y));
			PartialPolynomials = CalculateSplineParts();
		}

		/// <summary>
		/// Calculates all polynominal parts of the spline
		/// </summary>
		/// <returns>A List of Functions that represents all polynominals</returns>
		/// <seealso cref="http://www.tm-mathe.de/Themen/html/funnatsplines.html"/>
		private List<Function> CalculateSplineParts()
		{
			if (_sourcePoints != null && _sourcePoints.Count < 3)
			{
				return null;
			}

			// 3.Degree Polynomial := f(x) = a + bx + cx^2 + dx^3 
			// f'(x) = b + 2cx + 3dx^2
			// f''(x) = 2c + 6dx

			Parser parser = new Parser();

			double[] xs = _sourcePoints.Keys.ToArray();
			double[] ys = _sourcePoints.Values.ToArray();

			#region Calculate Help Variable hi and gi

			double[] hi = CalculateH(xs);

			double[] gi = CalcualteG(ys, hi);

			#endregion

			#region Calculate all c's

			Matrix coefficientMatrix = GetCoefficientMatrix(hi);
			Math_Collection.LinearAlgebra.Vectors.Vector coefficientVector = GetCoefficientVector(gi);
			LGS lgs = new LGS(coefficientMatrix, coefficientVector);

			Math_Collection.LinearAlgebra.Vectors.Vector resultFromLGS = lgs.Solve(Math_Collection.Enums.ESolveAlgorithm.eGauß);

			double[] ci = new double[_amountOfSplineParts + 2];
			for (int i = 2; i <= _amountOfSplineParts + 1; i++)
			{
				ci[i] = resultFromLGS[i - 1];
			}

			#endregion

			#region Calculates all a values

			// a's = yi-1
			double[] ai = new double[_amountOfSplineParts + 1];
			for (int i = 1; i <= _amountOfSplineParts; i++)
			{
				ai[i] = ys[i - 1];
			}

			#endregion

			#region Calculate all b's

			// bi = (yi - yi-1) / hi - (hi / 3) * (2*ci + ci+1)

			double[] bi = new double[_amountOfSplineParts + 1];
			for (int i = 1; i <= _amountOfSplineParts; i++)
			{
				bi[i] = ((ys[i] - ys[i - 1]) / hi[i]) - (hi[i] / 3) * (2 * ci[i] + ci[i + 1]);
			}

			#endregion

			#region Calculate all d's

			// di = (ci+1 - ci) / 3 * hi

			double[] di = new double[_amountOfSplineParts + 1];
			for (int i = 1; i <= _amountOfSplineParts; i++)
			{
				di[i] = (ci[i + 1] - ci[i]) / (3 * hi[i]);
			}

			#endregion

			#region Generate all Functions

			Function[] functionParts = new Function[_amountOfSplineParts];
			for (int i = 1; i <= _amountOfSplineParts; i++)
			{
				string f = ai[i] + "+" + bi[i] + "*x+" + ci[i] + "*x^2+" + di[i] + "*x^3";
				functionParts[i - 1] = parser.ParseFunction(f);
			}

			#endregion

			return functionParts.ToList();
		}

		/// <summary>
		/// Calculates the help variable g
		/// gj = 3 * ((yj - yj-1)/hj - (yj-1-yj-2)/hj-1)  
		/// </summary>
		/// <param name="ys"></param>
		/// <param name="hi"></param>
		/// <returns></returns>
		private double[] CalcualteG(double[] ys, double[] hi)
		{
			double[] gi = new double[_amountOfSplineParts + 1];
			gi[0] = 0;
			gi[1] = 0;
			for (int i = 2; i <= _amountOfSplineParts; i++)
				gi[i] = 3 * ((ys[i] - ys[i - 1]) / hi[i] - (ys[i - 1] - ys[i - 2]) / hi[i - 1]);

			return gi;
		}

		/// <summary>
		/// Calculates the help variable hi
		/// hi = xi - xi-1
		/// </summary>
		/// <param name="xValues">all x values from the points</param>
		/// <param name="n"></param>
		/// <returns></returns>
		private double[] CalculateH(double[] xValues)
		{
			double[] hi = new double[_amountOfSplineParts + 1];
			hi[0] = 0;
			for (int i = 1; i <= _amountOfSplineParts; i++)
				hi[i] = xValues[i] - xValues[i - 1];

			return hi;
		}

		private Matrix GetCoefficientMatrix(double[] h)
		{
			Matrix m = new Matrix(new double[_amountOfSplineParts + 1, _amountOfSplineParts + 1]);
			m[0, 0] = 1;
			// Diagnal
			for (int i = 2; i < m.RowCount; i++)
			{
				m[i - 1, i - 1] = 2 * (h[i - 1] + h[i]);
			}
			m[m.RowCount - 1, m.ColumnCount - 1] = 1;

			// Under Diagonal
			for (int k = 2; k < m.ColumnCount - 1; k++)
			{
				m[k - 1, k] = h[k];
			}

			// Abover Diagonal
			for (int o = 2; o < m.RowCount - 1; o++)
			{
				m[o, o - 1] = h[o];
			}

			return m;
		}

		private Math_Collection.LinearAlgebra.Vectors.Vector GetCoefficientVector(double[] gi)
		{
			Math_Collection.LinearAlgebra.Vectors.Vector v = new Math_Collection.LinearAlgebra.Vectors.Vector(new double[_amountOfSplineParts + 1]);
			for (int i = 2; i <= _amountOfSplineParts; i++)
			{
				v[i - 1] = gi[i];
			}

			return v;
		}

		private Math_Collection.LinearAlgebra.Vectors.Vector CopyTo(Math_Collection.LinearAlgebra.Vectors.Vector v, int startIndex, int sizeForNewArray)
		{
			if (v == null)
				return null;

			if (v.Size == 0)
				return v;

			if (sizeForNewArray < v.Size + startIndex)
				throw new System.ArgumentException("The new vector must be big enough to old the data from the old vector");

			double[] newValues = new double[sizeForNewArray];
			v.Values.CopyTo(newValues, startIndex);

			return new Math_Collection.LinearAlgebra.Vectors.Vector(newValues);
		}
	}
}
