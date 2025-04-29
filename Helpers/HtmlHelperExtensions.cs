// Helpers/HtmlHelperExtensions.cs
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Javo2.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent ActionButtonsForId(this IHtmlHelper helper, int id, string controller)
        {
            var editPermission = $"{controller.ToLower()}.editar";
            var viewPermission = $"{controller.ToLower()}.ver";
            var deletePermission = $"{controller.ToLower()}.eliminar";

            var user = helper.ViewContext.HttpContext.User;
            var controllerName = controller;

            var htmlBuilder = new HtmlContentBuilder();

            htmlBuilder.AppendHtml("<div class=\"btn-group\" role=\"group\">");

            if (user.HasPermission(editPermission))
            {
                htmlBuilder.AppendHtml($"<a href=\"/{controllerName}/Edit/{id}\" class=\"btn btn-sm btn-outline-primary\" title=\"Editar\">");
                htmlBuilder.AppendHtml("<i class=\"bi bi-pencil-square\"></i>");
                htmlBuilder.AppendHtml("</a>");
            }

            if (user.HasPermission(viewPermission))
            {
                htmlBuilder.AppendHtml($"<a href=\"/{controllerName}/Details/{id}\" class=\"btn btn-sm btn-outline-info\" title=\"Detalles\">");
                htmlBuilder.AppendHtml("<i class=\"bi bi-eye\"></i>");
                htmlBuilder.AppendHtml("</a>");
            }

            if (user.HasPermission(deletePermission))
            {
                htmlBuilder.AppendHtml($"<a href=\"/{controllerName}/Delete/{id}\" class=\"btn btn-sm btn-outline-danger\" title=\"Eliminar\">");
                htmlBuilder.AppendHtml("<i class=\"bi bi-trash\"></i>");
                htmlBuilder.AppendHtml("</a>");
            }

            htmlBuilder.AppendHtml("</div>");

            return htmlBuilder;
        }

        public static IHtmlContent CreateButtonIfCan(this IHtmlHelper helper, string controller, string text = "Crear Nuevo")
        {
            var createPermission = $"{controller.ToLower()}.crear";
            var user = helper.ViewContext.HttpContext.User;

            if (!user.HasPermission(createPermission))
                return HtmlString.Empty;

            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml($"<a href=\"/{controller}/Create\" class=\"btn btn-success btn-sm\">");
            htmlBuilder.AppendHtml("<i class=\"bi bi-plus-circle me-1\"></i> ");
            htmlBuilder.AppendHtml(HtmlEncoder.Default.Encode(text));
            htmlBuilder.AppendHtml("</a>");

            return htmlBuilder;
        }
    }
}