using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pixtro.Client.Editor
{
	public sealed class EditorLayout
	{
		private MainForm _mainForm;

		public enum SplitDirection
		{
			Vertical,
			Horizontal
		}

		public interface ILayoutInfo
		{
			Rectangle BoundingRect { get; set; }
			Size MinimumSize();
			void ResizeWindow(Rectangle bound);
		}
		public class LayoutSplit : ILayoutInfo
		{
			public const int SPLIT_PIXEL_SIZE = 3;

			public SplitDirection Direction;
			public float SplitPercent { get => split;
				set
				{
					split = value;
					

					ResizeWindow(BoundingRect);
				}
			}
			private float split = 0.5f;
			public ILayoutInfo Item1, Item2;

			public Rectangle BoundingRect { get; set; }

			public Size MinimumSize()
			{
				int width = 0, height;

				var p1 = Item1.MinimumSize();
				var p2 = Item2.MinimumSize();

				// Swap coordinates temporarily if splitting vertical just to reuse code to check sizes
				if (Direction == SplitDirection.Vertical)
				{
					int temp = p1.Width;
					p1.Width = p1.Height;
					p1.Height = temp;

					temp = p2.Width;
					p2.Width = p2.Height;
					p2.Height = temp;
				}

				if (p1.Width > 0 && p2.Width > 0)
					width = p1.Width + p2.Width;
				else if (p1.Width > 0)
					width = p1.Width;
				else if (p2.Width > 0)
					width = p2.Width;

				width += SPLIT_PIXEL_SIZE;

				height = Math.Max(p1.Height, p2.Height);

				if (Direction == SplitDirection.Vertical)
					return new Size(height, width);
				else
					return new Size(width, height);
			}
			public void ResizeWindow(Rectangle bound)
			{
				BoundingRect = bound;

				if (Direction == SplitDirection.Vertical)
					bound.Height -= SPLIT_PIXEL_SIZE;
				else
					bound.Width -= SPLIT_PIXEL_SIZE;

				Rectangle rect1 = new Rectangle(bound.X, bound.Y,
					(Direction == SplitDirection.Horizontal) ? (int)(bound.Width * SplitPercent) : bound.Height,
					(Direction == SplitDirection.Horizontal) ? bound.Width : (int)(bound.Height * SplitPercent));

				Rectangle rect2 = (Direction == SplitDirection.Horizontal) ?
					new Rectangle(bound.X + SPLIT_PIXEL_SIZE + rect1.Width, bound.Y, bound.Width - rect1.Width, bound.Height) :
					new Rectangle(bound.X, bound.Y + SPLIT_PIXEL_SIZE + rect1.Height, bound.Width, bound.Height - rect1.Height);

				Item1.ResizeWindow(rect1);
				Item2.ResizeWindow(rect2);
			}
		}
		public class LayoutWindow : ILayoutInfo
		{
			public PresentationPanel Panel { get; set; }

			public int MinimumWidth { get; private set; } = 240;
			public int MinimumHeight { get; private set; } = 160;

			public Rectangle BoundingRect { get; set; }

			public LayoutWindow(PresentationPanel panel)
			{
				Panel = panel;
			}

			public Size MinimumSize()
			{
				return new Size(MinimumWidth, MinimumHeight);
			}
			public void ResizeWindow(Rectangle bound)
			{
				BoundingRect = bound;
				Panel.Control.Width = bound.Width;
				Panel.Control.Height = bound.Height;

				Panel.Control.Location = new Point(bound.X, bound.Y);
			}
		}


		public ILayoutInfo layout;

		private LayoutSplit adjustingLayout = null;

		public EditorLayout (MainForm mainForm)
		{
			_mainForm = mainForm;
			_mainForm.Resize += OnResize;
		}

		private void OnResize(object sender, EventArgs e)
		{
			layout.ResizeWindow(_mainForm.EditorBounds);
		}

		public void OnLeftMouseDown(MouseEventArgs e)
		{

		}
		public void OnLeftMouseUp(MouseEventArgs e)
		{
			adjustingLayout = null;
		}
		public void OnMouseMove(MouseEventArgs e)
		{
			if (adjustingLayout != null)
			{
				bool horz = adjustingLayout.Direction == SplitDirection.Horizontal;
				float mouse = horz ? e.X - adjustingLayout.BoundingRect.X : e.Y - adjustingLayout.BoundingRect.Y;
				float end = horz ? adjustingLayout.BoundingRect.Width : adjustingLayout.BoundingRect.Height;

				adjustingLayout.SplitPercent = mouse / end;
			}
		}

		public LayoutWindow GetWindowAt(Point point)
		{
			return FindWindow(layout, point);
			
		}
		private LayoutWindow FindWindow(ILayoutInfo info, Point point)
		{
			if (info is LayoutWindow)
				return info as LayoutWindow;

			return null;
		}

		public void SplitAt(Point point, SplitDirection direction)
		{

		}
	}
}
