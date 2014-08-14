using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace JsReportVSTools.Impl
{
    public static class ProjectExtensions
    {
        public static IEnumerable<ProjectItem> GetAllProjectItems(this Project project)
        {
            var rootItems = project.ProjectItems.Cast<ProjectItem>().ToList();
            var result = new List<ProjectItem>(rootItems);


            foreach (var item in rootItems)
            {
                result.AddRange(GetAllProjectItemsInner(item));
            }

            return result;
        }

        private static IEnumerable<ProjectItem> GetAllProjectItemsInner(ProjectItem item)
        {
            var result = new List<ProjectItem>() { item };

            foreach (ProjectItem innerItem in item.ProjectItems)
            {
                result.AddRange(GetAllProjectItemsInner(innerItem));
            }

            return result;
        } 
    }
}