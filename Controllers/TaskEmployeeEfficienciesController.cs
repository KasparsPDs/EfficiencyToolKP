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
    public class TaskEmployeeEfficienciesController : Controller
    {
        private readonly EfficiencyToolNewContext _context;

        public TaskEmployeeEfficienciesController(EfficiencyToolNewContext context)
        {
            _context = context;
        }

        // GET: TaskEmployeeEfficiencies
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var taskEmployeeEfficiencies = from tee in _context.TaskEmployeeEfficiencies
                                           select tee;
            taskEmployeeEfficiencies = taskEmployeeEfficiencies.OrderByDescending(tee => tee.TaskId);
            var efficiencyToolNewContext = taskEmployeeEfficiencies.Include(t => t.Employee).Include(t => t.Task);
            //var efficiencyToolNewContext = _context.TaskEmployeeEfficiencies.Include(t => t.Employee).Include(t => t.Task);
            return View(await efficiencyToolNewContext.ToListAsync());
        }

        // GET: TaskEmployeeEfficiencies/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskEmployeeEfficiency = await _context.TaskEmployeeEfficiencies
                .Include(t => t.Employee)
                .Include(t => t.Task)
                .FirstOrDefaultAsync(m => m.EfficiencyId == id);
            if (taskEmployeeEfficiency == null)
            {
                return NotFound();
            }

            return View(taskEmployeeEfficiency);
        }

        // method for calculating and creating TaskEmployeeEfficiency database table records
        [Authorize]
        public IActionResult CreateEfficiencies(TaskEmployeeEfficiency taskEmployeeEfficiency)
        {
            List<string> taskIds = new List<string>(); // a complete list of all tasks
            HashSet<string> toRemoveTaskIds = new HashSet<string>(); // a list of tasks for which the employee's work efficiency has been calculated
            List<string> taskEmployeeIds = new List<string>();
            var tasks = from t in _context.Tasks
                        where t.Done == true
                        select t;
            foreach (var row in tasks)
            {
                taskIds.Add(row.TaskId);
            }
            foreach (string taskId in taskIds)
            {
                if (_context.TaskEmployeeEfficiencies.Any(tsk => tsk.TaskId == taskId))
                {
                    toRemoveTaskIds.Add(taskId);
                }
            }
            // a list of all tasks for which a task efficiency record has not been created is obtained
            taskIds.RemoveAll(toRemoveTaskIds.Contains);
            if (taskIds.Count != 0)
            {
                // each task is processed separately
                foreach (string taskId in taskIds)
                {
                    // at first the algorithm needs to determine how many people worked on the task
                    int taskEmployeeCount = (from te in _context.Worklogs
                                             where te.TaskId == taskId
                                             select te.EmployeeId).Distinct().Count();
                    if (taskEmployeeCount == 1)
                    {
                        // if the task was worked on by just one person the task efficiency is obtained by dividing estimated time and elapsed time
                        var task = (from t in _context.Tasks
                                    where t.TaskId == taskId
                                    select t).Single();
                        TaskEmployeeEfficiency insertTaskEmployeeEfficiency = new TaskEmployeeEfficiency();
                        insertTaskEmployeeEfficiency.TaskId = taskId;
                        insertTaskEmployeeEfficiency.EmployeeId = task.AsigneeId;
                        insertTaskEmployeeEfficiency.EfficiencyCoef = (double)(task.EstimatedTime/task.ElapsedTime);
                        _context.Add(insertTaskEmployeeEfficiency);
                        _context.SaveChanges();
                    }
                    else if (taskEmployeeCount > 1)
                    {
                        // if the task was worked on by two people then the task efficiency has to be calculated
                        // for each employee considering their time spent on the task
                        TimeSpan reporterTimeSpent = TimeSpan.Zero;
                        TimeSpan asigneeTimeSpent = TimeSpan.Zero;
                        TimeSpan totalRemainingTime = TimeSpan.Zero;
                        TimeSpan reporterEstimatedTime = TimeSpan.Zero;
                        string reporterId;
                        string asigneeId;
                        double reporterTimeRatio;
                        var task = (from t in _context.Tasks
                                    where t.TaskId == taskId
                                    select t).Single();
                        totalRemainingTime = (TimeSpan)task.EstimatedTime;
                        reporterId = task.ReporterId;
                        var reporterTimeSpentWorklogs = from w in _context.Worklogs
                                                      where w.TaskId == taskId
                                                      && w.EmployeeId == reporterId
                                                      select w.TimeSpent;
                        foreach (var row in reporterTimeSpentWorklogs)
                        {
                            reporterTimeSpent += (TimeSpan)row;
                        }
                        // the reporter's estimated time is obtained by comparing the reporter's time spent
                        // on the task in comparison to the total time spent on the task
                        reporterTimeRatio = (double)(reporterTimeSpent / task.ElapsedTime);
                        reporterEstimatedTime = (TimeSpan)(reporterTimeRatio * task.EstimatedTime);
                        TaskEmployeeEfficiency insertReporterEfficiency = new TaskEmployeeEfficiency();
                        insertReporterEfficiency.EmployeeId = reporterId;
                        insertReporterEfficiency.TaskId = taskId;
                        insertReporterEfficiency.EfficiencyCoef = reporterEstimatedTime / reporterTimeSpent;
                        _context.Add(insertReporterEfficiency);
                        _context.SaveChanges();
                        // for the asignee the estimated time is the difference between the 
                        // total estimated time and the time the reporter spent on the task
                        totalRemainingTime = (TimeSpan)(task.EstimatedTime - reporterTimeSpent);
                        if (task.ReporterId == task.AsigneeId)
                        {
                            asigneeId = (from w in _context.Worklogs
                                         where w.TaskId == taskId
                                         && w.EmployeeId != reporterId
                                         select w.EmployeeId).First();
                        }
                        else
                        {
                            asigneeId = task.AsigneeId;
                        }
                        asigneeTimeSpent = (TimeSpan)(task.ElapsedTime - reporterTimeSpent);
                        TaskEmployeeEfficiency insertAsigneeEfficiency = new TaskEmployeeEfficiency();
                        insertAsigneeEfficiency.EmployeeId = asigneeId;
                        insertAsigneeEfficiency.TaskId = taskId;
                        insertAsigneeEfficiency.EfficiencyCoef = totalRemainingTime / asigneeTimeSpent;
                        _context.Add(insertAsigneeEfficiency);
                        _context.SaveChanges();
                        // in total for this scenario two new TaskEmployeeEfficiency database table records are created
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: TaskEmployeeEfficiencies/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName");
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId");
            return View();
        }

        // POST: TaskEmployeeEfficiencies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("EfficiencyId,EmployeeId,TaskId,EfficiencyCoef")] TaskEmployeeEfficiency taskEmployeeEfficiency)
        {
            if (ModelState.IsValid)
            {
                _context.Add(taskEmployeeEfficiency);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", taskEmployeeEfficiency.EmployeeId);
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", taskEmployeeEfficiency.TaskId);
            return View(taskEmployeeEfficiency);
        }

        // GET: TaskEmployeeEfficiencies/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskEmployeeEfficiency = await _context.TaskEmployeeEfficiencies.FindAsync(id);
            if (taskEmployeeEfficiency == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", taskEmployeeEfficiency.EmployeeId);
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", taskEmployeeEfficiency.TaskId);
            return View(taskEmployeeEfficiency);
        }

        // POST: TaskEmployeeEfficiencies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("EfficiencyId,EmployeeId,TaskId,EfficiencyCoef")] TaskEmployeeEfficiency taskEmployeeEfficiency)
        {
            if (id != taskEmployeeEfficiency.EfficiencyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskEmployeeEfficiency);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskEmployeeEfficiencyExists(taskEmployeeEfficiency.EfficiencyId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", taskEmployeeEfficiency.EmployeeId);
            ViewData["TaskId"] = new SelectList(_context.Tasks, "TaskId", "TaskId", taskEmployeeEfficiency.TaskId);
            return View(taskEmployeeEfficiency);
        }

        // GET: TaskEmployeeEfficiencies/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskEmployeeEfficiency = await _context.TaskEmployeeEfficiencies
                .Include(t => t.Employee)
                .Include(t => t.Task)
                .FirstOrDefaultAsync(m => m.EfficiencyId == id);
            if (taskEmployeeEfficiency == null)
            {
                return NotFound();
            }

            return View(taskEmployeeEfficiency);
        }

        // POST: TaskEmployeeEfficiencies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskEmployeeEfficiency = await _context.TaskEmployeeEfficiencies.FindAsync(id);
            _context.TaskEmployeeEfficiencies.Remove(taskEmployeeEfficiency);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskEmployeeEfficiencyExists(int id)
        {
            return _context.TaskEmployeeEfficiencies.Any(e => e.EfficiencyId == id);
        }
    }
}
