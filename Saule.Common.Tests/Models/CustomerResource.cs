﻿namespace Saule.Common.Tests.Models
{
    public class CustomerResource : ApiResource
    {
        public CustomerResource()
        {
            Attribute(nameof(Customer.Name));
        }
    }
}
