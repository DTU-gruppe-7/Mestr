using Mestr.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mestr.Core.Interface
{
    public interface IClient
    {
        public Guid Uuid { get; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PostalAddress { get; set; }
        public string City { get; set; }
        public string? Cvr { get; set; }
        public DateTime InitDate { get; }
        public IList<IProject> Projects { get; set; }
        string GetFullAddress();
        bool IsBusinessClient();
    }
}
