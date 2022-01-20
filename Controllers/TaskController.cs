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
    public class TaskController : Controller
    {
        private readonly EfficiencyToolNewContext _context;

        public TaskController(EfficiencyToolNewContext context)
        {
            _context = context;
        }

        // GET: Task
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var tasks = from t in _context.Tasks
                        select t;
            // orders tasks into a descending order by their task key
            tasks = tasks.OrderByDescending(t => t.TaskId);
            var efficiencyToolNewContext = tasks.Include(t => t.Asignee).Include(t => t.ProjectKeyNavigation).Include(t => t.Reporter);
            //var efficiencyToolNewContext = _context.Tasks.Include(t => t.Asignee).Include(t => t.ProjectKeyNavigation).Include(t => t.Reporter);
            return View(await efficiencyToolNewContext.ToListAsync());
        }

        // GET: Task/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Asignee)
                .Include(t => t.ProjectKeyNavigation)
                .Include(t => t.Reporter)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Task/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["Asignee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName");
            ViewData["ProjectKey"] = new SelectList(_context.Projects, "ProjectKey", "ProjectKey");
            ViewData["Reporter"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName");
            return View();
        }

        // POST: Task/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("TaskId,TaskName,ReporterId,AsigneeId,ProjectKey,EstimatedTime,ElapsedTime,Done,DoneDate")] ModelsDB.Task task)
        {
            if (ModelState.IsValid)
            {
                var taskKey = (from t in _context.Tasks
                               where t.TaskId == task.TaskId
                               select t.TaskId);
                if (taskKey != null)
                {
                    return RedirectToAction(nameof(TakenKey));
                }
                // Total elapsed time of the task is automatically set to zero on task creation and is modified by adding/editing/deleting worklog records.
                task.ElapsedTime = TimeSpan.Zero;
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Asignee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", task.AsigneeId);
            ViewData["ProjectKey"] = new SelectList(_context.Projects, "ProjectKey", "ProjectKey", task.ProjectKey);
            ViewData["Reporter"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", task.ReporterId);
            return View(task);
        }

        public async Task<IActionResult> TakenKey()
        {
            return View();
        }

        // GET: Task/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            ViewData["Asignee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", task.AsigneeId);
            ViewData["ProjectKey"] = new SelectList(_context.Projects, "ProjectKey", "ProjectKey", task.ProjectKey);
            ViewData["Reporter"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", task.ReporterId);
            return View(task);
        }

        // POST: Task/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, [Bind("TaskId,TaskName,ReporterId,AsigneeId,ProjectKey,EstimatedTime,ElapsedTime,Done,DoneDate")] ModelsDB.Task task)
        {
            if (id != task.TaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.TaskId))
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
            
            ViewData["Asignee"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", task.AsigneeId);
            ViewData["ProjectKey"] = new SelectList(_context.Projects, "ProjectKey", "ProjectKey", task.ProjectKey);
            ViewData["Reporter"] = new SelectList(_context.Employees, "EmployeeId", "FirstLastName", task.ReporterId);
            return View(task);
        }

        // GET: Task/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.Asignee)
                .Include(t => t.ProjectKeyNavigation)
                .Include(t => t.Reporter)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var task = await _context.Tasks.FindAsync(id);
            var worklogs = from w in _context.Worklogs
                           where w.TaskId == task.TaskId
                           select w;
            foreach (var worklog in worklogs)
            {
                _context.Remove(worklog);
            }
            _context.SaveChanges();

            var taskEfficiencies = from te in _context.TaskEmployeeEfficiencies
                                   where te.TaskId == task.TaskId
                                   select te;
            foreach (var taskEfficiency in taskEfficiencies)
            {
                _context.Remove(taskEfficiency);
            }
            _context.SaveChanges();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskExists(string id)
        {
            return _context.Tasks.Any(e => e.TaskId == id);
        }
    }
}