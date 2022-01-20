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
    public class EmployeesController : Controller
    {
        private readonly EfficiencyToolNewContext _context;

        public EmployeesController(EfficiencyToolNewContext context)
        {
            _context = context;
        }

        // GET: Employees
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // GET: Employees/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("EmployeeId,FirstLastName,Email")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                var existingEmployee = (from e in _context.Employees
                                        where e.EmployeeId == employee.EmployeeId
                                        select e.EmployeeId).Single();
                if (existingEmployee != null)
                {
                    return RedirectToAction(nameof(TakenKey));
                }
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> TakenKey() 
        {
            return View();
        }

        // GET: Employees/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, [Bind("EmployeeId,FirstLastName,Email")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
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
            return View(employee);
        }

        // GET: Employees/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var employee = await _context.Employees.FindAsync(id);
            List<string> deleteTaskIds = new List<string>();
            var taskIds = (from w in _context.Worklogs
                       where w.EmployeeId == employee.EmployeeId
                       select w.TaskId).Distinct();
            // all of the tasks the employee worked on are obtained
            foreach (string taskId in taskIds)
            {
                deleteTaskIds.Add(taskId);
                
            }
            // each task is then used to delete any Worklog or TaskEmployeeEfficiency records
            foreach (var taskId in deleteTaskIds)
            {
                var worklogs = from w in _context.Worklogs
                               where w.TaskId == taskId
                               select w;
                // all of the Worklog records for the task are selected and deleted
                foreach (var row in worklogs)
                {
                    _context.Worklogs.Remove(row);
                }
                _context.SaveChanges();
                var taskEfficiencies = from t in _context.TaskEmployeeEfficiencies
                                       where t.TaskId == taskId
                                       select t;
                // all of the TaskEmployeeEfficiency records for the task are selected and deleted
                foreach (var row in taskEfficiencies)
                {
                    _context.TaskEmployeeEfficiencies.Remove(row);
                }
                _context.SaveChanges();
                var task = (from t in _context.Tasks
                            where t.TaskId == taskId
                            select t).Single();
                // once any records related to the task are deleted, the task is then deleted
                _context.Tasks.Remove(task);
                _context.SaveChanges();
            }
            // after all of the employee's tasks are deleted, the asociated EmployeeAvgEfficiency
            // records are deleted
            var employeeEfficienies = from e in _context.EmployeeAvgEfficiencies
                                      where e.EmployeeId == employee.EmployeeId
                                      select e;
            foreach (var row in employeeEfficienies)
            {
                _context.EmployeeAvgEfficiencies.Remove(row);
            }
            _context.SaveChanges();
            // once all of the database table records associated with the employee are deleted, only then 
            // is then is the employee deleted
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}