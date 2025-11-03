using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BikeStores_HW03.Models;
using BikeStores_HW03.Models.ViewModels;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace BikeStores_HW03.Controllers
{
    public class ReportsController : Controller
    {
        private readonly BikeStoresEntities1 _db = new BikeStoresEntities1();

        public ActionResult Index()
        {
            var model = new ReportViewModel
            {
                Labels = new List<string>(),
                Values = new List<int>(),
                SavedReports = GetSavedReports()
            };
            return View(model);
        }

        //Staff Performance Report
        [HttpPost]
        public ActionResult GenerateStaffPerformanceReport()
        {
            var data = _db.orders
               .Where(o => o.staffs != null) // make sure staff exists
               .GroupBy(o => o.staff_id)
               .Select(g => new
               {
                  StaffName = g.FirstOrDefault().staffs.first_name + " " + g.FirstOrDefault().staffs.last_name,
                  SalesCount = g.Count()
               })
              .ToList();

            var model = new ReportViewModel
            {
                ReportTitle = "Staff Performance Ranking",
                Labels = data.Select(d => d.StaffName).ToList(),
                Values = data.Select(d => d.SalesCount).ToList(),
                ReportType = "bar",
                SavedReports = GetSavedReports()
            };
            return View("Index", model);
        }

        //Current Sales Report
        [HttpPost]
        public ActionResult GenerateCurrentSalesReport()
        {
            var data = _db.order_items
           .Where(oi => oi.products != null) // 
            .GroupBy(oi => oi.product_id)   // use the navigation property to get product name
            .Select(g => new
               {
               ProductName = g.FirstOrDefault().products.product_name,
               SalesCount = g.Count() // how many times this product was sold
                })
             .ToList();
            var model = new ReportViewModel
            {
                ReportTitle = "Current Sales Report",
                Labels = data.Select(d => d.ProductName).ToList(),
                Values = data.Select(d => d.SalesCount).ToList(),
                ReportType = "bar",
                SavedReports = GetSavedReports()
            };
            return View("Index", model);
        }

        //Stock Items Report 
        [HttpPost]
        public ActionResult GenerateStockItemsReport()
        {
            var data = _db.products
                .Where(p => !_db.order_items.Any(oi => oi.product_id == p.product_id)) // products never sold
                .Select(p => new
                {
                    ProductName = p.product_name,
                    Stock = p.stocks.Any() ? p.stocks.Sum(s => s.quantity) : 0 // sum all stock entries
                })
                .ToList();

            var model = new ReportViewModel
            {
                ReportTitle = "Stock Items Report",
                Labels = data.Select(d => d.ProductName).ToList(),
                Values = data.Select(d => d.Stock ?? 0).ToList(),
                ReportType = "pie",
                SavedReports = GetSavedReports()
            };
            return View("Index", model);
        }

        //Order History Report
        [HttpPost]
        public ActionResult GenerateOrderHistoryReport()
        {
            var data = _db.orders
                .GroupBy(o => o.customers.first_name + " " + o.customers.last_name)
                .Select(g => new
                {
                    CustomerName = g.Key,
                    OrderCount = g.Count()
                }).ToList();

            var model = new ReportViewModel
            {
                ReportTitle = "Order History Report",
                Labels = data.Select(d => d.CustomerName).ToList(),
                Values = data.Select(d => d.OrderCount).ToList(),
                ReportType = "bar",
                SavedReports = GetSavedReports()
            };
            return View("Index", model);
        }

        //Popular Products Report
        [HttpPost]
        public ActionResult GeneratePopularProductsReport()
        {
            var data = _db.order_items
                .Where(oi => oi.products != null) 
                .GroupBy(oi => oi.products.product_name) 
                .Select(g => new
                {
                    ProductName = g.Key,
                    Count = g.Count() // no of time customers ordered product
                })
                .OrderByDescending(x => x.Count)
                .ToList();


            var model = new ReportViewModel
            {
                ReportTitle = "Popular Products Report",
                Labels = data.Select(d => d.ProductName).ToList(),
                Values = data.Select(d => d.Count).ToList(),
                ReportType = "bar",
                SavedReports = GetSavedReports()
            };
            return View("Index", model);
        }

        //Save Report after generating
   
        [HttpPost]
        public ActionResult SaveReport(ReportViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FileName) || string.IsNullOrWhiteSpace(model.FileType))
            {
                ModelState.AddModelError("", "Please enter a filename and select a file type.");
                model.SavedReports = GetSavedReports();
                return View("Index", model);
            }

            if (model.Labels == null)
                model.Labels = new List<string>();
            if (model.Values == null)
                model.Values = new List<int>();

            string folderPath = Server.MapPath("~/App_Data/Reports/");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, model.FileName);

            // TXT
            if (model.FileType.ToLower() == "txt")
            {
                filePath += ".txt";
                var reportContent = $"{model.ReportTitle}\n{model.Description}\n\n";
                for (int i = 0; i < model.Labels.Count; i++)
                {
                    var value = i < model.Values.Count ? model.Values[i] : 0;
                    reportContent += $"{model.Labels[i]}: {value}\n";
                }
                System.IO.File.WriteAllText(filePath, reportContent);
            }
            // Excel
            else if (model.FileType.ToLower() == "excel")
            {
                filePath += ".xlsx";

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add("Report");
                    ws.Cells[1, 1].Value = model.ReportTitle;
                    ws.Cells[2, 1].Value = model.Description;

                    ws.Cells[4, 1].Value = "Label";
                    ws.Cells[4, 2].Value = "Value";

                    for (int i = 0; i < model.Labels.Count; i++)
                    {
                        ws.Cells[i + 5, 1].Value = model.Labels[i];
                        ws.Cells[i + 5, 2].Value = i < model.Values.Count ? model.Values[i] : 0;
                    }

                    package.SaveAs(new System.IO.FileInfo(filePath));
                }
            }
            // PDF
            else if (model.FileType.ToLower() == "pdf")
            {
                filePath += ".pdf";
                using (var doc = new iTextSharp.text.Document())
                {
                    iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new System.IO.FileStream(filePath, System.IO.FileMode.Create));
                    doc.Open();
                    doc.Add(new iTextSharp.text.Paragraph(model.ReportTitle));
                    doc.Add(new iTextSharp.text.Paragraph(model.Description));
                    doc.Add(new iTextSharp.text.Paragraph("\n"));

                    for (int i = 0; i < model.Labels.Count; i++)
                    {
                        var value = i < model.Values.Count ? model.Values[i] : 0;
                        doc.Add(new iTextSharp.text.Paragraph($"{model.Labels[i]}: {value}"));
                    }

                    doc.Close();
                }
            }
            else
            {
                ModelState.AddModelError("", "Unsupported file type.");
                model.SavedReports = GetSavedReports();
                return View("Index", model);
            }

            TempData["Message"] = $"{model.FileType.ToUpper()} report saved successfully!";
            return RedirectToAction("Index");
        }


        //Download Report 
        public ActionResult Download(string fileName)
        {
            string path = Server.MapPath("~/App_Data/Reports/" + fileName);
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        //Delete Report 
        public ActionResult Delete(string fileName)
        {
            string path = Server.MapPath("~/App_Data/Reports/" + fileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            return RedirectToAction("Index");
        }

        //be able to get saved reports
        private List<string> GetSavedReports()
        {
            string folderPath = Server.MapPath("~/App_Data/Reports/");
            if (!Directory.Exists(folderPath)) return new List<string>();

            return Directory.GetFiles(folderPath)
                .Select(Path.GetFileName)
                .ToList();
        }
    }
}