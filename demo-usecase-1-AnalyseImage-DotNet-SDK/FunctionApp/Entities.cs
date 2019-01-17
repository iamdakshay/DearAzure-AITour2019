using System.Collections.Generic;

namespace FunctionApp
{
    public class Analysis
    {
        public List<Tag> tags { get; set; }
        public Description description { get; set; }
        public List<Face> faces { get; set; }
    }
    public class Tag
    {
        public string name;
    }
    public class Description
    {
        public List<Caption> captions;
    }
    public class Caption
    {
        public string text;
    }
    public class Face
    {
        public int age;
        public string gender;
    }
}
