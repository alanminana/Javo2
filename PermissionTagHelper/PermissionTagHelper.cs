// TagHelpers/PermissionTagHelper.cs
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Javo2.Helpers;

namespace Javo2.TagHelpers
{
    [HtmlTargetElement(Attributes = "require-permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("require-permission")]
        public string Permission { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext.User;
            if (!user.HasPermission(Permission))
            {
                output.SuppressOutput();
            }
        }
    }
}