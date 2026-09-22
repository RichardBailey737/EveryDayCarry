using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ensur_Trigger_Builder
{
	public delegate void PropertyChosenEventHandler(string propertyName);
	public partial class PropertyReference : UserControl
    {
        public PropertyReference()
        {
            InitializeComponent();
        }

		Dictionary<string, string> properties = new Dictionary<string, string>();

		public event PropertyChosenEventHandler PropertyChosen;

		

		public void Initalize()
        {

			
			var assembly = Assembly.LoadFrom(System.IO.Path.Combine(Application.StartupPath, "ensurMMS.dll"));
			List<Type> assemblyTypes = new List<Type>();
			assemblyTypes.Add(assembly.GetType("Document"));
			assemblyTypes.Add(assembly.GetType("User"));
			//Type[] assemblyTypes = assembly.GetTypes();
			//assemblyTypes = assemblyTypes.Where(asm => new string[] { "Document", "User" }.Contains(asm.FullName)).ToArray();

			

			foreach (var typ in assemblyTypes)
			{
				//var treeNode = treeViewItems.Nodes.Add(typ.Name);
				MemberInfo[] memberInfo = typ.GetProperties().Where(p => p.CanRead).ToArray();
				for (int j = 0; j < memberInfo.Length; j++)
				{
					if (memberInfo[j].ReflectedType.IsPublic && memberInfo[j].MemberType != MemberTypes.Method)
					{
						properties.Add(typ.Name + "." + memberInfo[j].Name, memberInfo[j].Name);
						//TreeNode node = treeNode.Nodes.Add(memberInfo[j].Name);
						//node.Tag = memberInfo[j].MemberType;

					}
				}
			}
			propertyLbx.DisplayMember = "Key";
			propertyLbx.ValueMember = "Value";
			propertyLbx.DataSource = new BindingSource(properties, null);
			
		}

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
			if (e.KeyCode == Keys.Enter)
			{
				if (propertyLbx.SelectedItems.Count > 0)
                {
					string val = ((KeyValuePair<string, string>)propertyLbx.SelectedItem).Value;
					if (PropertyChosen!=null) PropertyChosen(val);
                }
			} else if (e.KeyCode== Keys.Down)
            {
				if (propertyLbx.Items.Count -1 > propertyLbx.SelectedIndex) propertyLbx.SelectedIndex++;
				searchBox.Focus();
			} else if (e.KeyCode == Keys.Up)
            {
				if (propertyLbx.SelectedIndex > 0) propertyLbx.SelectedIndex--;
				searchBox.Focus();
			} else if (e.KeyCode == Keys.Escape)
            {
				searchBox.Text = "";
				propertyLbx.DataSource = null;
				propertyLbx.DisplayMember = "Key";
				propertyLbx.ValueMember = "Value";
				propertyLbx.DataSource = new BindingSource(properties, null);
			}
			else
			{
				propertyLbx.DataSource = null;
				propertyLbx.DisplayMember = "Key";
				propertyLbx.ValueMember = "Value";
				if (searchBox.Text.Length > 0) { 
					var props = properties.Where(p => p.Value.ToUpper().Contains(searchBox.Text.ToUpper())).ToDictionary(pair => pair.Key, pair => pair.Value); 
					if (props.Count > 0) propertyLbx.DataSource = new BindingSource(props, null);
				}
				else
					propertyLbx.DataSource = new BindingSource(properties, null);
			}
        }

        private void propertyLbx_MouseDoubleClick(object sender, MouseEventArgs e)
        {
			if (propertyLbx.SelectedItems.Count > 0)
			{
				string val = ((KeyValuePair<string, string>)propertyLbx.SelectedItem).Value;
				if (PropertyChosen!=null) PropertyChosen(val);
			}
		}
    }
}
