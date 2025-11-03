using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeStores_HW03.Models.ViewModels
{
    public class MaintainViewModel
 
    {
        //Staff card
        public class StaffCard
        {
            public staffs StaffMember { get; set; }
            public List<string> LatestSoldProducts { get; set; } = new List<string>();
        }

        //Customer card
        public class CustomerCard
        {
            public customers CustomerMember { get; set; }
            public List<string> LatestPurchasedProducts { get; set; } = new List<string>();
        }

        //Product card
        public class ProductCard
        {
            public products ProductMember { get; set; }
            public string ImageUrl { get; set; }
        }

        //lists
        public List<staffs> AllStaff { get; set; } = new List<staffs>();
        public List<customers> AllCustomers { get; set; } = new List<customers>();
        public List<products> AllProducts { get; set; } = new List<products>();

        //Dropdowns
        public List<staffs> StaffList { get; set; } = new List<staffs>();
        public List<customers> CustomerList { get; set; } = new List<customers>();
        public List<products> ProductList { get; set; } = new List<products>();

        //selected staff, customer and product
        public StaffCard SelectedStaff { get; set; }
        public CustomerCard SelectedCustomer { get; set; }
        public ProductCard SelectedProduct { get; set; }
    }
}