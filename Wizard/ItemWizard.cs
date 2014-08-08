using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wizard
{
    public class ItemWizard : IWizard
    {
        protected ProjectItemTypes GetProjectItemType(EnvDTE.ProjectItem item)
        {
            if (item.Name.IndexOf(".jsrep.html") > 0)
            {
                return ProjectItemTypes.Child;
            }

            if (item.Name.IndexOf(".jsrep.js") > 0 && item.Name.IndexOf(".jsrep.json") < 0)
            {
                return ProjectItemTypes.Child;
            }
          
            return ProjectItemTypes.Parent;
        }        

        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
            ProjectItemTypes type = GetProjectItemType(projectItem);
            switch (type)
            {
                case ProjectItemTypes.Parent:
                    this.parentProjectItem = projectItem;
                    break;
                case ProjectItemTypes.Child:
                    this.childrenProjectItems.Add(projectItem);
                    break;
            }

            projectItem.Properties.Item("CopyToOutputDirectory").Value = 1;     
        }

        public void RunFinished()
        {
            foreach (EnvDTE.ProjectItem item in this.childrenProjectItems)
            {
                string filename = item.get_FileNames(0);
                this.parentProjectItem.ProjectItems.AddFromFile(filename);
            }

        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        #endregion

        private EnvDTE.ProjectItem parentProjectItem;
        private List<EnvDTE.ProjectItem> childrenProjectItems = new List<EnvDTE.ProjectItem>();

        protected enum ProjectItemTypes { Parent, Child };
        
    
    }
}
