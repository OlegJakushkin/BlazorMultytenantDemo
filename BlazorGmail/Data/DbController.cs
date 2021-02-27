using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BlazorMultytenantDemo.Data
{
    public class DbController
    {
        private readonly DbContext dbContext;
        private async Task AddOrgAsync(Org org)
        {
            dbContext.Orgs.Add(org);
            await dbContext.SaveChangesAsync();
        }
        private async Task AddUserAsync(User usr)
        {
            dbContext.Users.Add(usr);
            await dbContext.SaveChangesAsync();
        }
        public DbController(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        //------------ Orgs ------------
        public async Task<User> IsThisAUser(string queryName)
        {
            var rels = await dbContext.Relations.ToListAsync();
            var usr= await dbContext.Users.FirstOrDefaultAsync(user => user.Name == queryName);
            return usr;
        }


        public async Task<List<Org>> GetUserOrgs(string queryName)
        {
            var usr = await IsThisAUser(queryName);

            if (usr == null)
            {
                return null;
            }
            var rels = dbContext.Relations
                .Where(relation => relation.UserId == usr.Id)
                .ToList();

            var orgs = await dbContext.Orgs.ToListAsync();

            return orgs
                    .Where(o=> rels.Any(r=> r.OrgId == o.Id))
                .ToList();
        }
        public async Task<List<User>> GetOrgUsersAsync(Org queryOrg)
        {
            var rels = await dbContext.Relations
                .Where(relation => relation.OrgId == queryOrg.Id)
                .ToListAsync();

            var users = await dbContext.Users.ToListAsync();

            return users.
                Where(u => rels.Any(relation => relation.UserId == u.Id))
                .ToList();
        }
        public async Task AddUserOrgAsync(string querySource)
        {
            var usr = await IsThisAUser(querySource);
            if (usr != null){
                return;
            }

            var userExist = dbContext
                .Users
                .FirstOrDefault(p => p.Name == querySource);

            var targetOrg = dbContext.Orgs
                .FirstOrDefault(org=>
                org.AdminName == querySource);

            if (targetOrg != null || userExist != null)
            {
                return;
            }

            var newUser = new User()
            {
                Name = querySource
            }; 
            await AddUserAsync(newUser);

            var newOrg = new Org()
            {
                AdminName = querySource
            };
            AddOrgAsync(newOrg);
            userExist = dbContext
                .Users
                .FirstOrDefault(p => p.Name == querySource);

            targetOrg = dbContext.Orgs
                .FirstOrDefault(org =>
                    org.AdminName == querySource);

            await UpdateUserOrgAsync(userExist, targetOrg);
        }

        public async Task UpdateUserOrgAsync(User usr, Org org)
        {
            var userExist = dbContext
                .Users
                .FirstOrDefault(p => p.Id == usr.Id);

            var orgExist = dbContext
                .Orgs
                .FirstOrDefault(p => p.Id == org.Id);

            if (userExist != null && orgExist != null)
            {
                var rel = new Relation()
                {
                    UserId = userExist.Id,
                    OrgId = orgExist.Id,
                };
                dbContext.Relations.Add(rel);
                await dbContext.SaveChangesAsync();
                var rels = await dbContext.Relations.ToListAsync();
                var a = 999;

            }
        }


        public async Task DeleteUserFromOrgAsync(string querySource, Org updateOrg, User userToRemove)
        {
            if (updateOrg.AdminName == querySource
                && userToRemove.Name != querySource) // TODO: check in the UI!
            {

                var rel = dbContext.Relations.First(relation =>
                    relation.OrgId == updateOrg.Id && relation.UserId == userToRemove.Id);
                if (rel != null)
                {
                    dbContext.Relations.Remove(rel);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Relation>> GetRelationsAsync()
        {
            return await dbContext.Relations.ToListAsync();
        }

    }
}