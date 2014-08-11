using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Mapping
{
    public class ReceivePackDataMap : EntityTypeConfiguration<ReceivePackData>
    {
        public ReceivePackDataMap()
        {
            ToTable("ReceivePackData");
            HasKey(m => m.PackId);
            Property(m => m.Data).IsMaxLength();
            Property(m => m.EntryTimestamp);
        }
    }
}