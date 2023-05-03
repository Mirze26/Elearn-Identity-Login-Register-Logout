using Elearn_temp.Areas.Admin.ViewModels;
using Elearn_temp.Data;
using Elearn_temp.Helpers;
using Elearn_temp.Models;
using Elearn_temp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Elearn_temp.Areas.Admin.Controllers
{

    [Area("Admin")]

    public class CourseController : Controller
    {

        private readonly ICourseService _courseService;

        private readonly IAuthorService _authorService;

        private readonly IWebHostEnvironment _env;     

        private readonly AppDbContext _context;
        public CourseController(ICourseService courseService, IAuthorService authorService, IWebHostEnvironment env, AppDbContext context)
        {
            _courseService = courseService;
            _authorService = authorService;
            _env = env;
            _context = context;
        }




        public async Task<IActionResult> Index()
        {
            IEnumerable<Course> courses = await _context.Courses.Include(m => m.CourseImages).Include(m => m.Author).Where(m => !m.SoftDelete).ToListAsync();

            return View(courses);
        }


        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null) return BadRequest();


            Course dbCourses = await _context.Courses.Include(m => m.CourseImages).Include(m => m.Author).FirstOrDefaultAsync(m => m.Id == id);


            if (dbCourses is null) return NotFound();

            ViewBag.desc = Regex.Replace(dbCourses.Description, "<.*?>", String.Empty);

            return View(new CourseDetailVM   
            {

                Name = dbCourses.Name,
                Description = dbCourses.Description,
                Price = dbCourses.Price.ToString("0.#####").Replace(",", "."),
                CountSale = dbCourses.CountSale,
                AuthorId = dbCourses.AuthorId,
                Images = dbCourses.CourseImages,
                AuthorName = dbCourses.Author.Name
            });

        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            

      

            ViewBag.authories = await GetAuthoriesAsync();

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseCreateVM model)
        {  
            try
            {
                

                


                ViewBag.authories = await GetAuthoriesAsync();

                if (!ModelState.IsValid)
                {
                    return View(model); 
                }


                foreach (var photo in model.Photos)
                {

                    if (!photo.CheckFileType("image/"))
                    {
                        ModelState.AddModelError("Photo", "File type must be image");
                        return View();
                    }

                    if (!photo.CheckFileSize(200))
                    {
                        ModelState.AddModelError("Photo", "Image size must be max 200kb");
                        return View();
                    }



                }

                List<CourseImage> courseImages = new();  

                foreach (var photo in model.Photos)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + photo.FileName; 


                    string path = FileHelper.GetFilePath(_env.WebRootPath, "images", fileName);

                    await FileHelper.SaveFlieAsync(path, photo);


                    CourseImage courseImage = new()   
                    {
                        Image = fileName
                    };

                    courseImages.Add(courseImage); 

                }

                courseImages.FirstOrDefault().IsMain = true; 
                int convertedPrice = int.Parse(model.Price);
                Course newProduct = new()
                {
                    Name = model.Name,       
                    CountSale = model.CountSale,         
                    AuthorId = model.AuthorId,
                    CourseImages = courseImages  
                };

                await _context.CourseImages.AddRangeAsync(courseImages); 
                await _context.Courses.AddAsync(newProduct);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return BadRequest();

                Course course = await _courseService.GetFullDataById((int)id);

                if (course is null) return NotFound();

                

                
              
                
                foreach (var item in course.CourseImages)
                {
                    string path = FileHelper.GetFilePath(_env.WebRootPath, "images", item.Image);

                    FileHelper.DeleteFile(path);
                }

                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {

                return RedirectToAction("Error", new { msj = ex.Message }); 
            }



        }


        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int? id)
        {
            if (id == null) return BadRequest();

            bool result = false;

            CourseImage courseImage = await _context.CourseImages.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (courseImage == null) return NotFound();

            var data = await _context.Courses.Include(m => m.CourseImages).FirstOrDefaultAsync(m => m.Id == courseImage.CoruseId);

            if (data.CourseImages.Count > 1)
            {
                string path = FileHelper.GetFilePath(_env.WebRootPath, "images", courseImage.Image);

                FileHelper.DeleteFile(path);

                _context.CourseImages.Remove(courseImage);

                await _context.SaveChangesAsync();

                result = true;
            }

            data.CourseImages.FirstOrDefault().IsMain = true;

            await _context.SaveChangesAsync();

            return Ok(result);

        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null) return BadRequest();

            ViewBag.authories = await GetAuthoriesAsync();

            Course dbCourse = await _courseService.GetFullDataById((int)id);

            if (dbCourse == null) return NotFound();


            CourseEditVM model = new()
            {
                Id = dbCourse.Id,
                Name = dbCourse.Name,
                CountSale = dbCourse.CountSale,
                Price = dbCourse.Price,
                AuthorId = dbCourse.AuthorId,
                Images = dbCourse.CourseImages,
                Description = dbCourse.Description
            };


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, CourseEditVM updatedCourse)
        {
            if (id == null) return BadRequest();

            ViewBag.authories = await GetAuthoriesAsync();

            Course dbCourse = await _context.Courses.AsNoTracking().Include(m => m.CourseImages).Include(m => m.Author).FirstOrDefaultAsync(m => m.Id == id);

            if (dbCourse == null) return NotFound();

            if (!ModelState.IsValid)
            {
                updatedCourse.Images = dbCourse.CourseImages;
                return View(updatedCourse);
            }

            List<CourseImage> courseImages = new();

            if (updatedCourse.Photos is not null)
            {
                foreach (var photo in updatedCourse.Photos)
                {
                    if (!photo.CheckFileType("image/"))
                    {
                        ModelState.AddModelError("Photo", "File type must be image");
                        updatedCourse.Images = dbCourse.CourseImages;
                        return View(updatedCourse);
                    }

                    if (!photo.CheckFileSize(200))
                    {
                        ModelState.AddModelError("Photo", "Image size must be max 200kb");
                        updatedCourse.Images = dbCourse.CourseImages;
                        return View(updatedCourse);
                    }
                }



                foreach (var photo in updatedCourse.Photos)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + photo.FileName;

                    string path = FileHelper.GetFilePath(_env.WebRootPath, "images", fileName);

                    await FileHelper.SaveFlieAsync(path, photo);

                    CourseImage courseImage = new()
                    {
                        Image = fileName
                    };

                    courseImages.Add(courseImage);
                }

                await _context.CourseImages.AddRangeAsync(courseImages);
            }



            Course newCourse = new()
            {
                Id = dbCourse.Id,
                Name = updatedCourse.Name,
                Price = updatedCourse.Price,
                CountSale = updatedCourse.CountSale,
                Description = updatedCourse.Description,
                AuthorId = updatedCourse.AuthorId,
                CourseImages = courseImages.Count == 0 ? dbCourse.CourseImages : courseImages
            };


            _context.Courses.Update(newCourse);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




      





        private async Task<SelectList> GetAuthoriesAsync()
        {

            IEnumerable<Author> authores = await _authorService.GetAll();

            return new SelectList(authores, "Id", "Name");


        }



    }
}
