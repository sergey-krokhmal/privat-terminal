//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PrivatTerminalApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Advertiser
    {
        public Advertiser()
        {
            this.Pays = new HashSet<Pay>();
        }
    
        public int Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Sname { get; set; }
        public Nullable<int> AgencyId { get; set; }
        public Nullable<System.Guid> MembershipId { get; set; }
        public string Phone { get; set; }
        public string Patronymic { get; set; }
        public string Icq { get; set; }
        public string uniqueNumber { get; set; }
        public int PacketLimit { get; set; }
    
        public virtual ICollection<Pay> Pays { get; set; }
    }
}
