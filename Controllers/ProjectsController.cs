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
    public class ProjectsController : Controller
    {
        private readonly EfficiencyToolNewContext _context;

        public ProjectsController(EfficiencyToolNewContext context)
        {
            _context = context;
        }

        // GET: Projects
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects.ToListAsync());
        }

        // GET: Projects/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectKey == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("ProjectKey,ProjectName")] Project project)
        {
            if (ModelState.IsValid)
            {
                var projectKey = (from p in _context.Projects
                                  where p.ProjectKey == project.ProjectKey
                                  select p.ProjectKey).Single();
                if (projectKey != null)
                {
                    return RedirectToAction(nameof(TakenKey));
                }
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        public async Task<IActionResult> TakenKey()
        {
            return View();
        }

        // GET: Projects/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, [Bind("ProjectKey,ProjectName")] Project project)
        {
            if (id != project.ProjectKey)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectKey))
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
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectKey == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var project = await _context.Projects.FindAsync(id);
            List<string> associatedTaskIds = new List<string>();
            // all of the project's associated tasks are fetched
            var tasks = from t in _context.Tasks
                        where t.ProjectKey == id
                        select t;
            foreach (var row in tasks)
            {
                associatedTaskIds.Add(row.TaskId);
            }

            // for each of the project's tasks all of the Worklog and TaskEmployeeEfficiency records are obtained and deleted
            foreach (var taskId in associatedTaskIds)
            {
                var worklogs = from w in _context.Worklogs
                               where w.TaskId == taskId
                               select w;
                foreach (var row in worklogs)
                {
                    _context.Remove(row);
                }
                _context.SaveChanges();

                var taskEfficiencies = from te in _context.TaskEmployeeEfficiencies
                                       where te.TaskId == taskId
                                       select te;
                foreach (var row in taskEfficiencies)
                {
                    _context.Remove(row);
                }
                _context.SaveChanges();

                var task = (from t in _context.Tasks
                            where t.TaskId == taskId
                            select t).Single();
                _context.Remove(task);
                _context.SaveChanges();
            }
            // once all of the project's tasks and their associated records are deleted, only then is the project deleted
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.ProjectKey == id);
        }
    }
}
