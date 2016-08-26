namespace Nerd.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class NerdEntities : DbContext
    {
        // Your context has been configured to use a 'NerdModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'Nerd.Data.NerdModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'NerdModel' 
        // connection string in the application configuration file.
        public NerdEntities()
            : base("name=NerdDatabase")
        {

            this.Configuration.AutoDetectChangesEnabled = true;
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

         public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }

        public virtual DbSet<Post> Post { get; set; }
}

    
    public class User
    {
        [Key]
        public int Id { get; set; }
         
        public string Name { get; set; }
        public string UserName{ get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public DateTime? DateofBirth { get; set; }
        public string Country { get; set; }
        public string AuthenticationMethod { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Post> Posts{ get; set; }
    }


    public class Comment {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UserId")]
        public virtual User User {get;set;}
        public int UserId { get; set; }
        public  string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; }


    }

    public class Post {
        [Key]
        public int Id { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }


    public class Event {
        [Key]
        public int Id { get; set; }


    }




}