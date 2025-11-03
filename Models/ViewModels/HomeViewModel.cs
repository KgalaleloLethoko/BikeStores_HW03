using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BikeStores_HW03.Models.ViewModels
{
    // ---- STAFF ----
    public class StaffCardVM
    {
        public staffs Staff { get; set; }
        public List<string> LatestSoldProducts { get; set; } = new List<string>();
    }

    // ---- CUSTOMER (card) ----
    public class CustomerCardVM
    {
        public customers Customer { get; set; }
        public List<string> LatestPurchasedProducts { get; set; } = new List<string>();
    }

    // ---- CUSTOMER (optional list item shape; keep if you later render a table) ----
    public class CustomerListItemVM
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
    }

    // ---- PRODUCT (list item) ----
    public class ProductListItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public short ModelYear { get; set; }
        public decimal ListPrice { get; set; }

        public string ImageUrl { get; set; }
    }

    //PRODUCT DETAIL
    public class ProductDetailVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public short ModelYear { get; set; }
        public decimal ListPrice { get; set; }
        public string ImageUrl { get; set; }
    }

    // ---- HOME INDEX VM ----
    public class HomeIndexVM
    {
        public StaffCardVM StaffCard { get; set; }
        public int StaffPage { get; set; }
        public int StaffTotal { get; set; }

        public CustomerCardVM CustomerCard { get; set; }
        public int CustomerPage { get; set; }
        public int CustomerTotal { get; set; }

        public ProductDetailVM SelectedProduct { get; set; }
        public int? SelectedProductId { get; set; }

        public List<CustomerListItemVM> Customers { get; set; } = new List<CustomerListItemVM>();
        public int CustomerTotalPages { get; set; }

        public List<ProductListItemVM> Products { get; set; } = new List<ProductListItemVM>();
        public int ProductPage { get; set; }
        public int ProductTotalPages { get; set; }

        public int? FilterBrandId { get; set; }
        public int? FilterCategoryId { get; set; }
    }
}