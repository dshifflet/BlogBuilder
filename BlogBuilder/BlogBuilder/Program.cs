using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace BlogBuilder
{
    class Program
    {
        private static string SiteUrl = "http://www.dshifflet.com";
        
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            //todo handle args...
            BuildBlogOutput(new DirectoryInfo(@"D:\src\sandbox\dshifflet\blog"),
                new DirectoryInfo(@"D:\src\sandbox\dshifflet2\blog"));

            BuildBlogOutput(new DirectoryInfo(@"D:\src\sandbox\dshifflet\blog\books"),
                new DirectoryInfo(@"D:\src\sandbox\dshifflet2\blog\books"));                
        }

        static void BuildBlogOutput(DirectoryInfo input, DirectoryInfo output)
        {
            if (!output.Exists)
            {
                output.Create();
            }
            var mainTemplate = new FileInfo(string.Format("{0}\\{1}", input.FullName, "index.html"));
            var newIndex = new FileInfo(string.Format("{0}\\index.html", output.FullName));
            using (var sr = new StreamReader(mainTemplate.OpenRead()))
            using (var sw = new StreamWriter(newIndex.Create()))
            {
                var s = sr.ReadLine();
                while (s != null)
                {
                    if (s.Contains("<!-- #BLOG"))
                    {
                        var blogContent = new FileInfo(string.Format("{0}\\{1}", input.FullName,
                            s.Replace("<!-- #BLOG", "").Replace("-->", "").Trim()));

                        BlogSummaryHtml(blogContent, sw, input);

                        BuildBlogStoryHtml(mainTemplate, blogContent, 
                            new FileInfo(string.Format("{0}\\{1}", output.FullName, blogContent.Name)), input);

                    }
                    sw.WriteLine(s);
                    s = sr.ReadLine();
                }
            }
        }

        static void BlogSummaryHtml(FileInfo blogContent, StreamWriter sw, DirectoryInfo root)
        {
            var blogContentName = GetBlogContentName(blogContent, root);
            using (var sr = new StreamReader(blogContent.OpenRead()))
            {
                var s = sr.ReadLine();
                while (s != null)
                {
                    if (s.Contains("<h2 class=\"blog-post-title\">"))
                    {
                        sw.WriteLine("<a href=\"{0}\" style=\"color:black;\">{1}</a>", blogContentName, s);
                    }
                    else if (s.Contains("<a name=\"endsummary\"/>"))
                    {
                        var txt = s.Replace("<a name=\"endsummary\"/>", "")
                            .Replace("<!--", "").Replace("-->", "");
                        sw.WriteLine(txt);
                        sw.WriteLine("...<br/><i><a href='{0}#endsummary'>Read More</a></i><br/>", blogContentName);
                        sw.WriteLine("		  </div> <!-- /.blog-post -->");
                        sw.WriteLine("<hr/>");
                        break;
                    }
                    else
                    {
                        sw.WriteLine(s);
                    }
                    
                    s = sr.ReadLine();
                }
            }
        }

        static void BuildBlogStoryHtml(FileInfo mainTemplate, FileInfo blogContent, FileInfo output, DirectoryInfo root)
        {
            var wroteContent = false;
            var blogContentName = GetBlogContentName(blogContent, root);

            if (blogContentName.Contains("/")) { return; }

            var title = "";
            using (var sr = new StreamReader(mainTemplate.OpenRead()))
            using (var sw = new StreamWriter(output.Create()))
            {
                var s = sr.ReadLine();
                while (s != null)
                {
                    if (s.Contains("<!-- #BLOG") && ! wroteContent)
                    {
                        title = WriteBlogContent(blogContent, sw);
                        wroteContent = true;
                    }
                    else if (s.Contains(
                        string.Format(
                        "this.page.url = '{0}/blog/index.html';  // Replace PAGE_URL with your page's canonical URL variable", 
                        SiteUrl)) ||
                        s.Contains(
                            string.Format(
                            "this.page.url = '{0}/blog/books/index.html';  // Replace PAGE_URL with your page's canonical URL variable", SiteUrl))
                    )
                    {
                        s = string.Format(
                            "this.page.url = '{0}/blog/{1}';  // Replace PAGE_URL with your page's canonical URL variable",
                            SiteUrl,
                            blogContentName);
                    }
                    else if (s.Contains(
                        "this.page.identifier = 'index.html'; // Replace PAGE_IDENTIFIER with your page's unique identifier variable")
                    )
                    {
                        s = string.Format(
                                "this.page.identifier = '{0}/blog/{1}'; // Replace PAGE_IDENTIFIER with your page's unique identifier variable",
                                SiteUrl,
                                blogContentName
                        );
                    }
                    sw.WriteLine(s);
                    s = sr.ReadLine();
                }
            }
            if (title.Length > 0)
            {
                //replace the titles so the links look better on social media...
                var lines = new List<string>();
                using (var sr = new StreamReader(output.OpenRead()))
                {
                    var s = sr.ReadLine();
                    while (s != null)
                    {
                        if (s.Contains("<title>"))
                        {
                            var msg = s.Replace("<title>", "").Replace("</title>", "");
                            s = string.Format("<title>{0} - {1}</title>", msg.Trim(), title.Trim());
                        }
                        lines.Add(s);
                        s = sr.ReadLine();
                    }
                }
                using (var sw = new StreamWriter(output.Create()))
                {
                    foreach (var line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
        }

        public static string GetBlogContentName(FileInfo blogContent, DirectoryInfo root)
        {
            if (blogContent.Directory!=null && blogContent.Directory.Parent!=null && 
                !blogContent.Directory.Name.Equals(root.Name, StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("{0}/{1}",
                    blogContent.Directory.Name, blogContent.Name);
            }
            return blogContent.Name;
        }

        public static string WriteBlogContent(FileInfo content, StreamWriter sw)
        {
            var result = "";
            using (var sr = new StreamReader(content.OpenRead()))
            {
                var s = sr.ReadLine();
                while (s != null)
                {
                    if (s.StartsWith("<!-- #TITLE "))
                    {
                        result = s.Replace("<!-- #TITLE ","").Replace("-->","");
                    }

                    if (s.Contains("<h2 class=\"blog-post-title\">"))
                    {
                        sw.WriteLine("<a href=\"index.html\">&lt; Back to Index</a><br/>");
                    }
                    sw.WriteLine(s);
                    s = sr.ReadLine();
                }
            }
            return result;
        }
    }
}
