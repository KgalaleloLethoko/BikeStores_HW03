using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BikeStores_HW03.Models;
using BikeStores_HW03.Models.ViewModels;

namespace BikeStores_HW03.Controllers
{
    public class MaintainController : Controller
    {
        private readonly BikeStoresEntities1 _db = new BikeStoresEntities1();


        public ActionResult Index(int? staffId, int? customerId, int? productId)
        {
            var vm = new MaintainViewModel();

            // Populate dropdowns
            vm.StaffList = _db.staffs.ToList();
            vm.CustomerList = _db.customers.ToList();
            vm.ProductList = _db.products.ToList();

            // Selected Staff
            if (staffId.HasValue)
            {
                var s = _db.staffs.Find(staffId.Value);
                if (s != null)
                {
                    vm.SelectedStaff = new MaintainViewModel.StaffCard
                    {
                        StaffMember = s,
                        LatestSoldProducts = _db.orders
                            .Where(o => o.staff_id == s.staff_id)
                            .SelectMany(o => o.order_items)
                            .Select(oi => oi.products.product_name)
                            .Distinct()
                            .OrderByDescending(x => x)
                            .Take(5)
                            .ToList()
                    };
                }
            }

            // Selected Customer
            if (customerId.HasValue)
            {
                var c = _db.customers.Find(customerId.Value);
                if (c != null)
                {
                    vm.SelectedCustomer = new MaintainViewModel.CustomerCard
                    {
                        CustomerMember = c,
                        LatestPurchasedProducts = _db.orders
                            .Where(o => o.customer_id == c.customer_id)
                            .SelectMany(o => o.order_items)
                            .Select(oi => oi.products.product_name)
                            .Distinct()
                            .OrderByDescending(x => x)
                            .Take(5)
                            .ToList()
                    };
                }
            }

            // Selected Product
            if (productId.HasValue)
            {
                var product = _db.products.Find(productId.Value);
                if (product != null)
                {
                    var imageFolder = Server.MapPath("~/Content/images/");
                    string imageToUse = null;

                    // Check if the DB image exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        var path = Path.Combine(imageFolder, product.ImageUrl);
                        if (System.IO.File.Exists(path))
                            imageToUse = product.ImageUrl;
                    }

                    // Fallback: pick a random image from folder
                    if (string.IsNullOrEmpty(imageToUse))
                    {
                        var files = Directory.GetFiles(imageFolder)
                                             .Select(f => Path.GetFileName(f))
                                             .ToList();

                        if (files.Any())
                        {
                            var rand = new Random();
                            imageToUse = files[rand.Next(files.Count)];
                        }
                        else
                        {
                            imageToUse = "default.jpg";
                        }
                    }

                    vm.SelectedProduct = new MaintainViewModel.ProductCard
                    {
                        ProductMember = product,
                        ImageUrl = Url.Content("~/Content/images/" + imageToUse)
                    };
                }
            }

            return View(vm);
        }

            //methods for my staff card
            [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStaff(int id, staffs updatedStaff)
        {
            var s = _db.staffs.Find(id);
            if (s == null) return HttpNotFound();

            s.first_name = updatedStaff.first_name;
            s.last_name = updatedStaff.last_name;
            s.email = updatedStaff.email;
            s.phone = updatedStaff.phone;
            s.active = updatedStaff.active;

            _db.SaveChanges();
            TempData["Success"] = "Staff updated successfully.";
            return RedirectToAction("Index", new { staffId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteStaff(int id)
        {
            var s = _db.staffs.Find(id);
            if (s == null) return HttpNotFound();

            _db.staffs.Remove(s);
            _db.SaveChanges();
            TempData["Success"] = "Staff deleted successfully.";
            return RedirectToAction("Index");
        }

        //methods for my customer card
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomer(int id, customers updatedCustomer)
        {
            var c = _db.customers.Find(id);
            if (c == null) return HttpNotFound();

            c.first_name = updatedCustomer.first_name;
            c.last_name = updatedCustomer.last_name;
            c.email = updatedCustomer.email;
            c.phone = updatedCustomer.phone;
            c.city = updatedCustomer.city;

            _db.SaveChanges();
            TempData["Success"] = "Customer updated successfully.";
            return RedirectToAction("Index", new { customerId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCustomer(int id)
        {
            var c = _db.customers.Find(id);
            if (c == null) return HttpNotFound();

            _db.customers.Remove(c);
            _db.SaveChanges();
            TempData["Success"] = "Customer deleted successfully.";
            return RedirectToAction("Index");
        }

      // methods for my product card
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(int id, products updatedProduct)
   
            {
                if (!ModelState.IsValid)
                {
                    return View(updatedProduct);
                }

                var product = _db.products.Find(updatedProduct.product_id);
                if (product == null) return HttpNotFound();

                product.product_name = updatedProduct.product_name;
                product.brand_id = updatedProduct.brand_id;
                product.category_id = updatedProduct.category_id;
                product.model_year = updatedProduct.model_year;
                product.list_price = updatedProduct.list_price;
        

                _db.SaveChanges();
                TempData["Success"] = "Product saved successfully.";

                return RedirectToAction("Index");
            }

            [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            var p = _db.products.Find(id);
            if (p == null) return HttpNotFound();

            _db.products.Remove(p);
            _db.SaveChanges();
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}