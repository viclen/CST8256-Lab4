using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Addcourses : System.Web.UI.Page
{
    public List<Course> courseList;
    private string order = "ascending";

    public class SortByNumber : IComparer<Course>
    {
        private String order = "ascending";
        public SortByNumber(String order)
        {
            this.order = order;
        }

        public int Compare(Course x, Course y)
        {
            Course a = (Course)x;
            Course b = (Course)y;

            if (this.order.Equals("descending"))
            {
                a = (Course)y;
                b = (Course)x;
            }

            return a.Code.CompareTo(b.Code);
        }
    }

    public class SortByName : IComparer<Course>
    {
        private String order = "ascending";
        public SortByName(String order)
        {
            this.order = order;
        }

        public int Compare(Course x, Course y)
        {
            Course a = (Course)x;
            Course b = (Course)y;

            if (this.order.Equals("descending"))
            {
                a = (Course)y;
                b = (Course)x;
            }

            return a.Title.CompareTo(b.Title);
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
                    Response.Redirect("AddStudent.aspx");
                    break;
            }
        };
        topMenu.Items.Remove("Add Courses");

        if (topMenu.Items.Count < 1)
        {
            topMenu.Items.Add("Add Students");
        }

        loadTable();
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        Course course = new Course();
        course.Code = courseNumber.Text;
        course.Title = courseName.Text;

        using(var context = new StudentRecordEntities1())
        {
            if(context.Courses.Where(c => c.Code == courseNumber.Text).Count() > 0)
            {
                labelCourseNumberError.Text = "Course with this code already exists.";
                return;
            }
            else
            {
                context.Courses.Add(course);
                context.SaveChanges();
                Response.Redirect("AddCourse.aspx");
            }
        }
    }

    private void loadTable()
    {
        if (Session["sort"] != null && Session["sort"].ToString() == Request.Params["sort"])
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

        using (var context = new StudentRecordEntities1())
        {
            List<Course> list = context.Courses.ToList();

            IComparer<Course> comparer;
            switch (Request.Params["sort"])
            {
                case "number":
                    comparer = new SortByNumber(this.order);
                    list.Sort(comparer);
                    break;
                case "name":
                    comparer = new SortByName(this.order);
                    list.Sort(comparer);
                    break;
            }

            foreach (Course course in list)
            {
                TableRow row = new TableRow();

                TableCell cell = new TableCell();
                cell.Text = course.Code;
                row.Cells.Add(cell);

                cell = new TableCell();
                cell.Text = course.Title;
                row.Cells.Add(cell);

                tblCourses.Rows.Add(row);
            }
        }
    }
}