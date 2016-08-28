using Nerd.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nerd.Api.Repository
{
    public class UnitOfWork : IDisposable
    {

        private readonly NerdEntities _context = new NerdEntities();

        private GenericRepository<User> _userRepository;
        public GenericRepository<User> UserRepository
        {
            get
            {
                if (this._userRepository == null)
                {
                    this._userRepository = new GenericRepository<User>(_context);
                }
                return _userRepository;
            }
        }


        private GenericRepository<Comment> _commentsRepository;
        public GenericRepository<Comment> CommentsRepository
        {
            get
            {
                if (this._commentsRepository == null)
                {
                    this._commentsRepository = new GenericRepository<Comment>(_context);
                }
                return _commentsRepository;
            }
        }


        private GenericRepository<Event> _eventsRepository;
        public GenericRepository<Event> EventsRepository
        {
            get
            {
                if (this._eventsRepository == null)
                {
                    this._eventsRepository = new GenericRepository<Event>(_context);
                }
                return _eventsRepository;
            }
        }

        private GenericRepository<Competition> _competitionRepository;
        public GenericRepository<Competition> CompetitionRepository
        {
            get
            {
                if (this._competitionRepository == null)
                {
                    this._competitionRepository = new GenericRepository<Competition>(_context);
                }
                return _competitionRepository;
            }
        }

        private GenericRepository<CompetitionEntry> _competitionEntryRepository;
        public GenericRepository<CompetitionEntry> CompetitionEntryRepository
        {
            get
            {
                if (this._competitionEntryRepository == null)
                {
                    this._competitionEntryRepository = new GenericRepository<CompetitionEntry>(_context);
                }
                return _competitionEntryRepository;
            }
        }


        public void Save()
        {

            using (var context= new NerdEntities() )
            {
                var user = new User();
            }
            _context.SaveChanges();
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}