using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfUndoable;
using EfUndoable.DemoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject;

[TestClass]
public class UndoRedoTests
{
    [TestMethod]
    public async Task CreateUndoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.UndoAsync();
                
                Assert.IsFalse(context.Blogs.Any());
                Assert.IsFalse(context.Posts.Any());
            }

        }
    }

    [TestMethod]
    public async Task CreateUndoRedoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            int blogId, post1Id, post2Id;
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                (blogId, post1Id, post2Id) = await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.UndoAsync();
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.RedoAsync();
                
                Assert.AreEqual(1, context.Blogs.Count());
                Assert.AreEqual(2, context.Posts.Count());
                
                Assert.AreEqual(blogId, context.Blogs.ToArray()[0].BlogId);
                Assert.AreEqual(3, context.Blogs.ToArray()[0].Rating);
                Assert.AreEqual(post1Id, context.Posts.ToArray()[0].PostId);
                Assert.AreEqual(post2Id, context.Posts.ToArray()[1].PostId);
            }
        }
    }

    [TestMethod]
    public async Task DeleteUndoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            int post1Id;
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                (_, post1Id, _) = await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                context.Remove(context.Posts.FirstOrDefault(p => p.PostId == post1Id)!);
                await context.SaveChangesAsync();
                
                Assert.AreEqual(1, context.Blogs.Include(b=>b.Posts).Single().Posts.Count);
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.UndoAsync();

                Assert.AreEqual(2, context.Blogs.Include(b=>b.Posts).Single().Posts.Count);
            }
        }
    }

    [TestMethod]
    public async Task DeleteUndoRedoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            int post1Id;
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                (_, post1Id, _) = await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                context.Remove(context.Posts.FirstOrDefault(p => p.PostId == post1Id)!);
                await context.SaveChangesAsync();
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.UndoAsync();
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.RedoAsync();

                Assert.AreEqual(1, context.Blogs.Include(b=>b.Posts).Single().Posts.Count);
            }
        }
    }

    [TestMethod]
    public async Task EditUndoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                var blog = context.Blogs.Include(b => b.Posts).Single();
                blog.Rating = 4;
                blog.Posts.Add(new Post() {Title = "Third post", Content = "With some content"});
                await context.SaveChangesAsync();
                Assert.AreEqual(3, blog.Posts.Count);
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.UndoAsync();
                
                var blog = context.Blogs.Include(b => b.Posts).Single();
                Assert.AreEqual(3, blog.Rating);
                Assert.AreEqual(2, blog.Posts.Count);
            }
        }
    }

    [TestMethod]
    public async Task EditUndoRedoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            int post3Id;
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                var blog = context.Blogs.Include(b => b.Posts).Single();
                blog.Rating = 4;
                blog.Posts.Add(new Post() {Title = "Third post", Content = "With some content"});
                await context.SaveChangesAsync();
                post3Id = blog.Posts[1].PostId;
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.UndoAsync();
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await context.RedoAsync();
                
                var blog = context.Blogs.Include(b => b.Posts).Single();
                Assert.AreEqual(4, blog.Rating);
                Assert.AreEqual(3, blog.Posts.Count);
                Assert.AreEqual("Third post" , blog.Posts.Last().Title);
                Assert.AreEqual(1, blog.Posts.Count(p=>p.PostId == post3Id));
            }
        }
    }

    [TestMethod]
    public async Task EditDoubleUndoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                var blog = context.Blogs.Include(b => b.Posts).Single();
                blog.Rating = 4;
                blog.Posts.Add(new Post() { Title = "Third post", Content = "With some content" });
                await context.SaveChangesAsync();
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
                await context.UndoAsync();

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
                await context.UndoAsync();

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                Assert.IsFalse(context.Blogs.Any());
                Assert.IsFalse(context.Posts.Any());
            }
        }
    }

    [TestMethod]
    public async Task EditDoubleUndoRedoTestAsync()
    {
        using (var connection = BloggingContextSqliteInMemory.CreateConnection())
        {
            EfUndoManager undoManager = new EfUndoManager();
            int post3Id;
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                await WriteDefaultInfoAsync(context);
            }

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                var blog = context.Blogs.Include(b => b.Posts).Single();
                blog.Rating = 4;
                blog.Posts.Add(new Post() {Title = "Third post", Content = "With some content"});
                await context.SaveChangesAsync();
                post3Id = blog.Posts[1].PostId;
            }
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
                await context.UndoAsync();
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
                await context.UndoAsync();

            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
                await context.RedoAsync();
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
                await context.RedoAsync();
            
            await using (var context = BloggingContextSqliteInMemory.Create(connection, undoManager))
            {
                var blog = context.Blogs.Include(b => b.Posts).Single();
                Assert.AreEqual(4, blog.Rating);
                Assert.AreEqual(3, blog.Posts.Count);
                Assert.AreEqual("Third post" , blog.Posts.Last().Title);
                Assert.AreEqual(1, blog.Posts.Count(p=>p.PostId == post3Id));
            }
        }
    }

    
    
    private async Task<(Int32 blogId, Int32 post1Id, Int32 post2Id)> WriteDefaultInfoAsync(BloggingContextSqliteInMemory context)
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

        var blogId = blog.BlogId;
        var post1Id = blog.Posts[0].PostId;
        var post2Id = blog.Posts[1].PostId;
        return (blogId, post1Id, post2Id);
    }
}