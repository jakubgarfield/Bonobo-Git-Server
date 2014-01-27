using System.Web;
using System.Web.Mvc;

namespace HolisticWare.ArbitrationExpert.EXE
{
	public partial class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}