using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolManagementApp.MVC.Data;

namespace SchoolManagementApp.MVC.Models
{
    public class ClassEnrollmentViewModel
    {
        public ClassViewModel? Class {get; set;}
        public List<StudentEnrollmentViewModel> Students {get; set;} = new 
           List<StudentEnrollmentViewModel>();
        
    }


}
