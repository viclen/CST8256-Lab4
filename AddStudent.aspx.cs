using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Addstudents : System.Web.UI.Page
{
    public string order = "ascending";

    public class SortById : IComparer<AcademicRecord>
    {
        private String order = "ascending";
        public SortById(String order)
        {
            this.order = order;
        }

        public int Compare(AcademicRecord x, AcademicRecord y)
        {
            AcademicRecord a = (AcademicRecord)x;
            AcademicRecord b = (AcademicRecord)y;

            if (this.order.Equals("descending"))
            {
                a = (AcademicRecord)y;
                b = (AcademicRecord)x;
            }

            return a.Student.Id.CompareTo(b.Student.Id);
        }
    }

    public class SortByName : IComparer<AcademicRecord>
    {
        private String order = "ascending";
        public SortByName(String order)
        {
            this.order = order;
        }

        public int Compare(AcademicRecord x, AcademicRecord y)
        {
            AcademicRecord a = (AcademicRecord)x;
            AcademicRecord b = (AcademicRecord)y;

            if (this.order.Equals("descending"))
            {
                a = (AcademicRecord)y;
                b = (AcademicRecord)x;
            }

            if (a.Student.Name.Split(' ')[1].Equals(b.Student.Name.Split(' ')[1]))
            {
                return a.Student.Name.CompareTo(b.Student.Name);
            }

            return a.Student.Name.Split(' ')[1].CompareTo(b.Student.Name.Split(' ')[1]);
        }
    }

    public class SortByGrade : IComparer<AcademicRecord>
    {
        private String order = "ascending";
        public SortByGrade(String order)
        {
            this.order = order;
        }

        public int Compare(AcademicRecord x, AcademicRecord y)
        {
            AcademicRecord a = (AcademicRecord)x;
            AcademicRecord b = (AcademicRecord)y;
            if (this.order.Equals("descending"))
            {
                a = (AcademicRecord)y;
                b = (AcademicRecord)x;
            }

            if (a.Grade == b.Grade)
            {
                return (new SortByName(this.order)).Compare(x, y);
            }

            return a.Grade < b.Grade ? 1 : -1;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        LinkButton btnHome = (LinkButton)Master.FindControl("btnHome");
        btnHome.Click += (object s, EventArgs ev) =>
        {
            Response.Redirect("Default.aspx");
        };

        BulletedList topMenu = (BulletedList)Master.FindControl("topMenu");
        topMenu.Click += (object s, BulletedListEventArgs ev) =>
        {
            switch (ev.Index)
            {
                case 0:
                    Response.Redirect("AddCourse.aspx");
                    break;
            }
        };
        topMenu.Items.Remove("Add Students");

        if (topMenu.Items.Count < 1)
        {
            topMenu.Items.Add("Add Course");
        }

        using (var context = new StudentRecordEntities1()) {
            var courses = context.Courses.ToList<Course>();

            if (drpCourseSelection.Items.Count < courses.Count) {
                for (int i = 0; i < courses.Count; i++)
                {
                    drpCourseSelection.Items.Add(new ListItem(courses[i].Title, courses[i].Code));
                }
            }
        }

        if (Session["selectedcourse"] != null && drpCourseSelection.SelectedValue=="-1")
        {
            drpCourseSelection.SelectedValue = Session["selectedcourse"].ToString();
            orderList();
        }
    }

    public void btnSubmit_Click(object sender, EventArgs e)
    {
        Page.Validate();

        if (Session["selectedcourse"] != null)
        {
            if (Page.IsValid)
            {
                using (var context = new StudentRecordEntities1())
                {
                    Course course = context.Courses.Where(c => c.Code == drpCourseSelection.SelectedValue).FirstOrDefault();

                    if (course != null)
                    {
                        List<AcademicRecord> records = course.AcademicRecords.Where(r => r.StudentId == studentID.Text).ToList();
                        if (records.Count > 0)
                        {
                            studentExistsError.Text = "The system already has a record of this student for the selected course";
                            orderList();
                        }
                        else
                        {
                            Student s = context.Students.Where(st => st.Id == studentID.Text).FirstOrDefault();

                            if (s == null)
                            {
                                s = new Student();
                                s.Id = studentID.Text;
                                s.Name = studentName.Text;
                                context.Students.Add(s);
                            }

                            AcademicRecord a = new AcademicRecord();
                            a.CourseCode = course.Code;
                            a.Grade = int.Parse(studentGrade.Text);
                            a.Student = s;

                            course.AcademicRecords.Add(a);
                            context.SaveChanges();

                            Response.Redirect("AddStudent.aspx");
                        }
                    }
                }
            }
        }
    }

    public void orderList()
    {
        if (Session["selectedcourse"] != null && !string.IsNullOrEmpty(Session["selectedcourse"].ToString()))
        {
            using (var context = new StudentRecordEntities1())
            {
                string s = Session["selectedcourse"].ToString();
                Course course = context.Courses.Where(c => c.Code.Equals(s)).FirstOrDefault();

                if(course == null)
                {
                    selectCourseError.Text = "Please select a course";
                    return;
                }

                TableRow header = tblStudents.Rows[0];
                tblStudents.Rows.Clear();
                tblStudents.Rows.Add(header);

                List<AcademicRecord> list = course.AcademicRecords.ToList();

                if (Session["sort"] != null && (string)Session["sort"] == Request.Params["sort"])
                {
                    if (Session["order"] != null && Session["order"].ToString() == "ascending")
                    {
                        this.order = "descending";
                        Session["order"] = this.order;
                    }
                    else
                    {
                        this.order = "ascending";
                        Session["order"] = this.order;
                    }
                }
                else
                {
                    this.order = "ascending";
                    Session["order"] = this.order;
                }

                Session["sort"] = Request.Params["sort"];

                IComparer<AcademicRecord> comparer;
                switch (Request.Params["sort"])
                {
                    case "id":
                        comparer = new SortById(this.order);
                        list.Sort(comparer);
                        break;
                    case "name":
                        comparer = new SortByName(this.order);
                        list.Sort(comparer);
                        break;
                    case "grade":
                        comparer = new SortByGrade(this.order);
                        list.Sort(comparer);
                        break;
                }

                foreach (AcademicRecord student in list)
                {
                    TableRow row = new TableRow();

                    TableCell cell = new TableCell();
                    cell.Text = student.Student.Id;
                    row.Cells.Add(cell);

                    cell = new TableCell();
                    cell.Text = student.Student.Name;
                    row.Cells.Add(cell);

                    cell = new TableCell();
                    cell.Text = student.Grade + "";
                    row.Cells.Add(cell);

                    tblStudents.Rows.Add(row);
                }
            }
        }
    }

    protected void drpCourseSelection_SelectedIndexChanged(object sender, EventArgs e)
    {
        Session["selectedcourse"] = drpCourseSelection.SelectedValue;

        orderList();
    }
}