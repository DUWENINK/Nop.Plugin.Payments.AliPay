using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.AliPay.Domain;

namespace Nop.Plugin.Payments.AliPay.Data
{
    public partial class RefundInfoMap : NopEntityTypeConfiguration<RefundInfo>
    {

        public override void Configure(EntityTypeBuilder<RefundInfo> builder)
        {
            builder.ToTable("dbl_RefundInfo");
            builder.HasKey(x => x.Id);
            builder.Ignore(x => x.RefundStatus);
        }
    }
}
