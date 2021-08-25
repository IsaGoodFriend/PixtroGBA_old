using Pixtro.Client.Editor.Projects.Editors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pixtro.Client.Editor
{
	public sealed class EditorLayout : IEnumerable<EditorLayout.LayoutWindow>
	{
		private MainForm _mainForm;

		public enum SplitDirection
		{
			None,
			Vertical,
			Horizontal
		}

		public interface ILayoutInfo
		{
			Rectangle BoundingRect { get; set; }
			Size MinimumSize();
			void ResizeWindow(Rectangle bound);
			void FinalizeSize();
		}
		public class LayoutSplit : ILayoutInfo
		{
			public const int SPLIT_PIXEL_SIZE = 3;

			public SplitDirection Direction = SplitDirection.Horizontal;
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
					(Direction == SplitDirection.Horizontal) ? (int)(bound.Width * SplitPercent) : bound.Width,
					(Direction == SplitDirection.Horizontal) ? bound.Height : (int)(bound.Height * SplitPercent));

				Rectangle rect2 = (Direction == SplitDirection.Horizontal) ?
					new Rectangle(0, bound.Y, bound.Width - rect1.Width, bound.Height) :
					new Rectangle(bound.X, 0, bound.Width, bound.Height - rect1.Height);

				var size1 = Item1.MinimumSize();
				var size2 = Item2.MinimumSize();

				if (Direction == SplitDirection.Horizontal)
				{
					if (rect1.Width < size1.Width)
					{
						rect2.Width -= size1.Width - rect1.Width;
						rect1.Width = size1.Width;
					}
					else if (rect2.Width < size2.Width)
					{
						rect1.Width -= size2.Width - rect2.Width;
						rect2.Width = size2.Width;
					}
					rect2.X = rect1.Right + SPLIT_PIXEL_SIZE;
				}
				else
				{
					if (rect1.Height < size1.Height)
					{
						rect2.Height -= size1.Height - rect1.Height;
						rect1.Height = size1.Height;
					}
					else if (rect2.Height < size2.Height)
					{
						rect1.Height -= size2.Height - rect2.Height;
						rect2.Height = size2.Height;
					}
					rect2.Y = rect1.Bottom + SPLIT_PIXEL_SIZE;
				}

				Item1.ResizeWindow(rect1);
				Item2.ResizeWindow(rect2);
			}
			public void FinalizeSize()
			{
				Item1.FinalizeSize();
				Item2.FinalizeSize();
			}
		}
		public class LayoutWindow : ILayoutInfo
		{
			public DisplayManager Manager { get; set; }

			public int MinimumWidth { get; private set; } = 100;
			public int MinimumHeight { get; private set; } = 100;

			public Rectangle BoundingRect { get; set; }

			public LayoutWindow(DisplayManager manager)
			{
				Manager = manager;

				switch (manager.Name)
				{
					case MainForm.EMULATOR_DISPLAY:
						MinimumWidth = 240;
						MinimumHeight = 160;
						break;
				}
			}

			public Size MinimumSize()
			{
				return new Size(MinimumWidth, MinimumHeight);
			}
			public void ResizeWindow(Rectangle bound)
			{
				BoundingRect = bound;

				Manager.Panel.Control.Width = bound.Width;
				Manager.Panel.Control.Height = bound.Height;

				Manager.Panel.Control.Location = new Point(bound.X, bound.Y);
			}
			public void FinalizeSize()
			{
				if (Manager.VideoProvider is Projects.VisualEditorBase)
				{
					var editor = Manager.VideoProvider as Projects.VisualEditorBase;
					editor.VirtualWidth = BoundingRect.Width;
					editor.VirtualHeight = BoundingRect.Height;
				}
			}
		}


		public ILayoutInfo layout;

		private LayoutSplit adjustingLayout = null;

		public EditorLayout (MainForm mainForm)
		{
			_mainForm = mainForm;
			_mainForm.Resize += OnResize;
			_mainForm.ResizeEnd += OnResizeEnd;
		}

		private void OnResize(object sender, EventArgs e)
		{
			layout.ResizeWindow(_mainForm.EditorBounds);
		}
		private void OnResizeEnd(object sender, EventArgs e)
		{
			layout.FinalizeSize();
		}

		public void OnLeftMouseDown(MouseEventArgs e)
		{
			var clickedObject = GetWindowAt(e.Location);

			if (clickedObject is LayoutSplit)
			{
				adjustingLayout = clickedObject as LayoutSplit;
			}
		}
		public void OnLeftMouseUp(MouseEventArgs e)
		{
			if (adjustingLayout != null)
			{
				adjustingLayout.FinalizeSize();
			}
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
			else
			{
				var splitDir = GetSplitDirection(e.Location);

				if (splitDir != SplitDirection.None)
				{
					Cursor.Current = splitDir == SplitDirection.Horizontal ? Cursors.SizeWE : Cursors.SizeNS;
				}
			}
		}

		public SplitDirection GetSplitDirection(Point point)
		{
			return GetSplitDirection(layout, point);
		}
		private SplitDirection GetSplitDirection(ILayoutInfo info, Point point)
		{
			if (info is LayoutWindow)
			{
				return SplitDirection.None;
			}
			else
			{
				LayoutSplit split = info as LayoutSplit;

				if (split.Item1.BoundingRect.Contains(point))
				{
					return GetSplitDirection(split.Item1, point);
				}
				else if (split.Item2.BoundingRect.Contains(point))
				{
					return GetSplitDirection(split.Item2, point);
				}

				return split.Direction;
			}
		}

		public ILayoutInfo GetWindowAt(Point point)
		{
			return GetWindowAt(layout, point);
		}
		private ILayoutInfo GetWindowAt(ILayoutInfo info, Point point)
		{
			if (info is LayoutWindow)
			{
				return info;
			}
			else
			{
				LayoutSplit split = info as LayoutSplit;

				info = null;

				if (split.Item1.BoundingRect.Contains(point))
				{
					return GetWindowAt(split.Item1, point);
				}
				else if (split.Item2.BoundingRect.Contains(point))
				{
					return GetWindowAt(split.Item2, point);
				}

				return split;
			}
		}

		public void SplitAt(Point point, SplitDirection direction)
		{

			//Controls.Add(emu.Panel);
			//Controls.SetChildIndex(emu.Panel, 0);

		}

		private IEnumerable<LayoutWindow> GetFromSplit(LayoutSplit split)
		{
			if (split.Item1 is LayoutSplit)
				foreach (var item in GetFromSplit(split.Item1 as LayoutSplit))
					yield return item;
			else
				yield return split.Item1 as LayoutWindow;

			if (split.Item2 is LayoutSplit)
				foreach (var item in GetFromSplit(split.Item2 as LayoutSplit))
					yield return item;
			else
				yield return split.Item2 as LayoutWindow;
		}

		public IEnumerator<LayoutWindow> GetEnumerator()
		{
			if (layout is LayoutSplit)
				foreach (var item in GetFromSplit(layout as LayoutSplit))
					yield return item;
			else
				yield return layout as LayoutWindow;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
