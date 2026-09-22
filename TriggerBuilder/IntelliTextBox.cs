using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ensur_Trigger_Builder
{
    public partial class IntelliTextBox : UserControl
    {
        public IntelliTextBox()
        {
            InitializeComponent();
        }

		private TreeNode findNodeResult = null;
		private string typed = "";
		private bool wordMatched = false;
		private Assembly assembly;
		private Hashtable namespaces;
		private TreeNode nameSpaceNode;
		private bool foundNode = false;
		private string currentPath;
		private List<string> properties = new List<string>();



		private void isTbx_KeyDown(object sender, KeyEventArgs e)
        {
            // Keep track of the current character, used
            // for tracking whether to hide the list of members,
            // when the delete button is pressed
            int i = isTbx.SelectionStart;
            string currentChar = "";

            if (i > 0)
            {
                currentChar = this.isTbx.Text.Substring(i-1, 1);
            }

            if (e.KeyData == Keys.OemOpenBrackets)
            {
                // The amazing dot key

                if (!this.listBoxAutoComplete.Visible)
                {
                    // Display the member listview if there are
                    // items in it
                    if (populateListBox())
                    {
                        //this.listBoxAutoComplete.SelectedIndex = 0;

                        // Find the position of the caret
                        Point point = this.isTbx.GetPositionFromCharIndex(isTbx.SelectionStart);
                        point.Y += (int)Math.Ceiling(this.isTbx.Font.GetHeight()) + 2;
                        point.X += 2; // for Courier, may need a better method

                        //this.statusBar1.Text = point.X + "," + point.Y;
                        this.listBoxAutoComplete.Location = point;
                        this.listBoxAutoComplete.BringToFront();
                        this.listBoxAutoComplete.Show();
                    }
                }
                else
                {
                    this.listBoxAutoComplete.Hide();
                    typed = "";
                }

            }
            else if (e.KeyCode == Keys.Back)
            {
                // Delete key - hides the member list if the character
                // being deleted is a dot

                this.textBoxTooltip.Hide();
                if (typed.Length > 0)
                {
                    typed = typed.Substring(0, typed.Length -1);
                }
                if (currentChar == ".")
                {
                    this.listBoxAutoComplete.Hide();
                }

            }
            else if (e.KeyCode == Keys.Up)
            {
                // The up key moves up our member list, if
                // the list is visible

                this.textBoxTooltip.Hide();

                if (this.listBoxAutoComplete.Visible)
                {
                    this.wordMatched = true;
                    if (this.listBoxAutoComplete.SelectedIndex > 0)
                        this.listBoxAutoComplete.SelectedIndex--;

                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                // The up key moves down our member list, if
                // the list is visible

                this.textBoxTooltip.Hide();

                if (this.listBoxAutoComplete.Visible)
                {
                    this.wordMatched = true;
                    if (this.listBoxAutoComplete.SelectedIndex < this.listBoxAutoComplete.Items.Count -1)
                        this.listBoxAutoComplete.SelectedIndex++;

                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.D9)
            {
                // Trap the open bracket key, displaying a cheap and
                // cheerful tooltip if the word just typed is in our tree
                // (the parameters are stored in the tag property of the node)

                string word = this.getLastWord();
                this.foundNode = false;
                this.nameSpaceNode = null;

                this.currentPath = "";
                searchTree(this.treeViewItems.Nodes, word, true);

                if (this.nameSpaceNode != null)
                {
                    if (this.nameSpaceNode.Tag is string)
                    {
                        this.textBoxTooltip.Text = (string)this.nameSpaceNode.Tag;

                        Point point = this.isTbx.GetPositionFromCharIndex(isTbx.SelectionStart);
                        point.Y += (int)Math.Ceiling(this.isTbx.Font.GetHeight()) + 2;
                        point.X -= 10;
                        this.textBoxTooltip.Location = point;
                        this.textBoxTooltip.Width = this.textBoxTooltip.Text.Length *6;

                        this.textBoxTooltip.Size = new Size(this.textBoxTooltip.Text.Length *6, this.textBoxTooltip.Height);

                        // Resize tooltip for long parameters
                        // (doesn't wrap text nicely)
                        if (this.textBoxTooltip.Width > 300)
                        {
                            this.textBoxTooltip.Width = 300;
                            int height = 0;
                            height = this.textBoxTooltip.Text.Length / 50;
                            this.textBoxTooltip.Height =  height *15;
                        }
                        this.textBoxTooltip.Show();
                    }
                }
            }
            else if (e.KeyCode == Keys.D8)
            {
                // Close bracket key, hide the tooltip textbox

                this.textBoxTooltip.Hide();
            }
            else if (e.KeyValue < 48 || (e.KeyValue >= 58 && e.KeyValue <= 64) || (e.KeyValue >= 91 && e.KeyValue <= 96) || e.KeyValue > 122)
            {
                // Check for any non alphanumerical key, hiding
                // member list box if it's visible.

                if (this.listBoxAutoComplete.Visible)
                {
                    // Check for keys for autofilling (return,tab,space)
                    // and autocomplete the richtextbox when they're pressed.
                    if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab || e.KeyCode == Keys.Space)
                    {
                        this.textBoxTooltip.Hide();

                        // Autocomplete
                        this.selectItem();

                        this.typed = "";
                        this.wordMatched = false;
                        e.Handled = true;
                    }

                    // Hide the member list view
                    this.listBoxAutoComplete.Hide();
                }
            }
            else
            {
                // Letter or number typed, search for it in the listview
                if (this.listBoxAutoComplete.Visible)
                {
                    char val = (char)e.KeyValue;
                    this.typed += val;

                    this.wordMatched = false;

                    // Loop through all the items in the listview, looking
                    // for one that starts with the letters typed
                    for (i=0; i < this.listBoxAutoComplete.Items.Count; i++)
                    {
                        if (this.listBoxAutoComplete.Items[i].ToString().ToLower().StartsWith(this.typed.ToLower()))
                        {
                            this.wordMatched = true;
                            this.listBoxAutoComplete.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    this.typed = "";
                }
            }
        }

		public void Initalize()
        {
			this.treeViewItems.Nodes.Clear();
			namespaces = new Hashtable();
			assembly = Assembly.LoadFrom(Path.Combine(Application.StartupPath, "ensurMMS.dll"));
			List<Type> assemblyTypes = new List<Type>();
			assemblyTypes.Add(assembly.GetType("Document"));
			assemblyTypes.Add(assembly.GetType("User"));
			//Type[] assemblyTypes = assembly.GetTypes();
			//assemblyTypes = assemblyTypes.Where(asm => new string[] { "Document", "User" }.Contains(asm.FullName)).ToArray();

			foreach (var typ in assemblyTypes)
            {
				//var treeNode = treeViewItems.Nodes.Add(typ.Name);
				MemberInfo[] memberInfo = typ.GetProperties().Where(p=>p.CanRead).ToArray();
				for (int j = 0; j < memberInfo.Length; j++)
				{
					if (memberInfo[j].ReflectedType.IsPublic && memberInfo[j].MemberType != MemberTypes.Method)
					{
						properties.Add(memberInfo[j].Name);
						//TreeNode node = treeNode.Nodes.Add(memberInfo[j].Name);
						//node.Tag = memberInfo[j].MemberType;

					}
				}
			}
		}

        private void richTextBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Hide the listview and the tooltip
            this.textBoxTooltip.Hide();
            this.listBoxAutoComplete.Hide();
        }


        private void listBoxAutoComplete_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Ignore any keys being pressed on the listview
            this.isTbx.Focus();
        }

        private void listBoxAutoComplete_DoubleClick(object sender, System.EventArgs e)
        {
            // Item double clicked, select it
            if (this.listBoxAutoComplete.SelectedItems.Count == 1)
            {
                this.wordMatched = true;
                this.selectItem();
                this.listBoxAutoComplete.Hide();
                this.isTbx.Focus();
                this.wordMatched = false;
            }
        }

        private void listBoxAutoComplete_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            // Make sure when an item is selected, control is returned back to the richtext
            this.isTbx.Focus();
        }

        private void textBoxTooltip_Enter(object sender, System.EventArgs e)
        {
            // Stop the fake tooltip's text being selected
            this.isTbx.Focus();
        }


		#region Util methods

		/// <summary>
		/// Takes an assembly filename, opens it and retrieves all types.
		/// </summary>
		/// <param name="filename">Filename to open</param>
		private void readAssembly(string filename)
		{
			this.treeViewItems.Nodes.Clear();
			namespaces = new Hashtable();
			assembly = Assembly.LoadFrom(filename);

			Type[] assemblyTypes = assembly.GetTypes();
			this.treeViewItems.Nodes.Clear();

			// Cycle through types
			foreach (Type type in assemblyTypes)
			{
				if (type.Namespace != null)
				{
					if (namespaces.ContainsKey(type.Namespace))
					{
						// Already got namespace, add the class to it
						TreeNode treeNode = (TreeNode)namespaces[type.Namespace];
						treeNode = treeNode.Nodes.Add(type.Name);
						this.addMembers(treeNode, type);

						if (type.IsClass)
						{
							treeNode.Tag = MemberTypes.Custom;
						}
					}
					else
					{
						// New namespace
						TreeNode membersNode = null;

						if (type.Namespace.IndexOf(".") != -1)
						{
							// Search for already existing parts of the namespace
							nameSpaceNode = null;
							foundNode = false;

							this.currentPath = "";
							searchTree(this.treeViewItems.Nodes, type.Namespace, false);

							// No existing namespace found
							if (nameSpaceNode == null)
							{
								// Add the namespace
								string[] parts = type.Namespace.Split('.');

								TreeNode treeNode = treeViewItems.Nodes.Add(parts[0]);
								string sNamespace = parts[0];

								if (!namespaces.ContainsKey(sNamespace))
								{
									namespaces.Add(sNamespace, treeNode);
								}

								for (int i = 1; i < parts.Length; i++)
								{
									treeNode = treeNode.Nodes.Add(parts[i]);
									sNamespace += "." +parts[i];
									if (!namespaces.ContainsKey(sNamespace))
									{
										namespaces.Add(sNamespace, treeNode);
									}
								}

								membersNode = treeNode.Nodes.Add(type.Name);
							}
							else
							{
								// Existing namespace, add this namespace to it,
								// and add the class
								string[] parts = type.Namespace.Split('.');
								TreeNode newNamespaceNode = null;

								if (!namespaces.ContainsKey(type.Namespace))
								{
									newNamespaceNode = nameSpaceNode.Nodes.Add(parts[parts.Length-1]);
									namespaces.Add(type.Namespace, newNamespaceNode);
								}
								else
								{
									newNamespaceNode = (TreeNode)namespaces[type.Namespace];
								}

								if (newNamespaceNode != null)
								{
									membersNode = newNamespaceNode.Nodes.Add(type.Name);
									if (type.IsClass)
									{
										membersNode.Tag = MemberTypes.Custom;
									}
								}
							}

						}
						else
						{
							// Single root namespace, add to root
							membersNode = treeViewItems.Nodes.Add(type.Namespace);
						}

						// Add all members
						if (membersNode != null)
						{
							this.addMembers(membersNode, type);
						}
					}
				}

			}
		}

		/// <summary>
		/// Adds all members to the node's children, grabbing the parameters
		/// for methods.
		/// </summary>
		/// <param name="treeNode"></param>
		/// <param name="type"></param>
		private void addMembers(TreeNode treeNode, System.Type type)
		{
			// Get all members except methods
			MemberInfo[] memberInfo = type.GetMembers();
			for (int j = 0; j < memberInfo.Length; j++)
			{
				if (memberInfo[j].ReflectedType.IsPublic && memberInfo[j].MemberType != MemberTypes.Method)
				{
					TreeNode node = treeNode.Nodes.Add(memberInfo[j].Name);
					node.Tag = memberInfo[j].MemberType;
				}
			}

			// Get all methods
			MethodInfo[] methodInfo = type.GetMethods();
			for (int j = 0; j < methodInfo.Length; j++)
			{
				TreeNode node = treeNode.Nodes.Add(methodInfo[j].Name);
				string parms = "";

				ParameterInfo[] parameterInfo = methodInfo[j].GetParameters();
				for (int f = 0; f < parameterInfo.Length; f++)
				{
					parms += parameterInfo[f].ParameterType.ToString()+ " " +parameterInfo[f].Name+ ", ";
				}

				// Knock off remaining ", "
				if (parms.Length > 2)
				{
					parms = parms.Substring(0, parms.Length -2);
				}

				node.Tag = parms;
			}
		}

		/// <summary>
		/// Searches the tree view for a namespace, saving the node. The method
		/// stops and returns as soon as the namespace search can't find any
		/// more items in its path, unless continueUntilFind is true.
		/// </summary>
		/// <param name="treeNodes"></param>
		/// <param name="path"></param>
		/// <param name="continueUntilFind"></param>
		private void searchTree(TreeNodeCollection treeNodes, string path, bool continueUntilFind)
		{
			if (this.foundNode)
			{
				return;
			}

			string p = "";
			int n = 0;
			n = path.IndexOf(".");

			if (n != -1)
			{
				p = path.Substring(0, n);

				if (currentPath != "")
				{
					currentPath += "." +p;
				}
				else
				{
					currentPath = p;
				}

				// Knock off the first part
				path = path.Remove(0, n+1);
			}
			else
			{
				currentPath += "." +path;
			}

			for (int i = 0; i < treeNodes.Count; i++)
			{
				if (treeNodes[i].FullPath == currentPath)
				{
					if (continueUntilFind)
					{
						nameSpaceNode = treeNodes[i];
					}

					nameSpaceNode = treeNodes[i];

					// got a dot, continue, or return
					this.searchTree(treeNodes[i].Nodes, path, continueUntilFind);

				}
				else if (!continueUntilFind)
				{
					foundNode = true;
					return;
				}
			}
		}

		/// <summary>
		/// Searches the tree until the given path is found, storing
		/// the found node in a member var.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="treeNodes"></param>
		private void findNode(string path, TreeNodeCollection treeNodes)
		{
			for (int i = 0; i < treeNodes.Count; i++)
			{
				if (treeNodes[i].FullPath == path)
				{
					this.findNodeResult = treeNodes[i];
					break;
				}
				else if (treeNodes[i].Nodes.Count > 0)
				{
					this.findNode(path, treeNodes[i].Nodes);
				}
			}
		}

		/// <summary>
		/// Called when a "." is pressed - the previous word is found,
		/// and if matched in the treeview, the members listbox is
		/// populated with items from the tree, which are first sorted.
		/// </summary>
		/// <returns>Whether an items are found for the word</returns>
		private bool populateListBox()
		{
			bool result = false;
			string word = this.getLastWord();

			//System.Diagnostics.Debug.WriteLine(" - Path: " +word);

			if (word != "")
			{
				findNodeResult = null;
				findNode(word, this.treeViewItems.Nodes);

				if (this.findNodeResult != null)
				{
					this.listBoxAutoComplete.Items.Clear();

					if (this.findNodeResult.Nodes.Count > 0)
					{
						result = true;

						// Sort alphabetically (this could be replaced with
						// a sortable treeview)
						MemberItem[] items = new MemberItem[this.findNodeResult.Nodes.Count];
						for (int n = 0; n < this.findNodeResult.Nodes.Count; n++)
						{
							MemberItem memberItem = new MemberItem();
							memberItem.DisplayText = this.findNodeResult.Nodes[n].Text;
							memberItem.Tag = this.findNodeResult.Nodes[n].Tag;

							if (this.findNodeResult.Nodes[n].Tag != null)
							{
								System.Diagnostics.Debug.WriteLine(this.findNodeResult.Nodes[n].Tag.GetType().ToString());
							}

							items[n] = memberItem;
						}
						Array.Sort(items);

						for (int n = 0; n < items.Length; n++)
						{
							int imageindex = 0;

							if (items[n].Tag != null)
							{
								// Default to method (contains text for parameters)
								imageindex = 2;
								if (items[n].Tag is MemberTypes)
								{
									MemberTypes memberType = (MemberTypes)items[n].Tag;

									switch (memberType)
									{
										case MemberTypes.Custom:
											imageindex = 1;
											break;
										case MemberTypes.Property:
											imageindex = 3;
											break;
										case MemberTypes.Event:
											imageindex = 4;
											break;
									}
								}
							}

							this.listBoxAutoComplete.Items.Add(new GListBoxItem(items[n].DisplayText, imageindex));
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Autofills the selected item in the member listbox, by
		/// taking everything before and after the "." in the richtextbox,
		/// and appending the word in the middle.
		/// </summary>
		private void selectItem()
		{
			if (this.wordMatched)
			{
				int selstart = this.isTbx.SelectionStart;
				int prefixend = this.isTbx.SelectionStart - typed.Length;
				int suffixstart = this.isTbx.SelectionStart + typed.Length;

				if (suffixstart >= this.isTbx.Text.Length)
				{
					suffixstart = this.isTbx.Text.Length;
				}

				string prefix = this.isTbx.Text.Substring(0, prefixend);
				string fill = this.listBoxAutoComplete.SelectedItem.ToString();
				string suffix = this.isTbx.Text.Substring(suffixstart, this.isTbx.Text.Length - suffixstart);

				this.isTbx.Text = prefix + fill + suffix;
				this.isTbx.SelectionStart = prefix.Length + fill.Length;
			}
		}

		/// <summary>
		/// Searches backwards from the current caret position, until
		/// a space or newline is found.
		/// </summary>
		/// <returns>The previous word from the carret position</returns>
		private string getLastWord()
		{
			string word = "";

			int pos = this.isTbx.SelectionStart;
			if (pos > 1)
			{

				string tmp = "";
				char f = new char();
				while (f != ' ' && f != 10 && pos > 0)
				{
					pos--;
					tmp = this.isTbx.Text.Substring(pos, 1);
					f = (char)tmp[0];
					word += f;
				}

				char[] ca = word.ToCharArray();
				Array.Reverse(ca);
				word = new String(ca);

			}
			return word.Trim();

		}
		#endregion
	}

	#region MemberItem class
	/// <summary>
	/// Used for storing member items which are then
	/// alphabetically sorted.
	/// </summary>
	public class MemberItem : IComparable
	{
		public string DisplayText;
		public object Tag;

		public int CompareTo(object obj)
		{
			int result = 1;
			if (obj != null)
			{
				if (obj is MemberItem)
				{
					MemberItem memberItem = (MemberItem)obj;
					return (this.DisplayText.CompareTo(memberItem.DisplayText));
				}
				else
				{
					throw new ArgumentException();
				}
			}


			return result;
		}
	}
	#endregion
}

