using Math_Collection.LGS;
using Math_Collection.LinearAlgebra.Matrices;
using RuntimeFunctionParser;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SplineDrawer
{
	internal class Spline
    {
        private SortedList<double,double> _sourcePoints { get; set; }

        private List<Function> _partialPolynomials;
        public List<Function> PartialPolynomials
        {
            get { return _partialPolynomials; }
            set { _partialPolynomials = value; }
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

			int n = _sourcePoints.Count;

			double[] xs = _sourcePoints.Keys.ToArray();
			double[] ys = _sourcePoints.Values.ToArray();
			
			#region Calculate Help Variable hi

			// hi = xi+1 - xi
			double[] hi = CalculateH(xs, n);

			#endregion

			#region Calculates all d values

			// a's = yi
			double[] ai = ys;

			#endregion

			#region Calculate all c's

			Matrix coefficientMatrix = GetCoefficientMatrix(hi, n);
			Math_Collection.LinearAlgebra.Vectors.Vector coefficientVector = GetCoefficientVector(hi, ys, n);
			LGS lgs = new LGS(coefficientMatrix, coefficientVector);

			Math_Collection.LinearAlgebra.Vectors.Vector resultFromLGS = lgs.Solve(LGS.SolveAlgorithm.Gauß);

			Math_Collection.LinearAlgebra.Vectors.Vector ci = CopyTo(resultFromLGS, 1, resultFromLGS.Size + 2);

			// Natural Spline Condition
			ci[0] = 0;
			ci[ci.Size - 1] = 0;
			#endregion

			#region Calculate all b's

			double[] bi = new double[n - 1];
			for(int i = 0; i < n-1; i++)
			{
				bi[i] = ((ys[i + 1] - ys[i] / hi[i]) - ((hi[i] / 3) * (2 * ci[i] + ci[i + 1])));
			}

			#endregion

			#region Calculate all d's

			double[] di = new double[n - 1];
			for(int i = 0; i < n - 1; i++)
			{
				di[i] = (1 / (3 * hi[i])) * (ci[i + 1] - ci[i]);
			}

			#endregion

			#region Generate all Functions

			Function[] functionParts = new Function[n];
			for(int i = 0; i< n-1; i++)
			{
				string f = ai[i] + "+" + bi[i] + "*x+" + ci[i] + "*x^2+" + di[i] + "*x^3";
				functionParts[i] = parser.ParseFunction(f);
			}

			#endregion

			return functionParts.ToList();
		}

		private double[] CalculateH(double[] xValues, int n)
		{
			double[] hi = new double[n - 1];
			for (int i = 1; i < n; i++)
				hi[i - 1] = xValues[i] - xValues[i - 1];

			return hi;
		}

		private Matrix GetCoefficientMatrix(double[] h, int n)
		{
			Matrix m = new Matrix(new double[n - 2, n - 2]);

			// Diagnal
			for (int i = 0; i < m.RowCount; i++)
			{
				m[i, i] = 2 * (h[i] + h[i + 1]);
			}

			// Under Diagonal
			for (int k = 1; k < m.ColumnCount; k++)
			{
				m[k - 1, k] = h[k];
			}

			// Abover Diagonal
			for (int o = 1; o < m.RowCount; o++)
			{
				m[o, o - 1] = h[o];
			}

			return m;
		}

		private Math_Collection.LinearAlgebra.Vectors.Vector GetCoefficientVector(double[] h, double[] y, int n)
		{
			Math_Collection.LinearAlgebra.Vectors.Vector v = new Math_Collection.LinearAlgebra.Vectors.Vector(new double[n-2]);
			
			for (int i = 1; i <= v.Size; i++)
			{
				v[i-1] = 6 * ((y[i + 1] - y[i]) / h[i] - (y[i] - y[i - 1]) / h[i - 1]);
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
