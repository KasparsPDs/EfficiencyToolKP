using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NEWefficiencyTool.ModelsDB;

namespace NEWefficiencyTool.Controllers
{
    public class WorklogsController : Controller
    {
        private readonly EfficiencyToolNewContext _context;

        public WorklogsController(EfficiencyToolNewContext context)
        {
            _context = context;
        }

        // GET: Worklogs
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var worklogs = from w in _context.Worklogs
                           select w;
            worklogs = worklogs.OrderByDescending(w => w.WorklogDate);
            var efficiencyToolNewContext = worklogs.Include(w => w.Employee).Include(w => w.Task);
            //var efficiencyToolNewContext = _context.Worklogs.Include(w => w.Employee).Include(w => w.Task);
            return View(await efficiencyToolNewContext.ToListAsync());
        }

        // GET: Worklogs/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worklog = await _context.Worklogs
                .Include(w => w.Employee)
                .Include(w => w.Task)
                .FirstOrDefaultAsync(m => m.WorklogId == id);
            if (worklog == null)
            {
                return NotFound();
            }

            return View(worklog);
        }

        // GET: Worklogs/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName");
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId");
            return View();
        }

        // POST: Worklogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("WorklogId,EmployeeId,TaskId,WorklogDate,TimeSpent")] Worklog worklog)
        {
            // Finding the task for which this worklog has been created
            var task = (from o in _context.Tasks
                       where o.TaskId == worklog.TaskId
                       select o).Single();

            // Modifying the tasks elapsed time to reflect the added worklog
            task.ElapsedTime += worklog.TimeSpent;

            //Saving changes made to the worklog's task for them to take effect
            _context.SaveChanges();

            if (ModelState.IsValid)
            {
                _context.Add(worklog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            

            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", worklog.EmployeeId);
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", worklog.TaskId);
            return View(worklog);
        }

        // GET: Worklogs/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var worklog = await _context.Worklogs.FindAsync(id);
            
            if (worklog == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", worklog.EmployeeId);
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", worklog.TaskId);
            return View(worklog);
        }

        // POST: Worklogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("WorklogId,EmployeeId,TaskId,WorklogDate,TimeSpent")] Worklog worklog)
        {
            System.Diagnostics.Debug.WriteLine("worklog time tiesi pirms id != worklogid " + worklog.TimeSpent);
            if (id != worklog.WorklogId)
            {
                return NotFound();
            }
            System.Diagnostics.Debug.WriteLine("worklog time tiesi pirms modelState.isvalid " + worklog.TimeSpent);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(worklog);
                    await _context.SaveChangesAsync();
                    var task = (from o in _context.Tasks
                                where o.TaskId == worklog.TaskId
                                select o).Single();
                    // resetting the task's total elapsed time to zero
                    task.ElapsedTime = TimeSpan.Zero;
                    // calculating the task's total elapsed time from the existing worklogs for that task
                    var taskWorkLogs = from p in _context.Worklogs
                                       where p.TaskId == task.TaskId
                                       select p;
                    foreach (var workrow in taskWorkLogs)
                    {
                        task.ElapsedTime += workrow.TimeSpent;
                    }

                    // saving changes that were made to the task's total elapsed time
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorklogExists(worklog.WorklogId))
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
            System.Diagnostics.Debug.WriteLine("worklog time tiesi peec modelState.isvalid " + worklog.TimeSpent);
            
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", worklog.EmployeeId);
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", worklog.TaskId);
            return View(worklog);
        }

        // GET: Worklogs/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worklog = await _context.Worklogs
                .Include(w => w.Employee)
                .Include(w => w.Task)
                .FirstOrDefaultAsync(m => m.WorklogId == id);
            if (worklog == null)
            {
                return NotFound();
            }

            return View(worklog);
        }

        // POST: Worklogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var worklog = await _context.Worklogs.FindAsync(id);

            // Finding the task for which this worklog was originally created
            var task = (from o in _context.Tasks
                       where o.TaskId == worklog.TaskId
                       select o).Single();

            // Modifying the task's elapsed time to reflect the added worklog
            task.ElapsedTime -= worklog.TimeSpent;

            //Saving changes made to the task for them to take effect
            _context.SaveChanges();

            _context.Worklogs.Remove(worklog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorklogExists(int id)
        {
            return _context.Worklogs.Any(e => e.WorklogId == id);
        }
    }
}
