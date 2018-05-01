using System;
using System.Security;
using System.Text.RegularExpressions;
using Nancy;
using Nancy.ModelBinding;
using AuthN.Configuration;
using AuthN.Models.Requests.User;
using AuthN.Modules.Exceptions;
using AuthN.Services.Application;
using AuthN.Services.Auth;
using AuthN.Utilities;

namespace AuthN.Modules.Auth {
    public class AuthenticationModule : SBaseModule {
        private UserManagerService _userManager;

        public AuthenticationModule(ISContext serverContext) : base("/auth", serverContext) {
            Before += ctx => {
                _userManager = new UserManagerService(this.serverContext);
                return null;
            };

            // Register account
            Post("/register", async args => {
                var charsetRegex = new Regex(@"^[a-zA-Z0-9._-]{3,24}$");

                var req = this.Bind<UserRegistrationRequest>();

                try {
                    if (this.serverContext.configuration.maxUsers > -1 &&
                        _userManager.registeredUserCount >= this.serverContext.configuration.maxUsers) {
                        throw new SecurityException("Maximum number of users for this server reached");
                    }

                    // Validate username charset
                    if (charsetRegex.Matches(req.username).Count <= 0) {
                        throw new InvalidParameterException("Invalid username.");
                    }

                    // Validate password
                    if (req.password.Length < 8) {
                        throw new InvalidParameterException("Password must be at least 8 characters.");
                    }

                    if (req.password.Length > 128) {
                        throw new InvalidParameterException("Password may not exceed 128 characters.");
                    }

                    // Check invite key if enabled
                    if (this.serverContext.configuration.inviteRequired) {
                        // Validate invite key
                        if (!this.serverContext.appState.inviteKeys.Remove(req.inviteKey)) {
                            return HttpStatusCode.PaymentRequired;
                        }
                    }

                    // Attempt to register user
                    var newUser = await _userManager.registerUserAsync(req);

                    serverContext.log.writeLine($"Registered user {newUser.username} [{newUser.identifier}]",
                        SLogger.LogLevel.Information);

                    // queue persist
                    this.serverContext.appState.queuePersist();

                    // Return user details
                    return Response.asJsonNet(newUser);
                } catch (NullReferenceException) {
                    return HttpStatusCode.BadRequest;
                } catch (SecurityException sx) {
                    return Response.AsText(sx.Message)
                        .WithStatusCode(HttpStatusCode.Unauthorized);
                } catch (InvalidParameterException sx) {
                    return Response.AsText(sx.Message)
                        .WithStatusCode(HttpStatusCode.UnprocessableEntity);
                }
            });

            // Log in with username and password
            Post("/login", async args => {
                var req = this.Bind<UserLoginRequest>();
                var user = await _userManager.findUserByUsernameAsync(req.username);

                if (user == null) return HttpStatusCode.Unauthorized;

                try {
                    // Validate password
                    if (user.enabled && await _userManager.checkPasswordAsync(req.password, user)) {
                        // Return user details
                        return Response.asJsonNet(user);
                    }

                    return HttpStatusCode.Unauthorized;
                } catch (NullReferenceException) {
                    // A parameter was not provided
                    return HttpStatusCode.BadRequest;
                } catch (SecurityException ex) {
                    // Registration blocked for security reasons
                    return Response.AsText(ex.Message)
                        .WithStatusCode(HttpStatusCode.Unauthorized);
                }
            });

            Delete("/delete", async args => {
                // Login fields are the same as those for account deletion
                var req = this.Bind<UserLoginRequest>();

                var user = await _userManager.findUserByUsernameAsync(req.username);

                if (user == null) return HttpStatusCode.Unauthorized;

                try {
                    // Validate password
                    if (user.enabled && await _userManager.checkPasswordAsync(req.password, user)) {
                        // Password was correct, delete account
                        await _userManager.deleteUserAsync(user.identifier);

                        // queue persist
                        this.serverContext.appState.queuePersist();

                        return HttpStatusCode.NoContent;
                    }

                    return HttpStatusCode.Unauthorized;
                } catch { return HttpStatusCode.Unauthorized; }
            });

            // Allow changing passswords
            Patch("/changepassword", async args => {
                var req = this.Bind<UserPasswordChangeRequest>();
                var user = await _userManager.findUserByUsernameAsync(req.username);

                try {
                    // Validate password
                    if (req.newPassword.Length < 8) {
                        throw new InvalidParameterException("Password must be at least 8 characters.");
                    }

                    if (req.newPassword.Length > 128) {
                        throw new InvalidParameterException("Password may not exceed 128 characters.");
                    }

                    if (user.enabled && await _userManager.checkPasswordAsync(req.oldPassword, user)) {
                        // Update password
                        await _userManager.changeUserPasswordAsync(user, req.newPassword);
                        return HttpStatusCode.NoContent;
                    }

                    return HttpStatusCode.Unauthorized;
                } catch (NullReferenceException) {
                    // A parameter was not provided
                    return new Response().WithStatusCode(HttpStatusCode.BadRequest);
                } catch (SecurityException ex) {
                    // Registration blocked for security reasons
                    return Response.AsText(ex.Message)
                        .WithStatusCode(HttpStatusCode.Unauthorized);
                } catch (InvalidParameterException ex) {
                    return Response.AsText(ex.Message)
                        .WithStatusCode(HttpStatusCode.UnprocessableEntity);
                }
            });

            // Authenticate with API key (used to validate a key during auto-login)
            Post("/reauth", async args => {
                var req = this.Bind<UserReauthRequest>();
                var user = await _userManager.findUserByUsernameAsync(req.username);

                if (user == null) return HttpStatusCode.Unauthorized;

                try {
                    // Validate key
                    if (user.enabled && user.apiKey == req.apiKey) {
                        // Return user details
                        return Response.asJsonNet(user);
                    }

                    return HttpStatusCode.Unauthorized;
                } catch (NullReferenceException) {
                    // A parameter was not provided
                    return HttpStatusCode.BadRequest;
                } catch (SecurityException ex) {
                    // Blocked for security reasons
                    return Response.AsText(ex.Message)
                        .WithStatusCode(HttpStatusCode.Unauthorized);
                }
            });

            // Request generation of a new API key
            Patch("/newkey", async _ => {
                var req = this.Bind<UserKeyResetRequest>();
                var user = await _userManager.findUserByUsernameAsync(req.username);

                if (user == null) return HttpStatusCode.Unauthorized;

                try {
                    // Validate key
                    if (user.enabled && user.apiKey == req.apiKey) {
                        // Update key
                        await _userManager.generateNewApiKeyAsync(user);
                        return HttpStatusCode.NoContent;
                    }

                    return HttpStatusCode.Unauthorized;
                } catch (NullReferenceException) {
                    // A parameter was not provided
                    return HttpStatusCode.BadRequest;
                } catch (SecurityException ex) {
                    // Blocked for security reasons
                    return Response.AsText(ex.Message)
                        .WithStatusCode(HttpStatusCode.Unauthorized);
                }
            });
        }
    }
}