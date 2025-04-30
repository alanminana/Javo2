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
        public static IHtmlContent ActionButtons(this IHtmlHelper helper, int id, string controller, string action = "", string title = "", string icon = "")
        {
            var controllerLower = controller.ToLower();
            bool canView = helper.ViewContext.HttpContext.User.HasPermission($"{controllerLower}.ver");
            bool canEdit = helper.ViewContext.HttpContext.User.HasPermission($"{controllerLower}.editar");
            bool canDelete = helper.ViewContext.HttpContext.User.HasPermission($"{controllerLower}.eliminar");

            // Determinar acción y texto
            var viewAction = string.IsNullOrEmpty(action) ? "Details" : action;
            var viewIcon = string.IsNullOrEmpty(icon) ? "eye" : icon;
            var viewTitle = string.IsNullOrEmpty(title) ? "Ver detalles" : title;

            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml("<div class=\"btn-group\" role=\"group\">");

            if (canEdit)
            {
                htmlBuilder.AppendHtml($"<a href=\"/{controller}/Edit/{id}\" class=\"btn btn-sm btn-outline-primary\" title=\"Editar\">");
                htmlBuilder.AppendHtml("<i class=\"bi bi-pencil-square\"></i>");
                htmlBuilder.AppendHtml("</a>");
            }

            if (canView)
            {
                htmlBuilder.AppendHtml($"<a href=\"/{controller}/{viewAction}/{id}\" class=\"btn btn-sm btn-outline-info\" title=\"{viewTitle}\">");
                htmlBuilder.AppendHtml($"<i class=\"bi bi-{viewIcon}\"></i>");
                htmlBuilder.AppendHtml("</a>");
            }

            if (canDelete)
            {
                htmlBuilder.AppendHtml($"<a href=\"/{controller}/Delete/{id}\" class=\"btn btn-sm btn-outline-danger\" title=\"Eliminar\">");
                htmlBuilder.AppendHtml("<i class=\"bi bi-trash\"></i>");
                htmlBuilder.AppendHtml("</a>");
            }

            htmlBuilder.AppendHtml("</div>");
            return htmlBuilder;
        }
        public static IHtmlContent CreateButton(this IHtmlHelper helper, string controller, string text = "Crear")
        {
            string permissionCode = $"{controller.ToLower()}.crear";
            if (!helper.ViewContext.HttpContext.User.HasPermission(permissionCode))
                return HtmlString.Empty;

            var builder = new HtmlContentBuilder();
            builder.AppendHtml($"<a href=\"/{controller}/Create\" class=\"btn btn-success btn-sm\">");
            builder.AppendHtml("<i class=\"bi bi-plus-circle me-1\"></i> ");
            builder.AppendHtml(text);
            builder.AppendHtml("</a>");

            return builder;
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