using System;
using System.Collections.Generic;
using System.Text;

namespace UsedCarsPrice.Common.Models
{
    public class DropdownItem
    {
        public DropdownItem(string id, string value)
        {
            Id = id;
            Value = value;
        }
        public string Id { get; set; }
        public string Value { get; set; }
    }

}
