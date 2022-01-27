using System.Diagnostics;
using EfUndoable.DemoApi;
using Microsoft.EntityFrameworkCore;

namespace EfUndoable.DemoApp;

class Program
{
    static async Task Main()
    {
        Console.Write("Creating Db...");
        await CreateContextAsync();
        Console.WriteLine("ok");

        var undoManager = new EfUndoManager();

        Int32 blogId, post1Id, post2Id, post3Id;
            
        using (var context = CreateBloggingContext(undoManager))
        {
            var blog = new Blog()
            {
                Rating = 3,
                Url = "https://blog1.com",
                Posts = new List<Post>()
                {
                    new Post()
                    {
                        Title = "First title",
                        Content = "Some content",
                    },
                    new Post()
                    {
                        Title = "Second title",
                        Content = "Lorem ipsum",
                    }

                }
            };
            await context.Blogs.AddAsync(blog);

            await context.SaveChangesAsync();

            blogId = blog.BlogId;
            post1Id = blog.Posts[0].PostId;
            post2Id = blog.Posts[1].PostId;
        }

        using (var context = CreateBloggingContext(undoManager))
        {
            await context.UndoAsync();
                
            Debug.Assert(!context.Blogs.Any());
            Debug.Assert(!context.Posts.Any());
        }
            
        using (var context = CreateBloggingContext(undoManager))
        {
            await context.RedoAsync();
                
            Debug.Assert(context.Blogs.Count() == 1);
            Debug.Assert(context.Posts.Count() == 2);
                
            Debug.Assert(context.Blogs.ToArray()[0].BlogId == blogId);
            Debug.Assert(context.Blogs.ToArray()[0].Rating == 3);
            Debug.Assert(context.Posts.ToArray()[0].PostId == post1Id);
            Debug.Assert(context.Posts.ToArray()[1].PostId == post2Id);
        }

        using (var context = CreateBloggingContext(undoManager))
        {
            context.Remove(context.Posts.FirstOrDefault(p => p.PostId == post1Id)!);
            await context.SaveChangesAsync();
                
            Debug.Assert(context.Blogs.Include(b=>b.Posts).Single().Posts.Count == 1);
        }

        using (var context = CreateBloggingContext(undoManager))
        {
            await context.UndoAsync();

            Debug.Assert(context.Blogs.Include(b=>b.Posts).Single().Posts.Count == 2);
        }

        using (var context = CreateBloggingContext(undoManager))
        {
            await context.RedoAsync();

            Debug.Assert(context.Blogs.Include(b=>b.Posts).Single().Posts.Count == 1);
        }
            
        using (var context = CreateBloggingContext(undoManager))
        {
            var blog = context.Blogs.Include(b => b.Posts).Single();
            blog.Rating = 4;
            blog.Posts.Add(new Post() {Title = "Third post", Content = "With some content"});
            await context.SaveChangesAsync();
            post3Id = blog.Posts[1].PostId;
        }

        using (var context = CreateBloggingContext(undoManager))
        {
            await context.UndoAsync();
                
            var blog = context.Blogs.Include(b => b.Posts).Single();
            Debug.Assert(blog.Rating == 3);
            Debug.Assert(blog.Posts.Count == 1);
        }

        using (var context = CreateBloggingContext(undoManager))
        {
            await context.RedoAsync();
                
            var blog = context.Blogs.Include(b => b.Posts).Single();
            Debug.Assert(blog.Rating == 4);
            Debug.Assert(blog.Posts.Count == 2);
            Debug.Assert(blog.Posts.Last().Title == "Third post");
            Debug.Assert(blog.Posts.Last().PostId == post3Id);
        }
            
        using (var context = CreateBloggingContext(undoManager))
        {
            await context.UndoAsync();
                
            var blog = context.Blogs.Include(b => b.Posts).Single();
            Debug.Assert(blog.Rating == 3);
            Debug.Assert(blog.Posts.Count == 1);
        }
            
        using (var context = CreateBloggingContext(undoManager))
        {
            await context.RedoAsync();
                
            var blog = context.Blogs.Include(b => b.Posts).Single();
            Debug.Assert(blog.Rating == 4);
            Debug.Assert(blog.Posts.Count == 2);
            Debug.Assert(blog.Posts.Last().Title == "Third post");
            Debug.Assert(blog.Posts.Last().PostId == post3Id);
        }
        
        Console.WriteLine("All scenarios passed");
    }

    private static BloggingContext CreateBloggingContext(EfUndoManager undoManager)
    {
        //return new BloggingContextSqlite(undoManager);
        return new BloggingContextMsSql(undoManager);
    }
    
    static async ValueTask CreateContextAsync()
    {
        File.Delete(BloggingContextSqlite.FileName);
        await using (var context = CreateBloggingContext(null!))
        {
            await context.Database.EnsureCreatedAsync();
            context.Posts.RemoveRange(context.Posts);
            context.Blogs.RemoveRange(context.Blogs);
            await context.SaveChangesAsync();
        }
    }
}