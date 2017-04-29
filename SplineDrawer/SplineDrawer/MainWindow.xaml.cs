using RuntimeFunctionParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SplineDrawer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private List<Point> _inputPoints;
		private Spline _spline;
		private const int _scale = 5;
		public MainWindow()
		{
			InitializeComponent();
			_inputPoints = new List<Point>();
			_spline = null;
		}

		#region Render Functions

		private void RenderSpline(Spline s)
		{
			if (s == null || s.PartialPolynomials.Count == 0)
				return;

			for (int i = 0; i < s.PartialPolynomials.Count; i++)
			{
				RenderFunction(s.PartialPolynomials[i], _inputPoints[i].X, _inputPoints[i + 1].X);
			}
		}

		private void RenderPoints(List<Point> points)
		{
			foreach (Point p in points)
			{
				Ellipse e = new Ellipse();
				e.Width = e.Height = 6;
				e.Margin = new Thickness(p.X, p.Y, 0, 0);
				e.Fill = Brushes.Black;
				e.Stroke = Brushes.White;
				e.StrokeThickness = 1;
				mainCanvas.Children.Add(e);
			}
		}

		private void RenderFunction(Function f, double minX, double maxX)
		{
			if (f == null)
				return;

			double step = ((maxX - minX) / _inputPoints.Count) * 0.1;
			Polyline pl = new Polyline();
			pl.Stroke = Brushes.Red;
			pl.StrokeThickness = 3;
			PointCollection pc = new PointCollection();
			for (double i = minX; i <= maxX; i += step)
			{
				Point p = new Point(i* _scale, f.Solve(i, 0)* _scale);
				pc.Add(p);
			}
			pl.Points = pc;
			mainCanvas.Children.Add(pl);
		}

		#endregion

		#region Helper Functions

		private void Reset()
		{
			mainCanvas.Children.Clear();
			_inputPoints.Clear();
			_spline = null;
		}

		private void CalculateAndRenderSpline()
		{
			// DEBUG
			_inputPoints = new List<Point>();
			_inputPoints.Add(new Point(1, 8));
			_inputPoints.Add(new Point(7, 10));
			_inputPoints.Add(new Point(12, 7));
			_inputPoints.Add(new Point(15, 8));
			_inputPoints.Add(new Point(19, 7));
			// 

			_spline = new Spline(_inputPoints);
			RenderSpline(_spline);
		}

		private List<Point> NormalizePoints(List<Point> points)
		{
			List<Point> normalizedPoints = new List<Point>(points.Count);
			foreach(Point p in points)
			{
				Point normalized = new Point(p.X / mainCanvas.ActualWidth, p.Y / mainCanvas.ActualHeight);
				normalizedPoints.Add(normalized);
			}
			return normalizedPoints;
		}

		private List<Point> UnnormalizePoints(List<Point> points)
		{
			List<Point> unnormalizedPoints = new List<Point>(points.Count);
			foreach (Point p in points)
			{
				Point unnormalized = new Point(p.X * mainCanvas.ActualWidth, p.Y * mainCanvas.ActualHeight);
				unnormalizedPoints.Add(unnormalized);
			}
			return unnormalizedPoints;
		}

		#endregion

		#region Canvas Events

		private void mainCanvas_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Enter:
					CalculateAndRenderSpline();
					break;
				case Key.R:
					Reset();
					break;
			}
		}

		private void mainCanvas_Loaded(object sender, RoutedEventArgs e)
		{
			mainCanvas.Focusable = true;
			mainCanvas.Focus();
		}

		private void mainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Point p = e.GetPosition(mainCanvas);
			_inputPoints.Add(p);
			RenderPoints(_inputPoints);
		}

		#endregion
	}
}
