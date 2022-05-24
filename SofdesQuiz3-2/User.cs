using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SofdesQuiz3_2;

public class User
{
    public User(int id, string name, byte[] picture)
    {
        Id = id;
        Name = name;
        Picture = picture;
        var now = DateTimeOffset.Now;
    }

    public int Id { get; }
    public string Name { get; }
    public byte[] Picture { get; }

    public UserEntity ToUserEntity()
    {
        return new UserEntity()
        {
            Id = Id,
            Name = Name,
            Picture = Picture
        };
    }
}

[Table("Users")]
public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public byte[] Picture { get; set; }

    public User ToUser()
    {
        return new User(Id, Name, Picture);
    }
}

public class UsersContext : DbContext
{
    public DbSet<UserEntity> UserEntities { get; set; }

    public string DbPath { get; }

    public UsersContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "users.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={DbPath}");

}

public static class UsersDb
{
    public static User Get(int id)
    {
        var context = new UsersContext();
        var userEntity = context.UserEntities.Where(user => user.Id == id).FirstOrDefault();
        return userEntity?.ToUser();
    }
    public static List<User> GetAll()
    {
        var context = new UsersContext();
        var userEntities = context.UserEntities.ToList();
        return userEntities.Select(userEntity => userEntity.ToUser()).ToList();
    }

    public static void InsertUpdate(User user)
    {
        var context = new UsersContext();
        var userEntityOnDb = context.UserEntities.Where(userEntity => userEntity.Id == user.Id).FirstOrDefault();
        if (userEntityOnDb == null)
        {
            var userEntity = user.ToUserEntity();
            context.Add(userEntity);
            context.SaveChanges();
        }
        else
        {
            userEntityOnDb.Name = user.Name;
            userEntityOnDb.Picture = user.Picture;
            context.SaveChanges();
        }
    }

    public static bool Delete(int id)
    {
        var context = new UsersContext();
        var userEntityOnDb = context.UserEntities.Where(userEntity => userEntity.Id == id).FirstOrDefault();
        if (userEntityOnDb == null)
        {
            return false;
        }
        context.Remove(userEntityOnDb);
        context.SaveChanges();
        return true;
    }
}
