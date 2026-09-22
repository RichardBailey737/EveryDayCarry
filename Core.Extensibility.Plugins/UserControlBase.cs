using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Core.Extensibility.Plugins
{
    /// <summary>
    /// Custom user control base class.  This is the custom base class for any ensure user control plugin.  
    /// To make this work you need two things: 
    /// 1) A class that inherits this base class
    /// 2) An .ascx file that exists in the same DLL as the inheriting class, has NO designer.cs, and has a build action
    /// set to EmbeddedResource
    /// </summary>
    public abstract class CoreUserControlBase : System.Web.UI.UserControl
    {
        /// <summary>
        /// Since this base class is in it's own DLL, this is a simple trick to ensure that 
        /// the ParentAssembly is the Assembly the PLUGIN exists in and not the base class or ensure.  
        /// This is used to load in the .ascx file so it needs to point to the DLL with the .ascx class in it.  You can set this to the following:
        /// public override Assembly ParentAssembly => Assembly.GetExecutingAssembly();
        /// </summary>
        public abstract Assembly ParentAssembly { get; }
        
        /// <summary>
        /// Complication optional dictionary of Input values.   Mostly used for Forms.
        /// </summary>
        public Dictionary<string, object> InputData { get; set; }


        /// <summary>
        /// This is a function to call when the control needs to store it's value(s).  So if you have a form, this will be called on Form Commit 
        /// to store it's value with the rest of the form.  (It's here because YOU know what kind of values and how many values it stores, but I have no idea 
        /// what you might use this for)
        /// </summary>
        /// <param name="input">Dictionary of values to send on execution</param>
        /// <returns>A string with any return message.  The assumption here is that a null or empty string means success.</returns>
        public virtual string UpdateControlValue(Dictionary<string, object> input)
        {
            return null;
        }

        /// <summary>
        /// This loads the .ascx file from the resource stream istead of looking for it in the project.  This is what allows us to 
        /// embed it in a DLL.
        /// </summary>
        protected override void FrameworkInitialize()
        {
            base.FrameworkInitialize();
            string content;

            var resourceName = GetType().FullName;
            //resourceName = resourceName.Substring(resourceName.LastIndexOf(".")+1, resourceName.Length - resourceName.LastIndexOf(".")-1);
            resourceName += ".ascx";
            var stream = ParentAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException(
                  string.Format("Loading resource '{0}' failed", resourceName)
                );
            }

            using (var reader = new StreamReader(stream))
                content = reader.ReadToEnd();

            var userControl = Page.ParseControl(content);
            if (userControl == null)
            {
                throw new InvalidOperationException(
                  string.Format("Parsing user control in resource '{0}' failed", resourceName)
                );
            }
            Controls.Add(userControl);


            WireControls(userControl);
        }


        /// <summary>
        /// Since there is no designer file, you MUST use this to tell the user control what controls it has and where to find them.  
        /// If you have label called "myLabel", create a local variable and add the following line in this override.
        /// myLabel = (Label)userControl.FindControl("myLabel");
        /// </summary>
        /// <param name="userControl">This user control</param>
        protected abstract void WireControls(System.Web.UI.Control userControl);
    }
}
