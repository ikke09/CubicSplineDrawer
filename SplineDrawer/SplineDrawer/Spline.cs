using RuntimeFunctionParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SplineDrawer
{
    internal class Spline
    {
        private List<Point> _sourcePoints { get; set; }

        private List<Function> _partialPolynomials;
        public List<Function> PartialPolynomials
        {
            get { return _partialPolynomials; }
            set { _partialPolynomials = value; }
        }

        public Spline()
        {
            _sourcePoints = new List<Point>();
            PartialPolynomials = new List<Function>();
        }

        public Spline(List<Point> points)
        {
            _sourcePoints = points;
            PartialPolynomials = CalculateSplineParts();
        }

        private List<Function> CalculateSplineParts()
        {
            if (_sourcePoints != null && _sourcePoints.Count < 3)
            {
                return null;
            }

            throw new NotImplementedException("Not Implemented");
        }
    }
}
