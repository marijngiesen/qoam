﻿namespace QOAM.Website.Controllers
{
    using System;
    using System.Linq;
    using System.Net.Mail;
    using System.Web.Mvc;
    using System.Web.Security;

    using AttributeRouting;
    using AttributeRouting.Web.Mvc;

    using Postal;

    using QOAM.Core.Repositories;
    using QOAM.Website.Helpers;
    using QOAM.Website.Models;
    using QOAM.Website.ViewModels.Account;

    using Validation;

    [RoutePrefix("")]
    [RequireHttps]
    public class AccountController : ApplicationController
    {
        private readonly IAuthentication authentication;

        private readonly IInstitutionRepository institutionRepository;

        private readonly IUserProfileRepository userProfileRepository;

        public AccountController(IUserProfileRepository userProfileRepository, IAuthentication authentication, IInstitutionRepository institutionRepository)
            : base(userProfileRepository, authentication)
        {
            Requires.NotNull(authentication, "authentication");
            Requires.NotNull(userProfileRepository, "userProfileRepository");
            Requires.NotNull(institutionRepository, "institutionRepository");

            this.authentication = authentication;
            this.userProfileRepository = userProfileRepository;
            this.institutionRepository = institutionRepository;
        }

        [GET("login")]
        public ViewResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;

            return this.View();
        }

        [POST("login")]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (this.ModelState.IsValid)
            {
                var userProfile = this.userProfileRepository.FindByEmail(model.Email);
                if (userProfile != null && this.authentication.Login(userProfile.UserName, model.Password, model.RememberMe))
                {
                    return this.RedirectToLocal(returnUrl);
                }
            }

            this.ModelState.AddModelError("", "The email address or password provided is incorrect.");

            return this.View(model);
        }

        [GET("logout")]
        public RedirectToRouteResult Logout()
        {
            this.authentication.Logout();

            return this.RedirectToAction("Index", "Home");
        }

        [GET("register")]
        public ActionResult Register()
        {
            return this.View();
        }

        [POST("register")]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var emailExists = this.userProfileRepository.FindByEmail(model.Email);
            if (emailExists != null)
            {
                this.ModelState.AddModelError("", "Sorry, this email address is already in our database.");
                return this.View(model);
            }

            var mailAddress = new MailAddress(model.Email);
            var institution = this.institutionRepository.All.FirstOrDefault(i => i.ShortName == mailAddress.Host);
            if (institution == null)
            {
                this.ModelState.AddModelError("", "Sorry, the domain name in your email address does not match the name of an academic institution known to us. If you want your institution to be included in our list, please enter it’s name and web address in our contact box and we will respond promptly.");
                return this.View(model);
            }

            model.InstitutionId = institution.Id;
            model.DisplayName = model.UserName;
            model.DateRegistered = DateTime.Now;

            try
            {
                var confirmationToken = this.authentication.CreateUserAndAccount(model.UserName, model.Password, new { model.Email, model.DisplayName, model.DateRegistered, model.InstitutionId, model.OrcId });

                dynamic email = new Email("RegistrationEmail");
                email.To = model.Email;
                email.DisplayName = model.DisplayName;
                email.Url = this.Url.Action("RegisterConfirmation", "Account", new { token = confirmationToken }, this.Request.Url.Scheme);
                email.Send();

                return this.RedirectToAction("RegistrationPending");
            }
            catch (MembershipCreateUserException)
            {
                this.ModelState.AddModelError("", "Can't create the user.");
            }

            return View(model);
        }

        [GET("registrationpending")]
        public ActionResult RegistrationPending()
        {
            return this.View();
        }

        [GET("registerconfirmation/{token}")]
        public ActionResult RegisterConfirmation(string token)
        {
            if (!this.Authentication.ConfirmAccount(token))
            {
                return this.RedirectToAction("RegisterFailure");
            }

            return this.RedirectToAction("RegisterSuccess");
        }

        [GET("registersuccess")]
        public ActionResult RegisterSuccess()
        {
            return this.View();
        }

        [GET("registerfailure")]
        public ActionResult RegisterFailure()
        {
            return this.View();
        }

        [GET("changepassword")]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        [POST("changepassword")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (this.ModelState.IsValid && this.authentication.ChangePassword(this.authentication.CurrentUserName, model.OldPassword, model.NewPassword))
            {
                return this.RedirectToAction("PasswordChanged");
            }

            this.ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");

            return this.View(model);
        }

        [GET("passwordchanged")]
        [Authorize]
        public ViewResult PasswordChanged()
        {
            return this.View();
        }

        [GET("resetpassword")]
        public ActionResult ResetPassword()
        {
            return this.View();
        }

        [POST("resetpassword")]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            
            var userProfile = this.userProfileRepository.FindByEmail(model.Email);
            if (userProfile == null)
            {
                return this.RedirectToAction("ResetPasswordFailure");
            }
            
            var passwordResetToken = this.authentication.GeneratePasswordResetToken(userProfile.UserName);

            dynamic email = new Email("ResetPasswordEmail");
            email.To = userProfile.Email;
            email.DisplayName = userProfile.DisplayName;
            email.Url = this.Url.Action("ResetPasswordConfirmed", "Account", new { token = passwordResetToken }, this.Request.Url.Scheme);
            email.Send();

            return this.RedirectToAction("ResetPasswordPending");
        }

        [GET("resetpasswordpending")]
        public ViewResult ResetPasswordPending()
        {
            return this.View();
        }

        [GET("resetpasswordconfirmed")]
        public ActionResult ResetPasswordConfirmed(string token)
        {
            var model = new ResetPasswordConfirmedViewModel { Token = token };

            return this.View(model);
        }

        [POST("resetpasswordconfirmed")]
        public ActionResult ResetPasswordConfirmed(ResetPasswordConfirmedViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            return this.RedirectToAction(this.authentication.ResetPassword(model.Token, model.NewPassword) ? "ResetPasswordSuccess" : "ResetPasswordFailure");
        }

        [GET("resetpasswordsuccess")]
        public ViewResult ResetPasswordSuccess()
        {
            return this.View();
        }

        [GET("resetpasswordfailure")]
        public ViewResult ResetPasswordFailure()
        {
            return this.View();
        }

        [GET("generatepasswords")]
        [Authorize(Roles = ApplicationRole.Admin)]
        public ViewResult GeneratePasswords()
        {
            return this.View();
        }

        [POST("generatepasswords")]
        [Authorize(Roles = ApplicationRole.Admin)]
        public ActionResult GeneratePasswords(GeneratePasswordsViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            foreach (var user in this.userProfileRepository.All.Where(u => !this.authentication.UserIsConfirmed(u.UserName)))
            {
                var generatedPassword = "t3mp0r4ry";
                this.authentication.CreateAccount(user.UserName, generatedPassword);

                dynamic email = new Email("GeneratedPasswordEmail");
                email.To = user.Email;
                email.DisplayName = user.DisplayName;
                email.GeneratedPassword = generatedPassword;
                email.Url = this.Url.Action("ChangePassword", "Account", null, this.Request.Url.Scheme);
                email.Send();
            }

            return this.RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Home");
        }
    }
}