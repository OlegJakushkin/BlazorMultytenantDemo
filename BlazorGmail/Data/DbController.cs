using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BlazorGmail.Data
{
    public class DbController
    {
        private readonly DbContext dbContext;

        public DbController(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        //------------ Orgs ------------

        public async Task<List<Org>> GetUserOrgs(string queryName)
        {
            var rels = await dbContext.Relations
                .Where(relation => relation.User.Name == queryName)
                .ToListAsync();

            return rels.Select(relation => relation.Org).ToList();
        }


        public async Task<User> UpdateUserOrgAsync(User usr, Org org)
        {
            var userExist = dbContext
                .Users
                .FirstOrDefault(p => p.Id == usr.Id);

            var orgExist = dbContext
                .Orgs
                .FirstOrDefault(p => p.Id == usr.Id);

            if (userExist != null && orgExist != null)
            {
                var rel = new Relation()
                {
                    UserId = usr.Id,
                    OrgId = org.Id,
                    Org = org,
                    User = usr

                };
                dbContext.Relations.Add(rel);
                await dbContext.SaveChangesAsync();
            }

            return usr;
        }

        public async Task<List<User>> GetOrgUsersAsync(Org queryOrg)
        {
            var rels = await dbContext.Relations
                .Where(relation => relation.Org == queryOrg)
                .ToListAsync();

            return rels.Select(relation => relation.User).ToList();
        }

        private async Task<Org> AddOrgAsync(Org org)
        {
            dbContext.Orgs.Add(org);
            await dbContext.SaveChangesAsync();
            return org;
        }

        private async Task<User> AddUserAsync(User usr)
        {
            dbContext.Users.Add(usr);
            await dbContext.SaveChangesAsync();
            return usr;
        }

        public async Task AddUserOrgAsync(string querySource)
        {
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
            newUser  = await AddUserAsync(newUser);
            var newOrg = new Org()
            {
                AdminName = querySource
            };
            newOrg = await AddOrgAsync(newOrg);

            await UpdateUserOrgAsync(newUser, newOrg);
        }

        public async Task DeleteUserFromOrgAsync(string querySource, Org updateOrg, User userToRemove)
        {
            if (updateOrg.AdminName == querySource
                && userToRemove.Name != querySource) // TODO: check in the UI!
            {

                var rel = dbContext.Relations.First(relation =>
                    relation.Org == updateOrg && relation.User == userToRemove);
                if (rel != null)
                {
                    dbContext.Relations.Remove(rel);
                }
            }
        }
    }
}