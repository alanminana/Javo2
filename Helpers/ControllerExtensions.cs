using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace Javo2.Helpers
{
    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            // Obtener el ICompositeViewEngine desde los servicios
            var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine)) as Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine;

            using var sw = new StringWriter();
            var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);
            if (viewResult.View == null)
            {
                throw new FileNotFoundException("No se encontró la vista", viewName);
            }

            var viewDictionary = new ViewDataDictionary<TModel>(controller.ViewData, model)
            {
                Model = model
            };

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                viewDictionary,
                controller.TempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
