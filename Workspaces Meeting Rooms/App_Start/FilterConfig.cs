using System.Web;
using System.Web.Mvc;

namespace Workspaces_Meeting_Rooms
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
