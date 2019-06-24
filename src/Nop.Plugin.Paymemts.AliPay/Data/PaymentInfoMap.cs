using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.AliPay.Domain;

namespace Nop.Plugin.Payments.AliPay.Data
{
    public partial class PaymentInfoMap : NopEntityTypeConfiguration<PaymentInfo>
    {


        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PaymentInfo> builder)
        {
            builder.ToTable("dbl_PaymentInfo");
            builder.HasKey(x => x.Id);
        }
    }
}
