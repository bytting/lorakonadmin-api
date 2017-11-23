using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Security;

namespace lorakonadmin_api
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {            
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {                
                string authString = actionContext.Request.Headers.Authorization.Parameter;
                string origString = Encoding.UTF8.GetString(Convert.FromBase64String(authString));

                string[] items = origString.Split(new char[] { ':' });
                if (items.Length != 2)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                else
                {
                    string username = items[0];
                    string password = items[1];

                    if (!Membership.ValidateUser(username, password))
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                }
            }

            base.OnAuthorization(actionContext);
        }
    }
}