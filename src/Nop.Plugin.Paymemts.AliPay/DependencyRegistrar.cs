using System;
using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.AliPay.Data;
using Nop.Plugin.Payments.AliPay.Domain;
using Nop.Plugin.Payments.AliPay.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Payments.AliPay
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //data context
            builder.RegisterPluginDataContext<AliPayObjectContext>( "nop_object_context_alipay");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PaymentInfo>>()
                .As<IRepository<PaymentInfo>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_alipay"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<RefundInfo>>()
               .As<IRepository<RefundInfo>>()
               .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_alipay"))
               .InstancePerLifetimeScope();
            //注册支付记录服务
            builder.RegisterType<PaymentInfoService>().As<IPaymentInfoService>().InstancePerLifetimeScope();
            //注册退款记录服务
            builder.RegisterType<RefundInfoService>().As<IRefundInfoService>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order => 1;
    }
}
