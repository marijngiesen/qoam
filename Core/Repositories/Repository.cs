﻿namespace QOAM.Core.Repositories
{
    using System;
    using System.Configuration;
    using System.Data.Entity;

    using Validation;

    public abstract class Repository<T> : IDisposable
        where T : Entity
    {
        protected Repository(ApplicationDbContext dbContext)
        {
            Requires.NotNull(dbContext, "dbContext");

            this.DbContext = dbContext;
        }

        protected ApplicationDbContext DbContext { get; private set; }

        protected static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            }
        }
        
        public void Save()
        {
            this.DbContext.SaveChanges();
        }

        public void Dispose()
        {
            this.DbContext.Dispose();
        }

        public void InsertOrUpdate(T entity)
        {
            Requires.NotNull(entity, "entity");

            if (entity.Id == default(int))
            {
                this.DbContext.Set<T>().Add(entity);
            }
            else
            {
                this.DbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Delete(T entity)
        {
            Requires.NotNull(entity, "entity");

            if (entity.Id == default(int))
            {
                return;
            }

            this.DbContext.Set<T>().Remove(entity);
        }
    }
}