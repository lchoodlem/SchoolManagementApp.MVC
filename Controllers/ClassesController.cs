using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagementApp.MVC.Data;
using SchoolManagementApp.MVC.Models;

namespace SchoolManagementApp.MVC.Controllers
{
    public class ClassesController : Controller
    {
        private readonly SchoolMgmtDbContext _context;

        public ClassesController(SchoolMgmtDbContext context)
        {
            _context = context;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var schoolMgmtDbContext = _context.Classes
                        .Include(q => q.Course)
                        .Include(q => q.Lecturer);
            return View(await schoolMgmtDbContext.ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Classes == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {

            CreteSelectList();
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // if something fails then reload the lists for the class
            CreteSelectList();
            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Classes == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            CreteSelectList();
            return View(@class);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LecturerId,CourseId,Time")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
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
             // if something fails then reload the lists for the class
            CreteSelectList();
            return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Classes == null)
            {
                return NotFound();
            }

            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Classes == null)
            {
                return Problem("Entity set 'SchoolMgmtDbContext.Classes'  is null.");
            }
            var @class = await _context.Classes.FindAsync(id);
            if (@class != null)
            {
                _context.Classes.Remove(@class);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> ManageEnrollments(int classId) 
        {
            var @class = await _context.Classes
                .Include(q => q.Course)
                .Include(q => q.Lecturer)
                .Include(q => q.Enrollments)
                    .ThenInclude(q => q.Student)
                .FirstOrDefaultAsync(m => m.Id == classId);

            var students = await _context.Students.ToListAsync();

            var model = new ClassEnrollmentViewModel();
            model.Class = new ClassViewModel 
                {
                    Id = @class.Id,
                    CourseName = $"{@class.Course.Code}: {@class.Course.Name}",
                    LecturerName = $"{@class.Lecturer.FirstName} {@class.Lecturer.LastName}",
                    Time = @class.Time.ToString()

                };
            foreach(var stu in students){
                model.Students.Add(new StudentEnrollmentViewModel
                {
                 Id = stu.Id,
                 FirstName = stu.FirstName,
                 LastName = stu.LastName,
                 IsEnrolled = (@class?.Enrollments.Any(q => q.StudentId == stu.Id))
                            .GetValueOrDefault()

                });
            }
            return View(model);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnrollStudent(int classId, int studentId, bool shouldEnroll)
        {
            var enrollment = new Enrollment();
            if(shouldEnroll == true)
            {
                enrollment.ClassId = classId;
                enrollment.StudentId = studentId;
                await _context.Enrollments.AddAsync(enrollment);
            } 
            else 
            {
                enrollment = await _context.Enrollments.FirstOrDefaultAsync(
                    q => q.ClassId == classId && q.StudentId == studentId);
                if(enrollment != null){
                    _context.Remove(enrollment);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageEnrollments), new { id = classId}); // advanced Redirect passing ID

        }


        private bool ClassExists(int id)
        {
          return (_context.Classes?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private void CreteSelectList()
        {
           var courses = _context.Courses.Select(crs => new 
                {
                        CourseName = $"{crs.Code}: {crs.Name} (credits: {crs.Credits})",
                        crs.Id 
                });    
            ViewData["CourseId"] = new SelectList(courses, "Id", "CourseName");
        
             var lecturers = _context.Lecturers.Select(lect => new 
                {
                        FullName = $"{lect.FirstName} {lect.LastName}",
                        lect.Id 
                });
            ViewData["LecturerId"] = new SelectList(lecturers, "Id", "FullName");
 
        }
    }
}
