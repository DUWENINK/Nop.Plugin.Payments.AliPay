using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.AliPay
{
    public partial class RouteProvider : IRouteProvider
    {
        #region Methods

        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //支付通知路由
            routeBuilder.MapRoute("Nop.Plugin.Payments.AliPay.Notify",
                 "Plugins/AliPay/Notify",
                 new { controller = "AliPay", action = "Notify" }
            );

            //支付页面跳转同步通知页面
            routeBuilder.MapRoute("Nop.Plugin.Payments.AliPay.Return",
                 "Plugins/AliPay/Return",
                 new { controller = "AliPay", action = "Return" }
            );

            //退款通知路由
            routeBuilder.MapRoute("Nop.Payments.AliPay.RefundNotify",
              "Plugins/AliPay/RefundNotify",
              new { controller = "AliPay", action = "RefundNotify" }
            );
        }

        #endregion

        #region Properties

        public int Priority => 0;

        #endregion
    }
}
