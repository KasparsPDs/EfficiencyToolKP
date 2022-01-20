using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEWefficiencyTool.ModelsDB;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.IO;

namespace NEWefficiencyTool.Controllers
{
    public class EmployeeAvgEfficienciesController : Controller
    {
        private readonly EfficiencyToolNewContext _context;

        public EmployeeAvgEfficienciesController(EfficiencyToolNewContext context)
        {
            _context = context;
        }

        // GET: EmployeeAvgEfficiencies
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var employeeAvgEfficiencies = from eae in _context.EmployeeAvgEfficiencies
                                          select eae;
            // the fetched records are ordered in a descending order by their time period ending dates
            employeeAvgEfficiencies = employeeAvgEfficiencies.OrderByDescending(eae => eae.DateTo);
            var efficiencyToolNewContext = employeeAvgEfficiencies.Include(e => e.Employee);
            //var efficiencyToolNewContext = _context.EmployeeAvgEfficiencies.Include(e => e.Employee);
            return View(await efficiencyToolNewContext.ToListAsync());
        }

        // GET: EmployeeAvgEfficiencies/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeAvgEfficiency = await _context.EmployeeAvgEfficiencies
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.EmplEfficiencyId == id);
            if (employeeAvgEfficiency == null)
            {
                return NotFound();
            }

            return View(employeeAvgEfficiency);
        }

        [Authorize]
        public IActionResult CreateEfficienciesView()
        {
            return View();
        }

        [Authorize]
        public IActionResult WrongDates()
        {
            return View();
        }

        // Method for sending an employee work efficiency record to the employee's e-mail address
        [Authorize]
        public IActionResult SendEfficiencies(int id)
        {
            EmployeeAvgEfficiency employeeAvgEfficiency = new EmployeeAvgEfficiency();
            Employee employee = new Employee();

            // the employee efficiency record is obtained from the record id
            employeeAvgEfficiency = (from ef in _context.EmployeeAvgEfficiencies
                                     where ef.EmplEfficiencyId == id
                                     select ef).Single();
            // the employee is obtained from the record's employee id parameter
            employee = (from e in _context.Employees
                        where e.EmployeeId == employeeAvgEfficiency.EmployeeId
                        select e).Single();
            
            // the e-mail message details such as the sender and recipient are set up
            string to = employee.Email;
            string from = "kpefficiencytooltest@gmail.com";
            MailMessage message = new MailMessage(from, to);
            string mailBody = "Greetings!" 
                + Environment.NewLine
                + "Your work efficiency for the time period from "
                + employeeAvgEfficiency.DateFrom
                + " to " + employeeAvgEfficiency.DateTo
                + " is "
                + employeeAvgEfficiency.EfficiencyCoef;

            // the employee is either congratulated with an outstanding work efficiency for the time period
            // or encouraged to work harder
            if (employeeAvgEfficiency.EfficiencyCoef > 1)
            {
                mailBody = mailBody
                    + Environment.NewLine
                    + "Outstanding job! We hope you keep it up!";
            }
            else if (employeeAvgEfficiency.EfficiencyCoef < 1)
            {
                mailBody = mailBody
                    + Environment.NewLine
                    + "It could be improved, but we hope you get back on your feet soon!";
            }
            else 
            {
                mailBody = mailBody
                    + Environment.NewLine
                    + "Perfect! We hope you keep it up!";
            }

            mailBody = mailBody
                + Environment.NewLine
                + "KP Efficiency Tool";

            // the time period is attached to the e-mail message body and subject line
            // from the record's parameters DateFrom and DateTo and the possible
            // time fragments of the message are removed
            string mailSubject = "Your work efficiency for the time period from "
                + employeeAvgEfficiency.DateFrom
                + " to "
                + employeeAvgEfficiency.DateTo;
            mailSubject = mailSubject.Replace(" 00:00:00", " ");
            mailBody = mailBody.Replace(" 00:00:00", " ");

            // the mail subject line and body are added to the employee's email
            message.Subject = mailSubject;
            message.Body = mailBody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = false;

            // a connection with the gmail mail server is set up and established
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            System.Net.NetworkCredential credential = new System.Net.NetworkCredential(from, "parole123");
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = credential;

            // once the mail client and message are set up, the system atempts to send a mail message
            // to the recipient
            try
            {
                client.Send(message);
            }
            catch
            {
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // this method sends information about newly created employee average efficiencies to the current user's e-mail
        public void sendEfficienciesToUser(List<EmployeeAvgEfficiency> employeeAvgEfficiencies) 
        {
            Employee employee = new Employee();
            DateTime dateFrom;
            DateTime dateTo;
            bool firstRecord = true;
            string mailSubject = "";
            string mailBody;
            // the user's e-mail address is obtained through his user identity name
            string to = HttpContext.User.Identity.Name;
            string from = "kpefficiencytooltest@gmail.com";
            MailMessage message = new MailMessage(from, to);

            // the pdf document is created and set up
            PdfDocument document = new PdfDocument();
            List<object> data = new List<object>();
            string fileName = "";
            string fileString = "";
            document.PageSettings.Orientation = PdfPageOrientation.Portrait;
            document.PageSettings.Margins.All = 50;
            // a page is added to the pdf document on which the data can be displayed
            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;
            PdfGrid pdfGrid = new PdfGrid();
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);

            mailBody = "<h3>Greetings!</h3> <p>Here are your employee efficiencies for the time period from ";

            foreach (EmployeeAvgEfficiency employeeAvgEfficiency in employeeAvgEfficiencies)
            {
                if (firstRecord == true)
                {
                    dateFrom = employeeAvgEfficiency.DateFrom;
                    dateTo = employeeAvgEfficiency.DateTo;
                    firstRecord = false;
                    // for the first record the e-mail message subject line and body are set up as well as
                    // the upper text for the pdf document is created
                    mailSubject = "Your Employee Work Efficiencies for the time period from " + dateFrom.Date + " to " + dateTo.Date;
                    mailSubject = mailSubject.Replace(" 00:00:00", "");
                    // this e-mail message body contains html
                    mailBody = mailBody
                        + dateFrom.Date
                        + " to "
                        + dateTo.Date
                        + "</p> <table> <tr> <th>Name</th> <th>Efficiency Coefficient</th>";
                    fileString = "Employee Work Efficiencies for the time period from "
                        + dateFrom.Date 
                        + " to " 
                        + dateTo.Date;
                    fileString = fileString.Replace(" 00:00:00", "");
                    graphics.DrawString(fileString, font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(0, 0));
                    fileName = "KPEfficiency_Report_" 
                        + dateFrom.Date 
                        + "_" 
                        + dateTo.Date
                        + ".pdf";
                    fileName = fileName.Replace(" 00:00:00", "");
                }

                employee = (from e in _context.Employees
                            where e.EmployeeId == employeeAvgEfficiency.EmployeeId
                            select e).Single();
                // an html table is used to display data in the e-mail message
                mailBody = mailBody
                    + "<tr>"
                    + "<td>"
                    + employee.FirstLastName
                    + "</td>"
                    + "<td>"
                    + employeeAvgEfficiency.EfficiencyCoef
                    + "</td>"
                    + "</tr>";
                // the data from the record is added to an object list which will be used
                // to construct the table part of the pdf document
                data.Add(new { Employee = employee.FirstLastName, Efficiency = employeeAvgEfficiency.EfficiencyCoef});
            }

            mailBody = mailBody + "</table>";
            mailBody = mailBody.Replace(" 00:00:00", "");
            message.Subject = mailSubject;
            message.Body = mailBody;
            message.BodyEncoding = Encoding.UTF8;
            // this e-mail message body contains html so the parameter has to be set to true
            message.IsBodyHtml = true;

            // the object list is used to construct a table within the pdf document
            IEnumerable<object> dataTable = data;
            pdfGrid.DataSource = dataTable;
            pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 50));
            MemoryStream stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;
            document.Close(true);
            // the generated pdf document is added to the e-mail message
            message.Attachments.Add(new Attachment(stream, fileName));

            // a connection with the gmail mail server is set up and established
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            System.Net.NetworkCredential credential = new System.Net.NetworkCredential(from, "parole123");
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = credential;
            // once the mail client and message are set up, the system atempts to send a mail message
            // to the recipient
            try
            {
                client.Send(message);
            }
            catch
            {
                throw;
            }
        }

        [Authorize]
        public async Task<IActionResult> CreateEfficiencies([Bind("DateFrom,DateTo")]EmployeeAvgEfficiency employeeAvgEfficiency)
        {
            List<string> periodTaskIds = new List<string>();
            List<string> periodEmployeeIds = new List<string>();
            List<EmployeeAvgEfficiency> employeeAvgEfficiencyList = new List<EmployeeAvgEfficiency>();
            double periodTotalEmployeeEfficiency;
            int periodEmployeeTaskCount;
            DateTime dateFrom;
            DateTime dateTo;
            dateFrom = employeeAvgEfficiency.DateFrom;
            dateTo = employeeAvgEfficiency.DateTo;

            // before any records are created the system first determines if the user
            // inputted correct dates
            if (dateFrom > dateTo)
            {
                return RedirectToAction(nameof(WrongDates));
            }
            // the employees, who finished tasks within the time period, are obtained
            var employees = (from e in _context.TaskEmployeeEfficiencies
                            join t in _context.Tasks on e.TaskId equals t.TaskId
                            where t.Done == true
                            && t.DoneDate >= dateFrom
                            && t.DoneDate <= dateTo
                            select e.EmployeeId).Distinct();
            foreach (var employeeRow in employees)
            {
                periodEmployeeIds.Add(employeeRow);
            }
            // for every active employee their entire task efficiency data is collected
            foreach (var employee in periodEmployeeIds)
            { 
                periodTotalEmployeeEfficiency = 0;
                periodEmployeeTaskCount = 0;
                EmployeeAvgEfficiency insertEmployeeAvgEfficiency = new EmployeeAvgEfficiency();
                periodTotalEmployeeEfficiency = (from w in _context.TaskEmployeeEfficiencies
                                                   where w.EmployeeId == employee
                                                   join t in _context.Tasks on w.TaskId equals t.TaskId
                                                   where t.Done == true
                                                   && t.DoneDate >= dateFrom
                                                   && t.DoneDate <= dateTo
                                                   select w.EfficiencyCoef).Sum();
                periodEmployeeTaskCount = (from w in _context.TaskEmployeeEfficiencies
                                           where w.EmployeeId == employee
                                           join t in _context.Tasks on w.TaskId equals t.TaskId
                                           where t.Done == true
                                           && t.DoneDate >= dateFrom
                                           && t.DoneDate <= dateTo
                                           select w).Count();

                insertEmployeeAvgEfficiency.EmployeeId = employee;
                // an employee's average efficiency is calculated by dividing the sum of the total
                // efficiency records efficiency by the amount of employee's task efficiency records 
                insertEmployeeAvgEfficiency.EfficiencyCoef = periodTotalEmployeeEfficiency/periodEmployeeTaskCount;
                insertEmployeeAvgEfficiency.DateFrom = dateFrom;
                insertEmployeeAvgEfficiency.DateTo = dateTo;
                _context.Add(insertEmployeeAvgEfficiency);
                _context.SaveChanges();
                employeeAvgEfficiencyList.Add(insertEmployeeAvgEfficiency);
            }
            // a method call for sending the new data to the current user's e-mail address
            this.sendEfficienciesToUser(employeeAvgEfficiencyList);
            return RedirectToAction(nameof(Index));
        }

        // GET: EmployeeAvgEfficiencies/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["Employee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName");
            return View();
        }

        // POST: EmployeeAvgEfficiencies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("EmplEfficiencyId,EmployeeId,EfficiencyCoef,DateFrom,DateTo")] EmployeeAvgEfficiency employeeAvgEfficiency)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeAvgEfficiency);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Employee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", employeeAvgEfficiency.EmployeeId);
            return View(employeeAvgEfficiency);
        }

        // GET: EmployeeAvgEfficiencies/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeAvgEfficiency = await _context.EmployeeAvgEfficiencies.FindAsync(id);
            if (employeeAvgEfficiency == null)
            {
                return NotFound();
            }
            ViewData["Employee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", employeeAvgEfficiency.EmployeeId);
            return View(employeeAvgEfficiency);
        }

        // POST: EmployeeAvgEfficiencies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("EmplEfficiencyId,EmployeeId,EfficiencyCoef,DateFrom,DateTo")] EmployeeAvgEfficiency employeeAvgEfficiency)
        {
            if (id != employeeAvgEfficiency.EmplEfficiencyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeAvgEfficiency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeAvgEfficiencyExists(employeeAvgEfficiency.EmplEfficiencyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Employee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", employeeAvgEfficiency.EmployeeId);
            return View(employeeAvgEfficiency);
        }

        // GET: EmployeeAvgEfficiencies/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeAvgEfficiency = await _context.EmployeeAvgEfficiencies
                .Include(e => e.Employee)
                .FirstOrDefaultAsync(m => m.EmplEfficiencyId == id);
            if (employeeAvgEfficiency == null)
            {
                return NotFound();
            }

            return View(employeeAvgEfficiency);
        }

        // POST: EmployeeAvgEfficiencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeAvgEfficiency = await _context.EmployeeAvgEfficiencies.FindAsync(id);
            _context.EmployeeAvgEfficiencies.Remove(employeeAvgEfficiency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeAvgEfficiencyExists(int id)
        {
            return _context.EmployeeAvgEfficiencies.Any(e => e.EmplEfficiencyId == id);
        }
    }
}
