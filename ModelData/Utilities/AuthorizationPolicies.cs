using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ModelData.Utilities
{

    public class UserRolePolicy : AuthorizationHandler<UserLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserLevelRequirement requirement)
        {
            //role-based validation
            if (context.User.IsInRole(requirement.Role))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class NoUserRolePolicy : AuthorizationHandler<UserLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserLevelRequirement requirement)
        {
            //role-based validation
            if (!context.User.IsInRole(requirement.Role))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class MinimumUserRolePolicy : AuthorizationHandler<UserLevelRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserLevelRequirement requirement)
        {
            //role-based validation

            bool check = context.User.IsInRole(requirement.Role);

            if (!check)
            {
                switch (requirement.Role)
                {
                    case RuoliUtente.REGISTERING:
                        check |= context.User.IsInRole(RuoliUtente.REGISTERING);
                        goto case RuoliUtente.UTENTE;
                    case RuoliUtente.UTENTE:
                        check |= context.User.IsInRole(RuoliUtente.UTENTE);
                        goto case RuoliUtente.AMMINISTRATORE;
                    case RuoliUtente.AMMINISTRATORE:
                        check |= context.User.IsInRole(RuoliUtente.AMMINISTRATORE);
                        goto case RuoliUtente.DIGICORP;
                    case RuoliUtente.DIGICORP:
                        check |= context.User.IsInRole(RuoliUtente.DIGICORP);
                        break;
                    default:
                        break;
                }
            }

            if (check)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }


    public class UserLevelMatchRequirement : UserLevelRequirement
    {
        public UserLevelMatchRequirement(string role) : base(role)
        {

        }
    }
    public class UserLevelNoMatchRequirement : UserLevelRequirement
    {
        public UserLevelNoMatchRequirement(string role) : base(role)
        {

        }
    }

    public class UserLevelMinimumRequirement : UserLevelRequirement
    {
        public UserLevelMinimumRequirement(string role) : base(role)
        {

        }
    }
    public abstract class UserLevelRequirement : IAuthorizationRequirement
    {
        public UserLevelRequirement(string role)
        {
            Role = role;
        }

        public string Role { get; set; }

    }

}

