using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace MountainStates.MSSA.Module.HelloWorld.Repository
{
    public interface IHelloWorldRepository
    {
        IEnumerable<Models.HelloWorld> GetHelloWorlds(int ModuleId);
        Models.HelloWorld GetHelloWorld(int HelloWorldId);
        Models.HelloWorld GetHelloWorld(int HelloWorldId, bool tracking);
        Models.HelloWorld AddHelloWorld(Models.HelloWorld HelloWorld);
        Models.HelloWorld UpdateHelloWorld(Models.HelloWorld HelloWorld);
        void DeleteHelloWorld(int HelloWorldId);
    }

    public class HelloWorldRepository : IHelloWorldRepository, ITransientService
    {
        private readonly IDbContextFactory<HelloWorldContext> _factory;

        public HelloWorldRepository(IDbContextFactory<HelloWorldContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.HelloWorld> GetHelloWorlds(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.HelloWorld.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Models.HelloWorld GetHelloWorld(int HelloWorldId)
        {
            return GetHelloWorld(HelloWorldId, true);
        }

        public Models.HelloWorld GetHelloWorld(int HelloWorldId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.HelloWorld.Find(HelloWorldId);
            }
            else
            {
                return db.HelloWorld.AsNoTracking().FirstOrDefault(item => item.HelloWorldId == HelloWorldId);
            }
        }

        public Models.HelloWorld AddHelloWorld(Models.HelloWorld HelloWorld)
        {
            using var db = _factory.CreateDbContext();
            db.HelloWorld.Add(HelloWorld);
            db.SaveChanges();
            return HelloWorld;
        }

        public Models.HelloWorld UpdateHelloWorld(Models.HelloWorld HelloWorld)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(HelloWorld).State = EntityState.Modified;
            db.SaveChanges();
            return HelloWorld;
        }

        public void DeleteHelloWorld(int HelloWorldId)
        {
            using var db = _factory.CreateDbContext();
            Models.HelloWorld HelloWorld = db.HelloWorld.Find(HelloWorldId);
            db.HelloWorld.Remove(HelloWorld);
            db.SaveChanges();
        }
    }
}
