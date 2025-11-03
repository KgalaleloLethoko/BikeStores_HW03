using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BikeStores_HW03.Models;
using BikeStores_HW03.Models.ViewModels;
using System.Data.Entity;
using System.IO;



namespace BikeStores_HW03.Controllers
{
    public class HomeController : Controller
    {
        private readonly BikeStoresEntities1 _db = new BikeStoresEntities1();

        public async Task<ActionResult> Index(
            int staffPage = 1,
            int custPage = 1,
            int prodPage = 1,
            int? brandId = null,
            int? categoryId = null,
            int? productId = null)
        {
            var vm = new HomeIndexVM
            {
                StaffPage = staffPage < 1 ? 1 : staffPage,
                CustomerPage = custPage < 1 ? 1 : custPage,
                ProductPage = prodPage < 1 ? 1 : prodPage,
                FilterBrandId = brandId,
                FilterCategoryId = categoryId,
                SelectedProductId = productId
            };

            //staff
            var staffIds = await _db.staffs.AsNoTracking().OrderBy(s => s.staff_id)
                                .Select(s => s.staff_id).ToListAsync();
            vm.StaffTotal = staffIds.Count;

            if (vm.StaffTotal > 0)
            {
                var sIdx = Math.Min(vm.StaffPage, vm.StaffTotal) - 1;
                var currentStaffId = staffIds[sIdx];

                var staff = await _db.staffs.AsNoTracking()
                                .FirstOrDefaultAsync(s => s.staff_id == currentStaffId);

                var latestSold = await _db.order_items.AsNoTracking()
                                    .Where(oi => oi.orders.staff_id == currentStaffId)
                                    .OrderByDescending(oi => oi.orders.order_date)
                                    .ThenByDescending(oi => oi.item_id)
                                    .Select(oi => oi.products.product_name)
                                    .Distinct()
                                    .Take(5)
                                    .ToListAsync();

                vm.StaffCard = new StaffCardVM
                {
                    Staff = staff,
                    LatestSoldProducts = latestSold
                };
            }

            //customers
            var customerIds = await _db.customers.AsNoTracking().OrderBy(c => c.customer_id)
                                    .Select(c => c.customer_id).ToListAsync();
            vm.CustomerTotal = customerIds.Count;

            if (vm.CustomerTotal > 0)
            {
                var cIdx = Math.Min(vm.CustomerPage, vm.CustomerTotal) - 1;
                var currentCustomerId = customerIds[cIdx];

                var cust = await _db.customers.AsNoTracking()
                                .FirstOrDefaultAsync(c => c.customer_id == currentCustomerId);

                var latestPurchased = await _db.order_items.AsNoTracking()
                                        .Where(oi => oi.orders.customer_id == currentCustomerId)
                                        .OrderByDescending(oi => oi.orders.order_date)
                                        .ThenByDescending(oi => oi.item_id)
                                        .Select(oi => oi.products.product_name)
                                        .Distinct()
                                        .Take(5)
                                        .ToListAsync();

                vm.CustomerCard = new CustomerCardVM
                {
                    Customer = cust,
                    LatestPurchasedProducts = latestPurchased
                };
            }

            //products
            var prodQuery = _db.products.AsNoTracking()
                                .Include(p => p.brands)
                                .Include(p => p.categories)
                                .OrderBy(p => p.product_id)
                                .AsQueryable();

            if (brandId.HasValue) prodQuery = prodQuery.Where(p => p.brand_id == brandId.Value);
            if (categoryId.HasValue) prodQuery = prodQuery.Where(p => p.category_id == categoryId.Value);

            var prodCount = await prodQuery.CountAsync();
            const int ProductPageSize = 6;
            vm.ProductTotalPages = (int)Math.Ceiling(prodCount / (double)ProductPageSize);

            vm.Products = await prodQuery
                .Skip((vm.ProductPage - 1) * ProductPageSize)
                .Take(ProductPageSize)
                .Select(p => new ProductListItemVM
                {
                    ProductId = p.product_id,
                    ProductName = p.product_name,
                    BrandName = p.brands.brand_name,
                    CategoryName = p.categories.category_name,
                    ModelYear = p.model_year,
                    ListPrice = p.list_price,
                    ImageUrl = "" // placeholder, will assign below
                })
                .ToListAsync();
            var allImages = Directory.Exists(Server.MapPath("~/Content/images/"))
                                ? Directory.GetFiles(Server.MapPath("~/Content/images/"))
                                    .Select(Path.GetFileName)
                                    .ToList()
                                : new List<string>();

            var randGen = new Random();
            foreach (var product in vm.Products)
            {
                product.ImageUrl = allImages.Any()
                    ? "/Content/images/" + allImages[randGen.Next(allImages.Count)]
                    : "/Content/images/placeholder.jpg";
            }

            if (productId.HasValue)
            {
                var sel = await _db.products.AsNoTracking()
                    .Include(p => p.brands)
                    .Include(p => p.categories)
                    .Where(p => p.product_id == productId.Value)
                    .Select(p => new ProductDetailVM
                    {
                        ProductId = p.product_id,
                        ProductName = p.product_name,
                        BrandName = p.brands.brand_name,
                        CategoryName = p.categories.category_name,
                        ModelYear = p.model_year,
                        ListPrice = p.list_price
                    })
                    .FirstOrDefaultAsync();

                sel.ImageUrl = allImages.Any()
                    ? "/Content/images/" + allImages[randGen.Next(allImages.Count)]
                    : "/Content/images/placeholder.jpg";

                vm.SelectedProduct = sel;
            }

            //dropdowns in producvts
            ViewBag.ProductList = new SelectList(
                await _db.products.AsNoTracking()
                    .OrderBy(p => p.product_name)
                    .Select(p => new { p.product_id, p.product_name })
                    .ToListAsync(),
                "product_id", "product_name", productId);

            ViewBag.BrandList = new SelectList(
                await _db.brands.AsNoTracking().OrderBy(b => b.brand_name).ToListAsync(),
                "brand_id", "brand_name", brandId);

            ViewBag.CategoryList = new SelectList(
                await _db.categories.AsNoTracking().OrderBy(c => c.category_name).ToListAsync(),
                "category_id", "category_name", categoryId);

            ViewBag.Success = TempData["Success"];
            return View(vm);
        }

        [HttpGet]
        public ActionResult CreateCustomer()
        {
            return View(new customers());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCustomer([Bind(Include =
           "first_name,last_name,phone,email,street,city,state,zip_code")] customers model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.customers.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Customer created.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
