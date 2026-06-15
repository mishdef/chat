# Implement Google OAuth 2.0 Authorization

This plan outlines the steps to implement Google OAuth 2.0 for the OnlineChat application. The architecture will use Google Sign-In on the React frontend to obtain an ID token, which will be securely validated on the ASP.NET Core backend.

## User Review Required

> [!WARNING]  
> **Google Client ID Configuration:**
> You will need to create a Google Cloud Project, configure the OAuth consent screen, and create OAuth 2.0 Client IDs for a Web application. You must provide the generated **Client ID** to be placed in the frontend and backend configuration files.
> For the backend, we also need to add the Client ID to `appsettings.json` to validate that the token was issued for our application. 

## Open Questions

> [!IMPORTANT]
> 1. Do you already have a **Google Client ID** ready to use? If so, please provide it, otherwise I can use a placeholder for now (`YOUR_GOOGLE_CLIENT_ID`) and you can replace it later.
> 2. How should we handle the scenario where a user tries to sign in with Google, but an account with that email already exists from a previous regular email/password registration? Should we link the Google account and log them in, or return an error? (I recommend linking them automatically).

## Proposed Changes

---

### Backend (ASP.NET Core Web API)

#### [MODIFY] [ApiCustomerAuthService.cs](file:///C:/Users/Dell/source/repos/OnlineChat/SignalRProject/Services/ApiCustomerAuthService.cs)
- Add `GoogleLoginAsync` method to validate the Google ID token. If a user with the token's email exists, log them in. If not, return a special status indicating that the user needs to provide a username.
- Add `GoogleRegisterAsync` method to take the Google ID token and a provided `Username`, validate the token, and create the user in the database.

#### [MODIFY] [AuthController.cs](file:///C:/Users/Dell/source/repos/OnlineChat/SignalRProject/Controllers/AuthController.cs)
- Add `POST /api/auth/google-login` endpoint.
- Add `POST /api/auth/google-register` endpoint.

#### [MODIFY] [appsettings.json](file:///C:/Users/Dell/source/repos/OnlineChat/SignalRProject/appsettings.json)
- Add `GoogleSettings:ClientId` section to store the Client ID for validation.

#### [NEW] Dependencies
- Install the `Google.Apis.Auth` NuGet package to validate Google ID tokens securely on the server.

---

### Frontend (React with Vite)

#### [MODIFY] [package.json](file:///C:/Users/Dell/source/repos/OnlineChat/signalrreactfront/package.json)
- Add `@react-oauth/google` and `jwt-decode` dependencies.

#### [MODIFY] `main.jsx` (or `index.jsx`/`App.jsx` depending on structure)
- Wrap the application with `GoogleOAuthProvider`.

#### [MODIFY] Login/Register Components
- Add the "Sign in with Google" button using `@react-oauth/google`.
- Implement a modal or a separate screen to prompt for a **Username** if the backend indicates this is a new Google user.
- Add logic to call the new backend `/api/auth/google-login` and `/api/auth/google-register` endpoints.

## Verification Plan

### Automated Tests
- No automated tests are currently present, but the code will be compiled to ensure there are no build errors.

### Manual Verification
- Start the frontend and backend servers.
- Click the "Sign in with Google" button.
- Choose a Google account.
- Since it's the first time, verify that a prompt appears asking for a "Username".
- Enter a username and submit.
- Verify that the registration completes, a JWT token is received, and the user is logged into the chat application.
- Sign out and sign in with Google again.
- Verify that it logs in immediately without prompting for a username.
