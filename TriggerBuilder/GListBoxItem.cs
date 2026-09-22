using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ensur_Trigger_Builder
{
	// GListBoxItem class 
	public class GListBoxItem
	{
		private string _myText;
		private int _myImageIndex;
		// properties 
		public string Text
		{
			get { return _myText; }
			set { _myText = value; }
		}
		public int ImageIndex
		{
			get { return _myImageIndex; }
			set { _myImageIndex = value; }
		}
		//constructor
		public GListBoxItem(string text, int index)
		{
			_myText = text;
			_myImageIndex = index;
		}
		public GListBoxItem(string text) : this(text, -1) { }
		public GListBoxItem() : this("") { }
		public override string ToString()
		{
			return _myText;
		}
	}//End of GListBoxItem class
}
