using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ensur_Trigger_Builder
{


	// GListBox class 
	public class GListBox : ListBox
	{
		private ImageList _myImageList;
		public ImageList ImageList
		{
			get { return _myImageList; }
			set { _myImageList = value; }
		}
		public GListBox()
		{
			// Set owner draw mode
			this.DrawMode = DrawMode.OwnerDrawFixed;
		}
		protected override void OnDrawItem(System.Windows.Forms.DrawItemEventArgs e)
		{
			e.DrawBackground();
			e.DrawFocusRectangle();
			GListBoxItem item;
			Rectangle bounds = e.Bounds;
			
			//try
			//{
				
			//	Size imageSize = _myImageList.ImageSize;
			//	item = (GListBoxItem)Items[e.Index];
			//	if (item.ImageIndex != -1)
			//	{
			//		_myImageList.Draw(e.Graphics, bounds.Left, bounds.Top, item.ImageIndex);
			//		e.Graphics.DrawString(item.Text, e.Font, new SolidBrush(e.ForeColor),
			//			bounds.Left+imageSize.Width, bounds.Top);
			//	}
			//	else
			//	{
			//		e.Graphics.DrawString(item.Text, e.Font, new SolidBrush(e.ForeColor),
			//			bounds.Left, bounds.Top);
			//	}
			//}
			//catch
			//{
				if (e.Index != -1  && Items.Count > 0)
				{
					e.Graphics.DrawString(Items[e.Index].ToString(), e.Font,
						new SolidBrush(e.ForeColor), bounds.Left, bounds.Top);
				}
				else
				{
					e.Graphics.DrawString(Text, e.Font, new SolidBrush(e.ForeColor),
						bounds.Left, bounds.Top);
				}
			//}
			base.OnDrawItem(e);
		}
	}//End of GListBox class
}
